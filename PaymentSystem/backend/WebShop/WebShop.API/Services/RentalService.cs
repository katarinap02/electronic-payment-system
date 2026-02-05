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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RentalService(
            RentalRepository rentalRepo, 
            IVehicleRepository vehicleRepo,
            ILogger<RentalService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _rentalRepo = rentalRepo;
            _vehicleRepo = vehicleRepo;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Kreira novu kupovinu nakon uspešnog plaćanja
        /// </summary>
        public async Task<RentalDto> CreateRentalAsync(CreateRentalDto dto)
        {
            var correlationId = GetCorrelationId();
            var ipAddress = GetClientIp();
            var startTime = DateTime.UtcNow;

            _logger.LogInformation("[WEBSHOP-RENTAL] CREATE_ATTEMPT | Desc: Creating rental after payment | PaymentId: {PaymentId} | GlobalTxId: {GlobalTxId} | UserId: {UserId} | VehicleId: {VehicleId} | Amount: {Amount} {Currency} | CorrId: {CorrId} | IP: {IP}",
                dto.PaymentId, dto.GlobalTransactionId, dto.UserId, dto.VehicleId, dto.TotalPrice, dto.Currency, correlationId, ipAddress);
            try
            {
                // Proveri da li vozilo postoji
                var vehicle = _vehicleRepo.GetById(dto.VehicleId);
                if (vehicle == null)
                {
                    _logger.LogWarning("[WEBSHOP-RENTAL] CREATE_FAILED | Desc: Vehicle not found | PaymentId: {PaymentId} | VehicleId: {VehicleId} | FailReason: VEHICLE_NOT_FOUND | CorrId: {CorrId} | IP: {IP}",
                        dto.PaymentId, dto.VehicleId, correlationId, ipAddress);
                    throw new ArgumentException("Vehicle not found");
                }

                // Izračunaj broj dana
                var rentalDays = (dto.EndDate.Date - dto.StartDate.Date).Days;
                if (rentalDays <= 0)
                {
                    _logger.LogWarning("[WEBSHOP-RENTAL] CREATE_FAILED | Desc: Invalid rental dates | PaymentId: {PaymentId} | StartDate: {StartDate} | EndDate: {EndDate} | FailReason: INVALID_DATES | CorrId: {CorrId} | IP: {IP}",
                        dto.PaymentId, dto.StartDate, dto.EndDate, correlationId, ipAddress);
                    throw new ArgumentException("End date must be after start date");
                }
                // Proveri dostupnost vozila
                var isAvailable = await _rentalRepo.IsVehicleAvailableAsync(
                    dto.VehicleId, 
                    dto.StartDate, 
                    dto.EndDate);

                if (!isAvailable)
                {
                    _logger.LogWarning("[WEBSHOP-RENTAL] CREATE_FAILED | Desc: Vehicle not available | PaymentId: {PaymentId} | VehicleId: {VehicleId} | StartDate: {StartDate} | EndDate: {EndDate} | FailReason: VEHICLE_UNAVAILABLE | CorrId: {CorrId} | IP: {IP}",
                        dto.PaymentId, dto.VehicleId, dto.StartDate, dto.EndDate, correlationId, ipAddress);
                    throw new InvalidOperationException("Vehicle is not available for selected dates");
                }

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

                var duration = DateTime.UtcNow - startTime;

                _logger.LogInformation("[WEBSHOP-RENTAL] CREATED | Desc: Rental successfully created | RentalId: {RentalId} | PaymentId: {PaymentId} | GlobalTxId: {GlobalTxId} | UserId: {UserId} | VehicleId: {VehicleId} | RentalDays: {RentalDays} | TotalPrice: {TotalPrice} {Currency} | DurationMs: {DurationMs} | CorrId: {CorrId}",
                    created.Id, dto.PaymentId, dto.GlobalTransactionId, dto.UserId, dto.VehicleId, rentalDays, dto.TotalPrice, dto.Currency, duration.TotalMilliseconds, correlationId);

                return await MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WEBSHOP-RENTAL] CREATE_ERROR | Desc: Unexpected error creating rental | PaymentId: {PaymentId} | GlobalTxId: {GlobalTxId} | UserId: {UserId} | ErrorType: {ErrorType} | CorrId: {CorrId} | IP: {IP}",
                   dto.PaymentId, dto.GlobalTransactionId, dto.UserId, ex.GetType().Name, correlationId, ipAddress);
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
            var correlationId = GetCorrelationId();

            _logger.LogInformation("[WEBSHOP-RENTAL] STATUS_UPDATE_ATTEMPT | Desc: Updating rental status | RentalId: {RentalId} | NewStatus: {NewStatus} | CorrId: {CorrId}",
                id, dto.Status, correlationId);
            var rental = await _rentalRepo.GetByIdAsync(id);
            if (rental == null)
            {
                _logger.LogWarning("[WEBSHOP-RENTAL] STATUS_UPDATE_FAILED | Desc: Rental not found | RentalId: {RentalId} | CorrId: {CorrId}",
                    id, correlationId);
                return null;
            }
            var oldStatus = rental.Status;
            rental.Status = dto.Status;
            
            if (dto.Status == "Completed")
                rental.CompletedAt = DateTime.UtcNow;
            else if (dto.Status == "Cancelled")
                rental.CancelledAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(dto.Notes))
                rental.Notes = dto.Notes;

            await _rentalRepo.UpdateAsync(rental);

            _logger.LogInformation("[WEBSHOP-RENTAL] STATUS_UPDATED | Desc: Rental status changed | RentalId: {RentalId} | PaymentId: {PaymentId} | StatusChange: {OldStatus}->{NewStatus} | CorrId: {CorrId}",
                id, rental.PaymentId, oldStatus, dto.Status, correlationId);

            return await MapToDto(rental);
        }

        /// <summary>
        /// Otkazuje rental
        /// </summary>
        public async Task<bool> CancelRentalAsync(int id)
        {
            return await _rentalRepo.DeleteAsync(id);
        }

        private string GetCorrelationId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                ?? Guid.NewGuid().ToString("N")[..12];
        }

        private string GetClientIp()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return "internal";

            var forwarded = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded)) return forwarded.Split(',')[0].Trim();

            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
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





