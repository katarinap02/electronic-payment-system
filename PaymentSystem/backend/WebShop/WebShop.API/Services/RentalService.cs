using WebShop.API.DTOs;
using WebShop.API.Models;
using WebShop.API.Repositories;
using System.Text.Json;

namespace WebShop.API.Services
{
    public class RentalService
    {
        private readonly RentalRepository _rentalRepo;
        private readonly IVehicleRepository _vehicleRepo;
        private readonly ILogger<RentalService> _logger;

        public RentalService(
            RentalRepository rentalRepo, 
            IVehicleRepository vehicleRepo,
            ILogger<RentalService> logger)
        {
            _rentalRepo = rentalRepo;
            _vehicleRepo = vehicleRepo;
            _logger = logger;
        }

        /// <summary>
        /// Kreira novu kupovinu nakon uspešnog plaćanja
        /// </summary>
        public async Task<RentalDto> CreateRentalAsync(CreateRentalDto dto)
        {
            try
            {
                // Proveri da li vozilo postoji
                var vehicle = _vehicleRepo.GetById(dto.VehicleId);
                if (vehicle == null)
                    throw new ArgumentException("Vehicle not found");

                // Izračunaj broj dana
                var rentalDays = (dto.EndDate.Date - dto.StartDate.Date).Days;
                if (rentalDays <= 0)
                    throw new ArgumentException("End date must be after start date");

                // Proveri dostupnost vozila
                var isAvailable = await _rentalRepo.IsVehicleAvailableAsync(
                    dto.VehicleId, 
                    dto.StartDate, 
                    dto.EndDate);

                if (!isAvailable)
                    throw new InvalidOperationException("Vehicle is not available for selected dates");

                // Kreiraj rental objekat
                var rental = new Rental
                {
                    UserId = dto.UserId,
                    VehicleId = dto.VehicleId,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    RentalDays = rentalDays,
                    
                    AdditionalServices = dto.AdditionalServices != null && dto.AdditionalServices.Any()
                        ? JsonSerializer.Serialize(dto.AdditionalServices)
                        : null,
                    AdditionalServicesPrice = dto.AdditionalServicesPrice,
                    
                    InsuranceType = dto.InsuranceType,
                    InsurancePrice = dto.InsurancePrice,
                    
                    VehiclePricePerDay = vehicle.PricePerDay,
                    TotalPrice = dto.TotalPrice,
                    
                    PaymentId = dto.PaymentId,
                    GlobalTransactionId = dto.GlobalTransactionId,
                    Currency = dto.Currency,
                    PaymentMethod = dto.PaymentMethod,
                    
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    Notes = dto.Notes
                };

                var created = await _rentalRepo.CreateAsync(rental);
                
                _logger.LogInformation($"Rental created: ID={created.Id}, User={dto.UserId}, Vehicle={dto.VehicleId}, PaymentId={dto.PaymentId}");

                return await MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating rental for payment: {dto.PaymentId}");
                throw;
            }
        }

        /// <summary>
        /// Vraća rental po ID-u
        /// </summary>
        public async Task<RentalDto?> GetRentalByIdAsync(int id)
        {
            var rental = await _rentalRepo.GetByIdAsync(id);
            return rental != null ? await MapToDto(rental) : null;
        }

        /// <summary>
        /// Vraća rental po PaymentId
        /// </summary>
        public async Task<RentalDto?> GetRentalByPaymentIdAsync(string paymentId)
        {
            var rental = await _rentalRepo.GetByPaymentIdAsync(paymentId);
            return rental != null ? await MapToDto(rental) : null;
        }

        /// <summary>
        /// Vraća aktivne rentale za korisnika
        /// </summary>
        public async Task<List<RentalDto>> GetActiveRentalsAsync(int userId)
        {
            var rentals = await _rentalRepo.GetActiveByUserIdAsync(userId);
            var dtos = new List<RentalDto>();
            
            foreach (var rental in rentals)
            {
                dtos.Add(await MapToDto(rental));
            }
            
            return dtos;
        }

