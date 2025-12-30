using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSP.Application.DTOs.WebShops;
using PSP.Application.Interfaces.Services;

namespace PSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebShopsController : ControllerBase
    {
        private readonly IWebShopService _webShopService;
        private readonly IWebShopAdminService _webShopAdminService;

        public WebShopsController(IWebShopService webShopService, IWebShopAdminService webShopAdminService)
        {
            _webShopService = webShopService;
            _webShopAdminService = webShopAdminService;
        }

        /// <summary>
        /// Get all WebShops
        /// </summary>
        /// <remarks>Required Role: SuperAdmin</remarks>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllWebShops()
        {
            var result = await _webShopService.GetAllWebShopsAsync();
            return Ok(result.Value);
        }

        /// <summary>
        /// Get WebShop by ID
        /// </summary>
        /// <remarks>Required Role: SuperAdmin</remarks>
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetWebShopById(int id)
        {
            var result = await _webShopService.GetWebShopByIdAsync(id);
            
            if (result.IsFailure)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Create new WebShop
        /// </summary>
        /// <remarks>Required Role: SuperAdmin</remarks>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateWebShop([FromBody] CreateWebShopRequest request)
        {
            var result = await _webShopService.CreateWebShopAsync(request);
            
            if (result.IsFailure)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return CreatedAtAction(nameof(GetWebShopById), new { id = result.Value!.Id }, result.Value);
        }

        /// <summary>
        /// Update WebShop
        /// </summary>
        /// <remarks>Required Role: SuperAdmin</remarks>
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateWebShop(int id, [FromBody] UpdateWebShopRequest request)
        {
            var result = await _webShopService.UpdateWebShopAsync(id, request);
            
            if (result.IsFailure)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Delete WebShop
        /// </summary>
        /// <remarks>Required Role: SuperAdmin</remarks>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteWebShop(int id)
        {
            var result = await _webShopService.DeleteWebShopAsync(id);
            
            if (result.IsFailure)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return NoContent();
        }

        /// <summary>
        /// Assign Admin user to WebShop
        /// </summary>
        /// <remarks>Required Role: SuperAdmin</remarks>
        [HttpPost("{webShopId}/admins/{userId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AssignAdminToWebShop(int webShopId, int userId)
        {
            var result = await _webShopAdminService.AssignAdminToWebShopAsync(userId, webShopId);
            
            if (result.IsFailure)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Admin assigned successfully" });
        }

        /// <summary>
        /// Remove Admin from WebShop
        /// </summary>
        /// <remarks>Required Role: SuperAdmin</remarks>
        [HttpDelete("{webShopId}/admins/{userId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RemoveAdminFromWebShop(int webShopId, int userId)
        {
            var result = await _webShopAdminService.RemoveAdminFromWebShopAsync(userId, webShopId);
            
            if (result.IsFailure)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return NoContent();
        }

        /// <summary>
        /// Get all Admins of a WebShop
        /// </summary>
        /// <remarks>Required Role: SuperAdmin</remarks>
        [HttpGet("{webShopId}/admins")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetWebShopAdmins(int webShopId)
        {
            var result = await _webShopAdminService.GetWebShopAdminsAsync(webShopId);
            return Ok(result.Value);
        }

        /// <summary>
        /// Get WebShops managed by current Admin user
        /// </summary>
        /// <remarks>Required Role: Admin</remarks>
        [HttpGet("my-webshops")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMyManagedWebShops()
        {
            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var result = await _webShopAdminService.GetUserManagedWebShopsAsync(userId);
            return Ok(result.Value);
        }
    }
}
