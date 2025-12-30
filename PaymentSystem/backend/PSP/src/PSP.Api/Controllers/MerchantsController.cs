using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSP.Application.DTOs.WebShops;
using PSP.Application.Interfaces.Services;

namespace PSP.API.Controllers
{
    [ApiController]
    [Route("api/superadmin/webshops")]
    [Authorize(Roles = "SuperAdmin")]
    public class WebShopsController : ControllerBase
    {
        private readonly IWebShopService _webShopService;
        private readonly IWebShopAdminService _webShopAdminService;

        public WebShopsController(IWebShopService webShopService, IWebShopAdminService webShopAdminService)
        {
            _webShopService = webShopService;
            _webShopAdminService = webShopAdminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllWebShops()
        {
            var result = await _webShopService.GetAllWebShopsAsync();
            return Ok(result.Value);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWebShopById(int id)
        {
            var result = await _webShopService.GetWebShopByIdAsync(id);
            
            if (result.IsFailure)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWebShop([FromBody] CreateWebShopRequest request)
        {
            var result = await _webShopService.CreateWebShopAsync(request);
            
            if (result.IsFailure)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return CreatedAtAction(nameof(GetWebShopById), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWebShop(int id, [FromBody] UpdateWebShopRequest request)
        {
            var result = await _webShopService.UpdateWebShopAsync(id, request);
            
            if (result.IsFailure)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok(result.Value);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWebShop(int id)
        {
            var result = await _webShopService.DeleteWebShopAsync(id);
            
            if (result.IsFailure)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return NoContent();
        }

        [HttpPost("{webShopId}/admins/{userId}")]
        public async Task<IActionResult> AssignAdminToWebShop(int webShopId, int userId)
        {
            var result = await _webShopAdminService.AssignAdminToWebShopAsync(userId, webShopId);
            
            if (result.IsFailure)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Admin assigned successfully" });
        }

        [HttpDelete("{webShopId}/admins/{userId}")]
        public async Task<IActionResult> RemoveAdminFromWebShop(int webShopId, int userId)
        {
            var result = await _webShopAdminService.RemoveAdminFromWebShopAsync(userId, webShopId);
            
            if (result.IsFailure)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return NoContent();
        }

        [HttpGet("{webShopId}/admins")]
        public async Task<IActionResult> GetWebShopAdmins(int webShopId)
        {
            var result = await _webShopAdminService.GetWebShopAdminsAsync(webShopId);
            return Ok(result.Value);
        }
    }
}
