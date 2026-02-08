using Crypto.API.DTOs;
using Crypto.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Crypto.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CryptoController : ControllerBase
    {
        private readonly CryptoPaymentService _paymentService;
        private readonly Web3Service _web3Service;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CryptoController> _logger;

        public CryptoController(
            CryptoPaymentService paymentService,
            Web3Service web3Service,
            IConfiguration configuration,
            ILogger<CryptoController> logger)
        {
            _paymentService = paymentService;
            _web3Service = web3Service;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("test-connection")]
        public async Task<ActionResult> TestInfuraConnection()
        {
            try
            {
                var infuraUrl = _configuration["CryptoSettings:InfuraApiUrl"];
                var merchantWallet = _configuration["CryptoSettings:MerchantWalletAddress"];

                var rate = await _web3Service.GetEthToEurExchangeRateAsync();
                var balance = await _web3Service.GetWalletBalanceAsync(merchantWallet!);
                var balanceEth = Nethereum.Web3.Web3.Convert.FromWei(balance);

                return Ok(new
                {
                    status = "SUCCESS",
                    infuraUrl = infuraUrl,
                    merchantWallet = merchantWallet,
                    exchangeRate = $"1 ETH = {rate} EUR",
                    walletBalance = $"{balanceEth} ETH",
                    network = "Sepolia Testnet"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "ERROR",
                    message = ex.Message,
                    troubleshooting = new[]
                    {
                        "Check if Infura API key is valid",
                        "Check if Sepolia network is enabled on Infura",
                        "Check internet connection",
                        "Check if wallet address is valid Ethereum address"
                    }
                });
            }
        }

        [HttpPost("simulate-payment")]
        public async Task<ActionResult> SimulatePayment([FromBody] SimulatePaymentRequest request)
        {
            try
            {
                var merchantWallet = _configuration["CryptoSettings:MerchantWalletAddress"];
                var exchangeRate = await _web3Service.GetEthToEurExchangeRateAsync();
                var amountInEth = request.AmountInEur / exchangeRate;

                _logger.LogInformation("Simulating payment: {Amount} EUR = {EthAmount} ETH", 
                    request.AmountInEur, amountInEth);

                return Ok(new
                {
                    message = "Payment simulation created",
                    customerWallet = request.CustomerWallet,
                    merchantWallet = merchantWallet,
                    amountInEur = request.AmountInEur,
                    amountInEth = amountInEth,
                    exchangeRate = exchangeRate,
                    instructions = new
                    {
                        step1 = "Open MetaMask with customer wallet",
                        step2 = $"Send {amountInEth:F8} ETH to merchant wallet",
                        step3 = "Wait 10-30 seconds for blockchain confirmation",
                        step4 = $"Check transaction on: https://sepolia.etherscan.io/address/{merchantWallet}"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating payment");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("validate-transaction")]
        public async Task<ActionResult> ValidateTransaction([FromBody] ValidateTransactionRequest request)
        {
            try
            {
                var merchantWallet = _configuration["CryptoSettings:MerchantWalletAddress"];
                var exchangeRate = await _web3Service.GetEthToEurExchangeRateAsync();
                var expectedAmountInEth = request.ExpectedAmountInEur / exchangeRate;

                var validation = await _web3Service.ValidateTransactionAsync(
                    request.TxHash, 
                    merchantWallet!, 
                    expectedAmountInEth);

                if (validation.IsValid)
                {
                    var confirmations = await _web3Service.GetTransactionConfirmationsAsync(request.TxHash);
                    
                    return Ok(new
                    {
                        valid = true,
                        txHash = request.TxHash,
                        confirmations = confirmations,
                        status = confirmations >= 1 ? "CONFIRMED" : "PENDING",
                        merchantWallet = merchantWallet,
                        expectedAmountInEth = expectedAmountInEth
                    });
                }
                else
                {
                    return Ok(new
                    {
                        valid = false,
                        error = validation.ErrorMessage
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating transaction");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("create-payment")]
        public async Task<ActionResult<CreatePaymentResponse>> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var userAgent = Request.Headers["User-Agent"].ToString();

                _logger.LogInformation("Create crypto payment requested for PSP: {PspId}", request.PspTransactionId);

                var response = await _paymentService.CreatePaymentAsync(request, ipAddress, userAgent);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreatePayment endpoint");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("transaction/{cryptoPaymentId}")]
        public async Task<ActionResult<TransactionStatusResponse>> GetTransactionStatus(string cryptoPaymentId)
        {
            try
            {
                var transaction = await _paymentService.GetTransactionStatusAsync(cryptoPaymentId);

                if (transaction == null)
                    return NotFound(new { message = "Transaction not found" });

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction status");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("cancel/{cryptoPaymentId}")]
        public async Task<ActionResult> CancelPayment(string cryptoPaymentId)
        {
            try
            {
                await _paymentService.CancelTransactionAsync(cryptoPaymentId);
                return Ok(new { message = "Payment cancelled successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling payment");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("confirm/{cryptoPaymentId}")]
        public async Task<ActionResult> ConfirmPayment(string cryptoPaymentId, [FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Confirming payment: {CryptoPaymentId}, TxHash: {TxHash}", 
                    cryptoPaymentId, request.TxHash);

                await _paymentService.ConfirmTransactionAsync(cryptoPaymentId, request.TxHash);
                
                return Ok(new { 
                    message = "Payment confirmed successfully",
                    status = "COMPLETED"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public record SimulatePaymentRequest(string CustomerWallet, decimal AmountInEur);
    public record ValidateTransactionRequest(string TxHash, decimal ExpectedAmountInEur);
    public record ConfirmPaymentRequest(string TxHash);
}
