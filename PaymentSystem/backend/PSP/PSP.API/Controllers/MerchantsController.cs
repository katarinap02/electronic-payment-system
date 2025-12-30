using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSP.API.DTOs;
using PSP.API.Services.Interfaces;

namespace PSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebShopsController : ControllerBase
    {
        private readonly IWebShopService _webShopService;

        public WebShopsController(IWebShopService webShopService)
        {
            _webShopService = webShopService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllWebShops()
        {
            var result = await _webShopService.GetAllWebShopsAsync();
            return Ok(result.Value);
        }

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
    }
}
