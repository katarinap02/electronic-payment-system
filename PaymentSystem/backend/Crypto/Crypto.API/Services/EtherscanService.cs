using Nethereum.Web3;
using System.Numerics;

namespace Crypto.API.Services
{
    public class EtherscanService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EtherscanService> _logger;
        private readonly string _apiUrl;
        private readonly string _apiKey;
        private readonly string _chainId;

        public EtherscanService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<EtherscanService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            _apiUrl = configuration["CryptoSettings:EtherscanApiUrl"] 
                ?? "https://api.etherscan.io/v2/api";
            _apiKey = configuration["CryptoSettings:EtherscanApiKey"] 
                ?? throw new Exception("Etherscan API Key not configured");
            _chainId = configuration["CryptoSettings:EtherscanChainId"] 
                ?? "11155111"; // Sepolia testnet by default
        }

        public async Task<(string? TxHash, decimal AmountInEth)?> FindIncomingTransactionAsync(
            string walletAddress,
            decimal expectedAmountInEth,
            DateTime createdAfter)
        {
            try
            {
                var expectedAmountInWei = Web3.Convert.ToWei(expectedAmountInEth);
                var tolerancePercent = decimal.Parse(_configuration["CryptoSettings:AmountTolerancePercent"] ?? "1.0");
                
                // Calculate tolerance in Wei
                var expectedInDecimal = (decimal)expectedAmountInWei;
                var toleranceInWei = expectedInDecimal * (tolerancePercent / 100m);
                var minAmount = new BigInteger(expectedInDecimal - toleranceInWei);
                var maxAmount = new BigInteger(expectedInDecimal + toleranceInWei);
                // Build Etherscan API URL
                var url = $"{_apiUrl}?apikey={_apiKey}&chainid={_chainId}&address={walletAddress}&action=txlist&module=account&startblock=0&endblock=9999999999&page=1&offset=20&sort=desc";
                
                _logger.LogInformation("Checking Etherscan for transactions to {Address}", walletAddress);

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Etherscan API returned {StatusCode}", response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonDocument.Parse(json);
                
                var status = data.RootElement.GetProperty("status").GetString();
                if (status != "1")
                {
                    _logger.LogWarning("Etherscan API error: {Message}", 
                        data.RootElement.GetProperty("message").GetString());
                    return null;
                }

                var transactions = data.RootElement.GetProperty("result");
                
                foreach (var tx in transactions.EnumerateArray())
                {
                    var toAddress = tx.GetProperty("to").GetString();
                    var valueStr = tx.GetProperty("value").GetString();
                    var timestampStr = tx.GetProperty("timeStamp").GetString();
                    var txHash = tx.GetProperty("hash").GetString();
                    var txStatus = tx.GetProperty("txreceipt_status").GetString();

                    // Skip failed transactions
                    if (txStatus != "1")
                        continue;

                    if (!System.Numerics.BigInteger.TryParse(valueStr, out var valueInWei))
                        continue;

                    if (!long.TryParse(timestampStr, out var timestamp))
                        continue;

                    var txDateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;

                    // Check if transaction is after payment creation
                    if (txDateTime < createdAfter)
                    {
                        _logger.LogDebug("Transaction {TxHash} is too old: {TxTime} < {CreatedTime}", 
                            txHash, txDateTime, createdAfter);
                        continue;
                    }

                    // Check if amount is within tolerance
                    if (valueInWei >= minAmount && valueInWei <= maxAmount)
                    {
                        var amountInEth = Web3.Convert.FromWei(valueInWei);
                        
                        _logger.LogInformation(
                            "âœ… Found matching transaction: {TxHash}, Amount: {Amount} ETH, Time: {Time}",
                            txHash, amountInEth, txDateTime);

                        return (txHash, (decimal)amountInEth);
                    }
                    else
                    {
                        _logger.LogDebug(
                            "Transaction {TxHash} amount mismatch: {Value} Wei (expected {Min}-{Max} Wei)",
                            txHash, valueInWei, minAmount, maxAmount);
                    }
                }

                _logger.LogInformation("No matching transaction found for {Address}", walletAddress);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Etherscan for address {Address}", walletAddress);
                return null;
            }
        }

        public async Task<int> GetTransactionConfirmationsAsync(string txHash)
        {
            try
            {
                var url = $"{_apiUrl}?apikey={_apiKey}&chainid={_chainId}&txhash={txHash}&action=eth_getTransactionReceipt&module=proxy";
                
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return 0;

                var json = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonDocument.Parse(json);
                
                if (!data.RootElement.TryGetProperty("result", out var result) || result.ValueKind == System.Text.Json.JsonValueKind.Null)
                    return 0;

                if (!result.TryGetProperty("blockNumber", out var blockNumberHex))
                    return 0;

                var blockNumberStr = blockNumberHex.GetString();
                if (string.IsNullOrEmpty(blockNumberStr))
                    return 0;

                var txBlockNumber = Convert.ToInt64(blockNumberStr, 16);

                // Get latest block number
                var latestBlockUrl = $"{_apiUrl}?apikey={_apiKey}&chainid={_chainId}&action=eth_blockNumber&module=proxy";
                var latestResponse = await _httpClient.GetAsync(latestBlockUrl);
                
                if (!latestResponse.IsSuccessStatusCode)
                    return 0;

                var latestJson = await latestResponse.Content.ReadAsStringAsync();
                var latestData = System.Text.Json.JsonDocument.Parse(latestJson);
                var latestBlockHex = latestData.RootElement.GetProperty("result").GetString();
                
                if (string.IsNullOrEmpty(latestBlockHex))
                    return 0;

                var latestBlockNumber = Convert.ToInt64(latestBlockHex, 16);
                var confirmations = (int)(latestBlockNumber - txBlockNumber) + 1;

                return confirmations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting confirmations for {TxHash}", txHash);
                return 0;
            }
        }
    }
}

