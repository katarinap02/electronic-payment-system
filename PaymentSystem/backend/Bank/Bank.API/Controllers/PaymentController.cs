using Bank.API.DTOs;
using Bank.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bank.API.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(PaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }
        [HttpPost("initiate")]
        [AllowAnonymous] //trebace posle neka autorizacija
        public IActionResult InitiatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = _paymentService.CreatePaymentUrl(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized PSP request: {ex.Message}");
                return Unauthorized(new { error = "Invalid PSP credentials" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid payment request: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Payment processing error: {ex.Message}");
                return Conflict(new { error = ex.Message }); // 409 za duplicate
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating payment");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("form/{paymentId}")]
        [AllowAnonymous] 
        public IActionResult GetPaymentForm(string paymentId)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentId))
                    return BadRequest(new { error = "Payment ID is required" });

                var form = _paymentService.GetPaymentForm(paymentId);
                return Ok(form);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid payment ID: {paymentId}, Error: {ex.Message}");
                return NotFound(new { error = "Payment not found" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Expired payment form: {paymentId}, Error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payment form: {paymentId}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("process")]
        [AllowAnonymous] 
        public IActionResult ProcessCardPayment([FromBody] CardInformation cardInfo)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = _paymentService.ProcessCardPayment(cardInfo);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid payment data: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Payment processing failed: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized card payment attempt: {ex.Message}");
                return Unauthorized(new { error = "Payment authorization failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing card payment");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("status/{paymentId}")]
        public IActionResult GetPaymentStatus(string paymentId)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentId))
                    return BadRequest(new { error = "Payment ID is required" });

                var status = _paymentService.GetPaymentStatus(paymentId);
                return Ok(status);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid payment ID: {paymentId}, Error: {ex.Message}");
                return NotFound(new { error = "Payment not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payment status: {paymentId}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        //otkazivanje placanja ako je potrebno
        [HttpPost("cancel/{paymentId}")]
        [AllowAnonymous]
        public IActionResult CancelPayment(string paymentId)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentId))
                    return BadRequest(new { error = "Payment ID is required" });

                var success = _paymentService.CancelPayment(paymentId);

                if (!success)
                    return BadRequest(new { error = "Cannot cancel payment" });

                return Ok(new { message = "Payment cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling payment: {paymentId}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        //mozda kasnije dodati capture sredstava
    }
}
