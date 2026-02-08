using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Crypto.API.Controllers
{
    /// <summary>
    /// Etherscan API Test Controller
    /// Testira Sepolia Etherscan API za ?itanje balance-a i transakcija
    /// </summary>
    [ApiController]
    [Route("api/etherscan-test")]
    public class EtherscanTestController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EtherscanTestController> _logger;

        public EtherscanTestController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<EtherscanTestController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint za proveru balance-a na Sepolia testnetu
        /// </summary>
        [HttpGet("balance/{address}")]
        public async Task<ActionResult> GetBalance(string address)
        {
            try
            {
                var apiKey = _configuration["EtherscanSettings:ApiKey"];
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    return BadRequest(new
                    {
                        error = "Etherscan API key not configured",
                        message = "Add 'EtherscanSettings:ApiKey' to appsettings.json"
                    });
                }

                var client = _httpClientFactory.CreateClient();
                
                // Sepolia Etherscan API endpoint
                var url = $"https://api-sepolia.etherscan.io/api" +
                         $"?module=account" +
                         $"&action=balance" +
                         $"&address={address}" +
                         $"&tag=latest" +
                         $"&apikey={apiKey}";

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new
                    {
                        error = "Etherscan API request failed",
                        statusCode = response.StatusCode,
                        response = content
                    });
                }

                var result = JsonSerializer.Deserialize<EtherscanBalanceResponse>(content);

                if (result?.Status != "1")
                {
                    return BadRequest(new
                    {
                        error = "Etherscan API returned error",
                        message = result?.Message,
                        result = content
                    });
                }

                // Konvertuj WEI u ETH
                var balanceInWei = decimal.Parse(result.Result ?? "0");
                var balanceInEth = balanceInWei / 1_000_000_000_000_000_000m; // 10^18

                _logger.LogInformation("Balance retrieved for {Address}: {Balance} ETH", address, balanceInEth);

                return Ok(new
                {
                    address = address,
                    balanceInWei = balanceInWei.ToString("F0"),
                    balanceInEth = balanceInEth,
                    network = "Sepolia Testnet",
                    source = "Etherscan API",
                    etherscanUrl = $"https://sepolia.etherscan.io/address/{address}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving balance from Etherscan");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test endpoint za proveru transakcija na adresi
        /// </summary>
        [HttpGet("transactions/{address}")]
        public async Task<ActionResult> GetTransactions(string address, [FromQuery] int page = 1, [FromQuery] int offset = 10)
        {
            try
            {
                var apiKey = _configuration["EtherscanSettings:ApiKey"];
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    return BadRequest(new
                    {
                        error = "Etherscan API key not configured"
                    });
                }

                var client = _httpClientFactory.CreateClient();
                
                // Get list of normal transactions
                var url = $"https://api-sepolia.etherscan.io/api" +
                         $"?module=account" +
                         $"&action=txlist" +
                         $"&address={address}" +
                         $"&startblock=0" +
                         $"&endblock=99999999" +
                         $"&page={page}" +
                         $"&offset={offset}" +
                         $"&sort=desc" +
                         $"&apikey={apiKey}";

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { error = content });
                }

                var result = JsonSerializer.Deserialize<EtherscanTransactionListResponse>(content);

                if (result?.Status != "1")
                {
                    return BadRequest(new
                    {
                        error = "Etherscan API returned error",
                        message = result?.Message
                    });
                }

                // Format transakcije
                var transactions = result.Result?.Select(tx => new
                {
                    txHash = tx.Hash,
                    from = tx.From,
                    to = tx.To,
                    valueInWei = tx.Value,
                    valueInEth = decimal.Parse(tx.Value ?? "0") / 1_000_000_000_000_000_000m,
                    blockNumber = tx.BlockNumber,
                    timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(tx.TimeStamp ?? "0")).DateTime,
                    confirmations = tx.Confirmations,
                    isError = tx.IsError != "0",
                    etherscanUrl = $"https://sepolia.etherscan.io/tx/{tx.Hash}"
                }).ToList();

                return Ok(new
                {
                    address = address,
                    totalTransactions = transactions?.Count ?? 0,
                    page = page,
                    transactions = transactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions from Etherscan");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test endpoint za proveru specifi?ne transakcije
        /// </summary>
        [HttpGet("transaction/{txHash}")]
        public async Task<ActionResult> GetTransactionByHash(string txHash)
        {
            try
            {
                var apiKey = _configuration["EtherscanSettings:ApiKey"];
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    return BadRequest(new { error = "Etherscan API key not configured" });
                }

                var client = _httpClientFactory.CreateClient();
                
                // Get transaction receipt
                var url = $"https://api-sepolia.etherscan.io/api" +
                         $"?module=proxy" +
                         $"&action=eth_getTransactionByHash" +
                         $"&txhash={txHash}" +
                         $"&apikey={apiKey}";

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { error = content });
                }

                var result = JsonSerializer.Deserialize<JsonElement>(content);
                var tx = result.GetProperty("result");

                // Parse hex values
                var valueHex = tx.GetProperty("value").GetString() ?? "0x0";
                var blockNumberHex = tx.GetProperty("blockNumber").GetString() ?? "0x0";

                var valueInWei = Convert.ToInt64(valueHex, 16);
                var blockNumber = Convert.ToInt64(blockNumberHex, 16);

                return Ok(new
                {
                    txHash = txHash,
                    from = tx.GetProperty("from").GetString(),
                    to = tx.GetProperty("to").GetString(),
                    valueInWei = valueInWei,
                    valueInEth = (decimal)valueInWei / 1_000_000_000_000_000_000m,
                    blockNumber = blockNumber,
                    gas = Convert.ToInt64(tx.GetProperty("gas").GetString() ?? "0x0", 16),
                    gasPrice = Convert.ToInt64(tx.GetProperty("gasPrice").GetString() ?? "0x0", 16),
                    etherscanUrl = $"https://sepolia.etherscan.io/tx/{txHash}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction from Etherscan");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // ===== Response Models =====
    public class EtherscanBalanceResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? Result { get; set; }
    }

    public class EtherscanTransactionListResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public List<EtherscanTransaction>? Result { get; set; }
    }

    public class EtherscanTransaction
    {
        public string? BlockNumber { get; set; }
        public string? TimeStamp { get; set; }
        public string? Hash { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public string? Value { get; set; }
        public string? Gas { get; set; }
        public string? GasPrice { get; set; }
        public string? IsError { get; set; }
        public string? Confirmations { get; set; }
    }
}
