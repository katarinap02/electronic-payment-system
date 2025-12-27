using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebShop.API.DTOs;
using WebShop.API.Services;

namespace WebShop.API.Controllers
{
    [Route("api/vehicles")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        /// <summary>
        /// Get all vehicles
        /// </summary>
        [HttpGet]
        public IActionResult GetAllVehicles()
        {
            try
            {
                var vehicles = _vehicleService.GetAllVehicles();
                return Ok(new
                {
                    success = true,
                    count = vehicles.Count,
                    data = vehicles
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
        /// Get vehicle by ID
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetVehicleById(long id)
        {
            try
            {
                var vehicle = _vehicleService.GetVehicleById(id);
                if (vehicle == null)
                    return NotFound(new { success = false, message = "Vehicle not found." });

                return Ok(new
                {
                    success = true,
                    data = vehicle
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
        /// Search/Filter vehicles
        /// </summary>
        [HttpPost("search")]
        public IActionResult SearchVehicles([FromBody] VehicleSearchDto searchDto)
        {
            try
            {
                var vehicles = _vehicleService.SearchVehicles(searchDto);
                return Ok(new
                {
                    success = true,
                    count = vehicles.Count,
                    data = vehicles
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
        /// Create new vehicle (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateVehicle([FromBody] CreateVehicleDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data.", errors = ModelState });

                var vehicle = _vehicleService.CreateVehicle(createDto);
                return CreatedAtAction(nameof(GetVehicleById), new { id = vehicle.Id }, new
                {
                    success = true,
                    message = "Vehicle created successfully.",
                    data = vehicle
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
        /// Update vehicle (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateVehicle(long id, [FromBody] UpdateVehicleDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data.", errors = ModelState });

                var vehicle = _vehicleService.UpdateVehicle(id, updateDto);
                if (vehicle == null)
                    return NotFound(new { success = false, message = "Vehicle not found." });

                return Ok(new
                {
                    success = true,
                    message = "Vehicle updated successfully.",
                    data = vehicle
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
        /// Delete vehicle (Admin only) - Soft delete (sets status to Unavailable)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteVehicle(long id)
        {
            try
            {
                var result = _vehicleService.DeleteVehicle(id);
                if (!result)
                    return NotFound(new { success = false, message = "Vehicle not found." });

                return Ok(new
                {
                    success = true,
                    message = "Vehicle deleted successfully (status set to Unavailable)."
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
