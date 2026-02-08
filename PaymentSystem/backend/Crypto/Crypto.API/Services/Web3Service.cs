using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using System.Numerics;

namespace Crypto.API.Services
{
    public class Web3Service
    {
        private readonly Web3 _web3;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Web3Service> _logger;
        private readonly string _merchantWalletAddress;

        public Web3Service(
            IConfiguration configuration,
            HttpClient httpClient,
            ILogger<Web3Service> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;

            var infuraUrl = _configuration["CryptoSettings:InfuraApiUrl"] 
                ?? throw new Exception("Infura API URL not configured");
            
            _merchantWalletAddress = _configuration["CryptoSettings:MerchantWalletAddress"] 
                ?? throw new Exception("Merchant Wallet Address not configured");

            _web3 = new Web3(infuraUrl);
        }

        public async Task<decimal> GetEthToEurExchangeRateAsync()
        {
            try
            {
                var apiUrl = _configuration["CryptoSettings:ExchangeRateApiUrl"] 
                    ?? "https://api.coingecko.com/api/v3/simple/price?ids=ethereum&vs_currencies=eur";

                var response = await _httpClient.GetAsync(apiUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Exchange rate API returned {StatusCode}, using fallback rate", response.StatusCode);
                    return 1680m; // Fallback
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonDocument.Parse(json);
                
                var rate = data.RootElement
                    .GetProperty("ethereum")
                    .GetProperty("eur")
                    .GetDecimal();

                _logger.LogInformation("Current ETH/EUR rate: {Rate}", rate);
                return rate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rate, using fallback");
                return 1680m; // Fallback rate
            }
        }

        public string GetMerchantWalletAddress()
        {
            return _merchantWalletAddress;
        }

        public async Task<BigInteger> GetWalletBalanceAsync(string address)
        {
            try
            {
                var balance = await _web3.Eth.GetBalance.SendRequestAsync(address);
                return balance.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet balance for {Address}", address);
                return BigInteger.Zero;
            }
        }

        public async Task<(string? TxHash, BigInteger Amount)?> FindIncomingTransactionAsync(
            string walletAddress, 
            decimal expectedAmountInEth)
        {
            try
            {
                var expectedAmountInWei = Web3.Convert.ToWei(expectedAmountInEth);
                var tolerancePercent = decimal.Parse(_configuration["CryptoSettings:AmountTolerancePercent"] ?? "1.0");
                
                // Calculate tolerance in decimal first, then convert to BigInteger
                var toleranceInWei = (decimal)expectedAmountInWei * (tolerancePercent / 100m);
                var tolerance = new BigInteger(toleranceInWei);

                var minAmount = expectedAmountInWei - tolerance;
                var maxAmount = expectedAmountInWei + tolerance;

                var latestBlockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                var blocksToCheck = 100;

                for (var i = 0; i < blocksToCheck; i++)
                {
                    var blockNumber = latestBlockNumber.Value - (ulong)i;
                    if (blockNumber < 0) break;

                    var block = await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(
                        new BlockParameter(new HexBigInteger(blockNumber)));

                    if (block?.Transactions == null) continue;

                    foreach (var tx in block.Transactions)
                    {
                        if (tx.To?.ToLower() == walletAddress.ToLower() &&
                            tx.Value.Value >= minAmount &&
                            tx.Value.Value <= maxAmount)
                        {
                            _logger.LogInformation(
                                "Found matching transaction: {TxHash}, Amount: {Amount} Wei", 
                                tx.TransactionHash, 
                                tx.Value.Value);

                            return (tx.TransactionHash, tx.Value.Value);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring blockchain for address {Address}", walletAddress);
                return null;
            }
        }

        public async Task<int> GetTransactionConfirmationsAsync(string txHash)
        {
            try
            {
                var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
                
                if (receipt == null || receipt.BlockNumber == null)
                    return 0;

                var latestBlockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                var confirmations = (int)(latestBlockNumber.Value - receipt.BlockNumber.Value) + 1;

                return confirmations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting confirmations for {TxHash}", txHash);
                return 0;
            }
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateTransactionAsync(
            string txHash, 
            string expectedWalletAddress, 
            decimal expectedAmountInEth)
        {
            try
            {
                var transaction = await _web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);
                
                if (transaction == null)
                    return (false, "Transaction not found on blockchain");

                if (transaction.To?.ToLower() != expectedWalletAddress.ToLower())
                    return (false, "Transaction recipient does not match");

                var expectedAmountInWei = Web3.Convert.ToWei(expectedAmountInEth);
                var tolerancePercent = decimal.Parse(_configuration["CryptoSettings:AmountTolerancePercent"] ?? "1.0");
                
                // Calculate tolerance in decimal first, then convert to BigInteger
                var toleranceInWei = (decimal)expectedAmountInWei * (tolerancePercent / 100m);
                var tolerance = new BigInteger(toleranceInWei);

                if (transaction.Value.Value < expectedAmountInWei - tolerance ||
                    transaction.Value.Value > expectedAmountInWei + tolerance)
                {
                    return (false, $"Amount mismatch. Expected: {expectedAmountInEth} ETH, Got: {Web3.Convert.FromWei(transaction.Value.Value)} ETH");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating transaction {TxHash}", txHash);
                return (false, $"Validation error: {ex.Message}");
            }
        }
    }
}
