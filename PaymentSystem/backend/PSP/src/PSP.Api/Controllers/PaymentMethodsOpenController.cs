using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSP.Application.Interfaces.Services;

namespace PSP.API.Controllers
{
    [ApiController]
    [Route("api/payment-methods")]
    [AllowAnonymous]
    public class PaymentMethodsOpenController : ControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IWebShopService _webShopService;

        public PaymentMethodsOpenController(
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
    }
}
