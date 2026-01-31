using Microsoft.AspNetCore.Mvc;
using PayPal.API.DTOs;
using PayPal.API.Service;

namespace PayPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayPalController : ControllerBase
    {
        private readonly PayPalService _payPalService;
        private readonly ILogger<PayPalController> _logger;

        public PayPalController(PayPalService payPalService, ILogger<PayPalController> logger)
        {
            _payPalService = payPalService;
            _logger = logger;
        }

        //ovo poziva psp
        [HttpPost("create-order")]
        public async Task<ActionResult<CreateOrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                // Uzmi IP adresu iz HTTP zahteva (za PCI DSS audit)
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var userAgent = Request.Headers["User-Agent"].ToString();

                _logger.LogInformation("Create order requested for PSP: {PspId}", request.PspTransactionId);

                var response = await _payPalService.CreateOrderAsync(request, ipAddress, userAgent);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateOrder endpoint");
                return BadRequest(new { error = ex.Message });
            }
        }

        // Callback sa PayPal-a kada korisnik plati (SuccessUrl)
        [HttpPost("capture-order")]
        public async Task<ActionResult> CaptureOrder([FromBody] CaptureOrderRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "psp-backend";
                var success = await _payPalService.CaptureOrderAsync(
                    request.PayPalOrderId,
                    request.PspTransactionId,
                    ipAddress
                );

                return Ok(new { success, status = success ? "COMPLETED" : "FAILED" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing order");
                return BadRequest(new { error = ex.Message });
            }
        }

        public record CaptureOrderRequest(string PayPalOrderId, string PspTransactionId);

        //provera statusa transakcije ako bude trebala
        [HttpGet("transaction/{pspTransactionId}")]
        public async Task<ActionResult> GetTransaction(string pspTransactionId)
        {
            try
            {
                var transaction = await _payPalService.GetTransactionAsync(pspTransactionId);

                if (transaction == null)
                    return NotFound(new { message = "Transaction not found" });

                // Ne vraća enkriptovane podatke 
                return Ok(new
                {
                    transaction.PspTransactionId,
                    transaction.Status,
                    transaction.Amount,
                    transaction.Currency,
                    transaction.CreatedAt,
                    transaction.CompletedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
