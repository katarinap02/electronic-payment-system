using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebShop.API.DTOs;
using WebShop.API.Services;

namespace WebShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalsController : ControllerBase
    {
        private readonly RentalService _rentalService;
        private readonly ILogger<RentalsController> _logger;

        public RentalsController(RentalService rentalService, ILogger<RentalsController> logger)
        {
            _rentalService = rentalService;
            _logger = logger;
        }

        /// <summary>
        /// Kreira novu kupovinu/rental (poziva se nakon uspešnog plaćanja)
        /// POST /api/rentals
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateRental([FromBody] CreateRentalDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var rental = await _rentalService.CreateRentalAsync(dto);
                return Ok(rental);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rental");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Vraća rental po ID-u
        /// GET /api/rentals/{id}
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRental(int id)
        {
            try
            {
                var rental = await _rentalService.GetRentalByIdAsync(id);
                if (rental == null)
                    return NotFound(new { error = "Rental not found" });

                return Ok(rental);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting rental: {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Vraća rental po PaymentId
        /// GET /api/rentals/payment/{paymentId}
        /// </summary>
        [HttpGet("payment/{paymentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRentalByPaymentId(string paymentId)
        {
            try
            {
                var rental = await _rentalService.GetRentalByPaymentIdAsync(paymentId);
                if (rental == null)
                    return NotFound(new { error = "Rental not found" });

                return Ok(rental);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting rental by payment: {paymentId}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Vraća aktivne rentale za korisnika
        /// GET /api/rentals/user/{userId}/active
        /// </summary>
        [HttpGet("user/{userId}/active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveRentals(int userId)
        {
            try
            {
                var rentals = await _rentalService.GetActiveRentalsAsync(userId);
                return Ok(rentals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting active rentals for user: {userId}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Vraća istoriju rentala za korisnika
        /// GET /api/rentals/user/{userId}/history
        /// </summary>
        [HttpGet("user/{userId}/history")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRentalHistory(int userId)
        {
            try
            {
                var rentals = await _rentalService.GetRentalHistoryAsync(userId);
                return Ok(rentals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting rental history for user: {userId}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Vraća sve rentale za korisnika (aktivne + istorija)
        /// GET /api/rentals/user/{userId}
        /// </summary>
        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllUserRentals(int userId)
        {
            try
            {
                var rentals = await _rentalService.GetAllUserRentalsAsync(userId);
                return Ok(rentals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting all rentals for user: {userId}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Ažurira status rentala
        /// PUT /api/rentals/{id}/status
        /// </summary>
        [HttpPut("{id}/status")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateRentalStatus(int id, [FromBody] UpdateRentalStatusDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var rental = await _rentalService.UpdateRentalStatusAsync(id, dto);
                if (rental == null)
                    return NotFound(new { error = "Rental not found" });

                return Ok(rental);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating rental status: {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Otkazuje rental
        /// DELETE /api/rentals/{id}
        /// </summary>
        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> CancelRental(int id)
        {
            try
            {
                var success = await _rentalService.CancelRentalAsync(id);
                if (!success)
                    return NotFound(new { error = "Rental not found" });

                return Ok(new { message = "Rental cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling rental: {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
