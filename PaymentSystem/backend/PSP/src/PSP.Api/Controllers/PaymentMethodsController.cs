using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSP.Application.Interfaces.Services;

namespace PSP.API.Controllers
{
    [ApiController]
    [Route("api/superadmin/payment-methods")]
    [Authorize(Roles = "SuperAdmin")]
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

        [HttpGet]
        public async Task<IActionResult> GetAllPaymentMethods()
        {
            var result = await _paymentMethodService.GetAllPaymentMethodsAsync();
            return Ok(result.Value);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActivePaymentMethods()
        {
            var result = await _paymentMethodService.GetActivePaymentMethodsAsync();
            return Ok(result.Value);
        }

        [HttpGet("webshops/{webShopId}")]
        public async Task<IActionResult> GetWebShopPaymentMethods(int webShopId)
        {
            var result = await _webShopService.GetWebShopPaymentMethodsAsync(webShopId);
            
            if (result.IsFailure)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok(result.Value);
        }

        [HttpPost("webshops/{webShopId}/{paymentMethodId}")]
        public async Task<IActionResult> AddPaymentMethodToWebShop(int webShopId, int paymentMethodId)
        {
            var result = await _webShopService.AddPaymentMethodToWebShopAsync(webShopId, paymentMethodId);
            
            if (result.IsFailure)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Payment method added successfully" });
        }

        [HttpDelete("webshops/{webShopId}/{paymentMethodId}")]
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
