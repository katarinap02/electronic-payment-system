using Bank.API.DTOs;
using Bank.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bank.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(PaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // PSP → Bank (Tabela 2)
        [HttpPost("initiate")]
        public IActionResult InitiatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                var response = _paymentService.InitiatePayment(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InitiatePayment");
                return BadRequest(new { error = ex.Message });
            }
        }

        // Frontend → Bank (forma za plaćanje)
        [HttpGet("{paymentId}")]
        public IActionResult GetPaymentForm(string paymentId)
        {
            try
            {
                var formData = _paymentService.GetPaymentForm(paymentId);
                return Ok(formData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaymentForm");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
