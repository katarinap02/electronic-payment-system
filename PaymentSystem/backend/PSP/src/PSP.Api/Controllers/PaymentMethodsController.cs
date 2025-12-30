using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSP.Application.Interfaces.Services;

namespace PSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IWebShopService _webShopService;

        public PaymentMethodsController(
            IPaymentMethodService paymentMethodService,
            IWebShopService webShopService)
        {
            _paymentMethodService = paymentMethodService;
            _webShopService = webShopService;
        }

        /// <summary>
        /// Get all payment methods
        /// </summary>
        /// <remarks>Required Role: None (Public endpoint)</remarks>
        [HttpGet]
        public async Task<IActionResult> GetAllPaymentMethods()
        {
            var result = await _paymentMethodService.GetAllPaymentMethodsAsync();
            return Ok(result.Value);
        }

        /// <summary>
        /// Get active payment methods only
        /// </summary>
        /// <remarks>Required Role: None (Public endpoint)</remarks>
        [HttpGet("active")]
        public async Task<IActionResult> GetActivePaymentMethods()
        {
            var result = await _paymentMethodService.GetActivePaymentMethodsAsync();
            return Ok(result.Value);
        }

        /// <summary>
        /// Get payment methods for specific WebShop
        /// </summary>
        /// <remarks>Required Role: SuperAdmin</remarks>
        [HttpGet("webshops/{webShopId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetWebShopPaymentMethods(int webShopId)
        {
            var result = await _webShopService.GetWebShopPaymentMethodsAsync(webShopId);
            
            if (result.IsFailure)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Add payment method to WebShop
        /// </summary>
        /// <remarks>Required Role: SuperAdmin</remarks>
        [HttpPost("webshops/{webShopId}/{paymentMethodId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AddPaymentMethodToWebShop(int webShopId, int paymentMethodId)
        {
            var result = await _webShopService.AddPaymentMethodToWebShopAsync(webShopId, paymentMethodId);
            
            if (result.IsFailure)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Payment method added successfully" });
        }

        /// <summary>
        /// Remove payment method from WebShop
        /// </summary>
        /// <remarks>Required Role: SuperAdmin</remarks>
        [HttpDelete("webshops/{webShopId}/{paymentMethodId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RemovePaymentMethodFromWebShop(int webShopId, int paymentMethodId)
        {
            var result = await _webShopService.RemovePaymentMethodFromWebShopAsync(webShopId, paymentMethodId);
            
            if (result.IsFailure)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return NoContent();
        }
    }
}
