using Microsoft.AspNetCore.Mvc;
using WebShop.API.Services;

namespace WebShop.API.Controllers
{
    [Route("api/services")]
    [ApiController]
    public class AdditionalServiceController : ControllerBase
    {
        private readonly IAdditionalServiceService _service;

        public AdditionalServiceController(IAdditionalServiceService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all additional services
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var services = _service.GetAllServices();
                return Ok(new
                {
                    success = true,
                    count = services.Count,
                    data = services
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get additional service by ID
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(long id)
        {
            try
            {
                var service = _service.GetServiceById(id);
                if (service == null)
                    return NotFound(new { success = false, message = "Service not found." });

                return Ok(new
                {
                    success = true,
                    data = service
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get only available additional services
        /// </summary>
        [HttpGet("available")]
        public IActionResult GetAvailable()
        {
            try
            {
                var services = _service.GetAvailableServices();
                return Ok(new
                {
                    success = true,
                    count = services.Count,
                    data = services
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