        /// <summary>
        /// Vraća istoriju rentala za korisnika
        /// </summary>
        public async Task<List<RentalDto>> GetRentalHistoryAsync(int userId)
        {
            var rentals = await _rentalRepo.GetHistoryByUserIdAsync(userId);
            var dtos = new List<RentalDto>();
            
            foreach (var rental in rentals)
            {
                dtos.Add(await MapToDto(rental));
            }
            
            return dtos;
        }

        /// <summary>
        /// Vraća sve rentale za korisnika (aktivne + istorija)
        /// </summary>
        public async Task<List<RentalDto>> GetAllUserRentalsAsync(int userId)
        {
            var rentals = await _rentalRepo.GetByUserIdAsync(userId);
            var dtos = new List<RentalDto>();
            
            foreach (var rental in rentals)
            {
                dtos.Add(await MapToDto(rental));
            }
            
            return dtos;
        }

        /// <summary>
        /// Ažurira status rentala
        /// </summary>
        public async Task<RentalDto?> UpdateRentalStatusAsync(int id, UpdateRentalStatusDto dto)
        {
            var rental = await _rentalRepo.GetByIdAsync(id);
            if (rental == null)
                return null;

            rental.Status = dto.Status;
            
            if (dto.Status == "Completed")
                rental.CompletedAt = DateTime.UtcNow;
            else if (dto.Status == "Cancelled")
                rental.CancelledAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(dto.Notes))
                rental.Notes = dto.Notes;

            await _rentalRepo.UpdateAsync(rental);
            
            _logger.LogInformation($"Rental status updated: ID={id}, Status={dto.Status}");

            return await MapToDto(rental);
        }

        /// <summary>
        /// Otkazuje rental
        /// </summary>
        public async Task<bool> CancelRentalAsync(int id)
        {
            return await _rentalRepo.DeleteAsync(id);
        }

        /// <summary>
        /// Mapira Rental model u DTO
        /// </summary>
        private async Task<RentalDto> MapToDto(Rental rental)
        {
            List<string> services = new();
            if (!string.IsNullOrEmpty(rental.AdditionalServices))
            {
                try
                {
                    services = JsonSerializer.Deserialize<List<string>>(rental.AdditionalServices) ?? new();
                }
                catch
                {
                    // Fallback if JSON parsing fails
                    services = rental.AdditionalServices.Split(',').ToList();
                }
            }

            return new RentalDto
            {
                Id = rental.Id,
                UserId = rental.UserId,
                UserName = rental.User?.Name ?? "Unknown",
                UserEmail = rental.User?.Email ?? "Unknown",
                
                VehicleId = rental.VehicleId,
                VehicleBrand = rental.Vehicle?.Brand ?? "Unknown",
                VehicleModel = rental.Vehicle?.Model ?? "Unknown",
                VehicleCategory = rental.Vehicle?.Category.ToString() ?? "Unknown",
                LicensePlate = rental.Vehicle?.LicensePlate ?? "Unknown",
                
                StartDate = rental.StartDate,
                EndDate = rental.EndDate,
                RentalDays = rental.RentalDays,
                
                AdditionalServices = services,
                AdditionalServicesPrice = rental.AdditionalServicesPrice,
                
                InsuranceType = rental.InsuranceType,
                InsurancePrice = rental.InsurancePrice,
                
                VehiclePricePerDay = rental.VehiclePricePerDay,
                TotalPrice = rental.TotalPrice,
                
                PaymentId = rental.PaymentId,
                GlobalTransactionId = rental.GlobalTransactionId,
                Currency = rental.Currency,
                PaymentMethod = rental.PaymentMethod,
                
                Status = rental.Status,
                CreatedAt = rental.CreatedAt,
                CompletedAt = rental.CompletedAt,
                
                Notes = rental.Notes
            };
        }
    }
}





