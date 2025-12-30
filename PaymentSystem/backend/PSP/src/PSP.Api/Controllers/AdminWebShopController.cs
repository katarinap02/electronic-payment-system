using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSP.Application.Interfaces.Services;

namespace PSP.API.Controllers;

[ApiController]
[Route("api/admin/webshops")]
[Authorize(Roles = "Admin")]
public class AdminWebShopController : ControllerBase
{
    private readonly IWebShopService _webShopService;
    private readonly IWebShopAdminService _webShopAdminService;

    public AdminWebShopController(IWebShopService webShopService, IWebShopAdminService webShopAdminService)
    {
        _webShopService = webShopService;
        _webShopAdminService = webShopAdminService;
    }

    /// <summary>
    /// Add payment method to WebShop (Admin can only manage their own WebShops)
    /// </summary>
    /// <remarks>Required Role: Admin (must be admin of this WebShop)</remarks>
    [HttpPost("{webShopId}/payment-methods/{paymentMethodId}")]
    public async Task<IActionResult> AddPaymentMethod(int webShopId, int paymentMethodId)
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        
        var isAdmin = await _webShopAdminService.IsUserAdminOfWebShopAsync(userId, webShopId);
        if (!isAdmin)
        {
            return Forbid();
        }

        var result = await _webShopService.AddPaymentMethodToWebShopAsync(webShopId, paymentMethodId);
        
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = "Payment method added successfully" });
    }

    /// <summary>
    /// Remove payment method from WebShop (Admin can only manage their own WebShops)
    /// </summary>
    /// <remarks>Required Role: Admin (must be admin of this WebShop)</remarks>
    [HttpDelete("{webShopId}/payment-methods/{paymentMethodId}")]
    public async Task<IActionResult> RemovePaymentMethod(int webShopId, int paymentMethodId)
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        
        var isAdmin = await _webShopAdminService.IsUserAdminOfWebShopAsync(userId, webShopId);
        if (!isAdmin)
        {
            return Forbid();
        }

        var result = await _webShopService.RemovePaymentMethodFromWebShopAsync(webShopId, paymentMethodId);
        
        if (result.IsFailure)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    /// <summary>
    /// Get payment methods for WebShop (Admin can only view their own WebShops)
    /// </summary>
    /// <remarks>Required Role: Admin (must be admin of this WebShop)</remarks>
    [HttpGet("{webShopId}/payment-methods")]
    public async Task<IActionResult> GetWebShopPaymentMethods(int webShopId)
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        
        var isAdmin = await _webShopAdminService.IsUserAdminOfWebShopAsync(userId, webShopId);
        if (!isAdmin)
        {
            return Forbid();
        }

        var result = await _webShopService.GetWebShopPaymentMethodsAsync(webShopId);
        
        if (result.IsFailure)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Value);
    }
}
