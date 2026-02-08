using Microsoft.AspNetCore.Mvc;
using Crypto.API.Services;

namespace Crypto.API.Controllers
{
    /// <summary>
    /// SIMPLE PAYMENT FLOW - BEZ BAZE
    /// Samo Web3/Blockchain validacija
    /// </summary>
    [ApiController]
    [Route("api/simple-payment")]
    public class SimplePaymentController : ControllerBase
    {
        private readonly Web3Service _web3Service;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SimplePaymentController> _logger;

        // In-memory "baza" za test (u produkciji bi bio PostgreSQL)
        private static readonly Dictionary<string, PaymentSession> _activeSessions = new();

        public SimplePaymentController(
            Web3Service web3Service,
            IConfiguration configuration,
            ILogger<SimplePaymentController> logger)
        {
            _web3Service = web3Service;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// STEP 1: Kreiranje payment sesije
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult> CreatePaymentSession([FromBody] CreateSimplePaymentRequest request)
        {
            try
            {
                var merchantWallet = _configuration["CryptoSettings:MerchantWalletAddress"]!;
                var exchangeRate = await _web3Service.GetEthToEurExchangeRateAsync();
                var amountInEth = request.AmountInEur / exchangeRate;

                var sessionId = Guid.NewGuid().ToString("N")[..12]; // Kratak ID
                var expiresAt = DateTime.UtcNow.AddMinutes(15);

                var session = new PaymentSession
                {
                    SessionId = sessionId,
                    CustomerWallet = request.CustomerWallet,
                    MerchantWallet = merchantWallet,
                    AmountInEur = request.AmountInEur,
                    AmountInEth = amountInEth,
                    ExchangeRate = exchangeRate,
                    Status = "PENDING",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt
                };

                _activeSessions[sessionId] = session;

                _logger.LogInformation(
                    "Payment session created: {SessionId}, Amount: {Eur} EUR = {Eth} ETH",
                    sessionId, request.AmountInEur, amountInEth);

                return Ok(new
                {
                    sessionId = sessionId,
                    customerWallet = request.CustomerWallet,
                    merchantWallet = merchantWallet,
                    amountInEur = request.AmountInEur,
                    amountInEth = amountInEth,
                    exchangeRate = exchangeRate,
                    status = "PENDING",
                    expiresAt = expiresAt,
                    instructions = new
                    {
                        step1 = "Open MetaMask with customer wallet",
                        step2 = $"Send {amountInEth:F8} ETH to merchant wallet",
                        step3 = $"Merchant wallet: {merchantWallet}",
                        step4 = "Copy transaction hash after sending",
                        step5 = $"Call /api/simple-payment/verify-payment with sessionId and txHash"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment session");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// STEP 2: Verifikacija pla?anja sa Transaction Hash-om
        /// </summary>
        [HttpPost("verify-payment")]
        public async Task<ActionResult> VerifyPayment([FromBody] VerifyPaymentRequest request)
        {
            try
            {
                if (!_activeSessions.TryGetValue(request.SessionId, out var session))
                {
                    return NotFound(new { error = "Payment session not found" });
                }

                if (session.Status == "SUCCESS")
                {
                    return Ok(new
                    {
                        status = "ALREADY_COMPLETED",
                        message = "This payment was already verified",
                        txHash = session.TransactionHash,
                        completedAt = session.CompletedAt
                    });
                }

                if (DateTime.UtcNow > session.ExpiresAt)
                {
                    session.Status = "EXPIRED";
                    return BadRequest(new
                    {
                        status = "EXPIRED",
                        error = "Payment session expired (15 minutes limit)"
                    });
                }

                // Validacija transakcije na blockchain-u
                _logger.LogInformation("Validating transaction {TxHash} for session {SessionId}",
                    request.TxHash, request.SessionId);

                var validation = await _web3Service.ValidateTransactionAsync(
                    request.TxHash,
                    session.MerchantWallet,
                    session.AmountInEth);

                if (!validation.IsValid)
                {
                    _logger.LogWarning("Transaction validation failed: {Error}", validation.ErrorMessage);
                    return BadRequest(new
                    {
                        status = "INVALID",
                        error = validation.ErrorMessage,
                        expectedAmount = session.AmountInEth,
                        merchantWallet = session.MerchantWallet
                    });
                }

                // Provera konfirmacija
                var confirmations = await _web3Service.GetTransactionConfirmationsAsync(request.TxHash);

                session.TransactionHash = request.TxHash;
                session.Confirmations = confirmations;

                if (confirmations >= 1)
                {
                    session.Status = "SUCCESS";
                    session.CompletedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Payment SUCCESS: Session {SessionId}, TxHash {TxHash}, Confirmations: {Confirmations}",
                        request.SessionId, request.TxHash, confirmations);

                    return Ok(new
                    {
                        status = "SUCCESS",
                        message = "Payment successfully verified!",
                        sessionId = session.SessionId,
                        txHash = session.TransactionHash,
                        confirmations = confirmations,
                        amountInEur = session.AmountInEur,
                        amountInEth = session.AmountInEth,
                        completedAt = session.CompletedAt,
                        etherscanUrl = $"https://sepolia.etherscan.io/tx/{request.TxHash}"
                    });
                }
                else
                {
                    session.Status = "CONFIRMING";

                    _logger.LogInformation(
                        "Payment CONFIRMING: Session {SessionId}, Confirmations: {Confirmations}/1",
                        request.SessionId, confirmations);

                    return Ok(new
                    {
                        status = "CONFIRMING",
                        message = "Transaction found, waiting for confirmations...",
                        sessionId = session.SessionId,
                        txHash = session.TransactionHash,
                        confirmations = confirmations,
                        requiredConfirmations = 1,
                        waitMessage = "Please wait 15-30 seconds and check again"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// STEP 3: Check payment status (polling)
        /// </summary>
        [HttpGet("status/{sessionId}")]
        public async Task<ActionResult> GetPaymentStatus(string sessionId)
        {
            try
            {
                if (!_activeSessions.TryGetValue(sessionId, out var session))
                {
                    return NotFound(new { error = "Payment session not found" });
                }

                // Ako je CONFIRMING, re-check confirmations
                if (session.Status == "CONFIRMING" && !string.IsNullOrEmpty(session.TransactionHash))
                {
                    var confirmations = await _web3Service.GetTransactionConfirmationsAsync(session.TransactionHash);
                    session.Confirmations = confirmations;

                    if (confirmations >= 1)
                    {
                        session.Status = "SUCCESS";
                        session.CompletedAt = DateTime.UtcNow;
                    }
                }

                return Ok(new
                {
                    sessionId = session.SessionId,
                    status = session.Status,
                    amountInEur = session.AmountInEur,
                    amountInEth = session.AmountInEth,
                    txHash = session.TransactionHash,
                    confirmations = session.Confirmations,
                    createdAt = session.CreatedAt,
                    completedAt = session.CompletedAt,
                    expiresAt = session.ExpiresAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Helper: Lista svih aktivnih sesija (debug)
        /// </summary>
        [HttpGet("sessions")]
        public ActionResult GetAllSessions()
        {
            return Ok(_activeSessions.Values.OrderByDescending(s => s.CreatedAt).ToList());
        }
    }

    // ===== DTOs =====
    public record CreateSimplePaymentRequest(string CustomerWallet, decimal AmountInEur);
    public record VerifyPaymentRequest(string SessionId, string TxHash);

    // ===== In-Memory Model =====
    public class PaymentSession
    {
        public string SessionId { get; set; } = string.Empty;
        public string CustomerWallet { get; set; } = string.Empty;
        public string MerchantWallet { get; set; } = string.Empty;
        public decimal AmountInEur { get; set; }
        public decimal AmountInEth { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Status { get; set; } = "PENDING"; // PENDING, CONFIRMING, SUCCESS, EXPIRED
        public string? TransactionHash { get; set; }
        public int Confirmations { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
