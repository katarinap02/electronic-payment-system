using Microsoft.AspNetCore.Mvc;
using PSP.Application.DTOs.Auth;
using PSP.Application.Interfaces.Services;

namespace PSP.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            
            if (result.IsFailure)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            return Ok(result.Value);
        }
    }
}
