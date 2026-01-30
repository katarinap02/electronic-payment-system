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
        [HttpGet("capture")]
        public async Task<ActionResult> CaptureOrder(
        [FromQuery] string token,        // PayPal Order ID
        [FromQuery] string pspTransactionId,  // Tvoj PSP ID
        [FromQuery] string? PayerID)     // PayPal ID korisnika neobavezno
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "callback";

                _logger.LogInformation("Capture callback received for PSP: {PspId}", pspTransactionId);

                // Pozovi PayPal da potvrdiš da je plaćeno
                var success = await _payPalService.CaptureOrderAsync(token, pspTransactionId, ipAddress);

                if (success)
                {
                    // Vrati na PSP sa statusom success
                    var redirectUrl = $"http://localhost:5174/payment/{pspTransactionId}?status=success&method=paypal";
                    return Redirect(redirectUrl);
                }
                else
                {
                    // Vrati na PSP sa statusom failed
                    var redirectUrl = $"http://localhost:5174/payment/{pspTransactionId}?status=failed&method=paypal";
                    return Redirect(redirectUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CaptureOrder callback");
                // I u slučaju greške redirectuj na failed
                var redirectUrl = $"http://localhost:5174/payment/{pspTransactionId}?status=error&method=paypal";
                return Redirect(redirectUrl);
            }
        }

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
