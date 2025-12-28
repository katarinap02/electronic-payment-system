using Microsoft.AspNetCore.Mvc;
using WebShop.API.Services;

namespace WebShop.API.Controllers
{
    [Route("api/insurance")]
    [ApiController]
    public class InsurancePackageController : ControllerBase
    {
        private readonly IInsurancePackageService _service;

        public InsurancePackageController(IInsurancePackageService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all insurance packages
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var packages = _service.GetAllPackages();
                return Ok(new
                {
                    success = true,
                    count = packages.Count,
                    data = packages
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
        /// Get insurance package by ID
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(long id)
        {
            try
            {
                var package = _service.GetPackageById(id);
                if (package == null)
                    return NotFound(new { success = false, message = "Insurance package not found." });

                return Ok(new
                {
                    success = true,
                    data = package
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
        /// Get only active insurance packages
        /// </summary>
        [HttpGet("active")]
        public IActionResult GetActive()
        {
            try
            {
                var packages = _service.GetActivePackages();
                return Ok(new
                {
                    success = true,
                    count = packages.Count,
                    data = packages
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
