using WebShop.API.DTOs;
using WebShop.API.Models;
using WebShop.API.Repositories;

namespace WebShop.API.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;

        public VehicleService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public List<VehicleDto> GetAllVehicles()
        {
            var vehicles = _vehicleRepository.GetAll();
            return vehicles.Select(MapToDto).ToList();
        }

        public VehicleDto? GetVehicleById(long id)
        {
            var vehicle = _vehicleRepository.GetById(id);
            return vehicle == null ? null : MapToDto(vehicle);
        }

        public List<VehicleDto> SearchVehicles(VehicleSearchDto searchDto)
        {
            var vehicles = _vehicleRepository.Search(
                category: searchDto.Category,
                transmission: searchDto.Transmission,
                fuelType: searchDto.FuelType,
                minPrice: searchDto.MinPrice,
                maxPrice: searchDto.MaxPrice,
                minSeats: searchDto.MinSeats,
                maxSeats: searchDto.MaxSeats,
                status: searchDto.Status,
                brand: searchDto.Brand,
                year: searchDto.Year
            );

            return vehicles.Select(MapToDto).ToList();
        }

        public VehicleDto CreateVehicle(CreateVehicleDto createDto)
        {
            // Validate license plate uniqueness
            if (_vehicleRepository.LicensePlateExists(createDto.LicensePlate))
                throw new Exception("License plate already exists.");

            var vehicle = new Vehicle
            {
                Brand = createDto.Brand.Trim(),
                Model = createDto.Model.Trim(),
                Year = createDto.Year,
                Category = createDto.Category,
                PricePerDay = createDto.PricePerDay,
                Transmission = createDto.Transmission,
                FuelType = createDto.FuelType,
                Seats = createDto.Seats,
                ImageUrl = createDto.ImageUrl?.Trim(),
                LicensePlate = createDto.LicensePlate.ToUpper().Trim(),
                Status = VehicleStatus.Available,
                Mileage = createDto.Mileage,
                Color = createDto.Color.Trim(),
                Description = createDto.Description?.Trim()
            };

            var createdVehicle = _vehicleRepository.Create(vehicle);
            return MapToDto(createdVehicle);
        }

        public VehicleDto? UpdateVehicle(long id, UpdateVehicleDto updateDto)
        {
            var vehicle = _vehicleRepository.GetById(id);
            if (vehicle == null)
                return null;

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(updateDto.Brand))
                vehicle.Brand = updateDto.Brand.Trim();

            if (!string.IsNullOrWhiteSpace(updateDto.Model))
                vehicle.Model = updateDto.Model.Trim();

            if (updateDto.Year.HasValue)
                vehicle.Year = updateDto.Year.Value;

            if (updateDto.Category.HasValue)
                vehicle.Category = updateDto.Category.Value;

            if (updateDto.PricePerDay.HasValue)
                vehicle.PricePerDay = updateDto.PricePerDay.Value;

            if (updateDto.Transmission.HasValue)
                vehicle.Transmission = updateDto.Transmission.Value;

            if (updateDto.FuelType.HasValue)
                vehicle.FuelType = updateDto.FuelType.Value;

            if (updateDto.Seats.HasValue)
                vehicle.Seats = updateDto.Seats.Value;

            if (updateDto.ImageUrl != null)
                vehicle.ImageUrl = updateDto.ImageUrl.Trim();

            if (updateDto.Status.HasValue)
                vehicle.Status = updateDto.Status.Value;

            if (updateDto.Mileage.HasValue)
                vehicle.Mileage = updateDto.Mileage.Value;

            if (!string.IsNullOrWhiteSpace(updateDto.Color))
                vehicle.Color = updateDto.Color.Trim();

            if (updateDto.Description != null)
                vehicle.Description = updateDto.Description.Trim();

            var updatedVehicle = _vehicleRepository.Update(vehicle);
            return updatedVehicle == null ? null : MapToDto(updatedVehicle);
        }

        public bool DeleteVehicle(long id)
        {
            return _vehicleRepository.Delete(id);
        }

        private VehicleDto MapToDto(Vehicle vehicle)
        {
            return new VehicleDto
            {
                Id = vehicle.Id,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Category = vehicle.Category.ToString(),
                PricePerDay = vehicle.PricePerDay,
                Transmission = vehicle.Transmission.ToString(),
                FuelType = vehicle.FuelType.ToString(),
                Seats = vehicle.Seats,
                ImageUrl = vehicle.ImageUrl,
                LicensePlate = vehicle.LicensePlate,
                Status = vehicle.Status.ToString(),
                Mileage = vehicle.Mileage,
                Color = vehicle.Color,
                Description = vehicle.Description
            };
        }
    }
}
