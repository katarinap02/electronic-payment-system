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
        private readonly NbsQrCodeService _qrCodeService;

        public PaymentController(
            PaymentService paymentService, 
            NbsQrCodeService qrCodeService)
        {
            _paymentService = paymentService;
            _qrCodeService = qrCodeService;
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
                return Unauthorized(new { error = "Invalid PSP credentials" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message }); // 409 za duplicate
            }
            catch (Exception ex)
            {

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
                return NotFound(new { error = "Payment not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
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
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = "Payment authorization failed" });
            }
            catch (Exception ex)
            {
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
                return NotFound(new { error = "Payment not found" });
            }
            catch (Exception ex)
            {
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
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Generiše NBS IPS QR kod za plaćanje
        /// </summary>
        [HttpPost("qr/generate/{paymentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateQrCode(string paymentId, [FromQuery] int size = 300)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentId))
                    return BadRequest(new { error = "Payment ID is required" });

                var qrCodeData = await _paymentService.GenerateQrCodeForPaymentAsync(paymentId, size);
                
                if (!qrCodeData.Success)
                    return BadRequest(new { error = qrCodeData.ErrorMessage });

                return Ok(qrCodeData);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = "Payment not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Proverava status QR plaćanja (polling endpoint)
        /// </summary>
        [HttpGet("qr/status/{paymentId}")]
        [AllowAnonymous]
        public IActionResult GetQrPaymentStatus(string paymentId)
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
                return NotFound(new { error = "Payment not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Simulira skeniranje QR koda i potvrđuje plaćanje
        /// (U produkciji, ovo bi bilo poziv iz mBanking aplikacije)
        /// </summary>
        [HttpPost("qr/confirm/{paymentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmQrPayment(string paymentId)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentId))
                    return BadRequest(new { error = "Payment ID is required" });

                var result = await _paymentService.ConfirmQrPayment(paymentId);
                
                if (!result.Success)
                    return BadRequest(new { error = result.ErrorMessage });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = "Payment not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        //mozda kasnije dodati capture sredstava
    }
}
