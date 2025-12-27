using WebShop.API.Data;
using WebShop.API.Models;

namespace WebShop.API.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly AppDbContext _context;

        public VehicleRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Vehicle> GetAll()
        {
            return _context.Vehicles
                .OrderByDescending(v => v.CreatedAt)
                .ToList();
        }

        public Vehicle? GetById(long id)
        {
            return _context.Vehicles.Find(id);
        }

        public List<Vehicle> Search(
            VehicleCategory? category = null,
            TransmissionType? transmission = null,
            FuelType? fuelType = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? minSeats = null,
            int? maxSeats = null,
            VehicleStatus? status = null,
            string? brand = null,
            int? year = null)
        {
            var query = _context.Vehicles.AsQueryable();

            if (category.HasValue)
                query = query.Where(v => v.Category == category.Value);

            if (transmission.HasValue)
                query = query.Where(v => v.Transmission == transmission.Value);

            if (fuelType.HasValue)
                query = query.Where(v => v.FuelType == fuelType.Value);

            if (minPrice.HasValue)
                query = query.Where(v => v.PricePerDay >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(v => v.PricePerDay <= maxPrice.Value);

            if (minSeats.HasValue)
                query = query.Where(v => v.Seats >= minSeats.Value);

            if (maxSeats.HasValue)
                query = query.Where(v => v.Seats <= maxSeats.Value);

            if (status.HasValue)
                query = query.Where(v => v.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(brand))
                query = query.Where(v => v.Brand.ToLower().Contains(brand.ToLower()));

            if (year.HasValue)
                query = query.Where(v => v.Year == year.Value);

            return query.OrderByDescending(v => v.CreatedAt).ToList();
        }

        public Vehicle Create(Vehicle vehicle)
        {
            vehicle.CreatedAt = DateTime.UtcNow;
            vehicle.UpdatedAt = DateTime.UtcNow;
            _context.Vehicles.Add(vehicle);
            _context.SaveChanges();
            return vehicle;
        }

        public Vehicle? Update(Vehicle vehicle)
        {
            var existingVehicle = _context.Vehicles.Find(vehicle.Id);
            if (existingVehicle == null)
                return null;

            existingVehicle.Brand = vehicle.Brand;
            existingVehicle.Model = vehicle.Model;
            existingVehicle.Year = vehicle.Year;
            existingVehicle.Category = vehicle.Category;
            existingVehicle.PricePerDay = vehicle.PricePerDay;
            existingVehicle.Transmission = vehicle.Transmission;
            existingVehicle.FuelType = vehicle.FuelType;
            existingVehicle.Seats = vehicle.Seats;
            existingVehicle.ImageUrl = vehicle.ImageUrl;
            existingVehicle.Status = vehicle.Status;
            existingVehicle.Mileage = vehicle.Mileage;
            existingVehicle.Color = vehicle.Color;
            existingVehicle.Description = vehicle.Description;
            existingVehicle.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return existingVehicle;
        }

        public bool Delete(long id)
        {
            var vehicle = _context.Vehicles.Find(id);
            if (vehicle == null)
                return false;

            // Soft delete - change status to Unavailable
            vehicle.Status = VehicleStatus.Unavailable;
            vehicle.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
            return true;
        }

        public bool Exists(long id)
        {
            return _context.Vehicles.Any(v => v.Id == id);
        }

        public bool LicensePlateExists(string licensePlate)
        {
            return _context.Vehicles.Any(v => v.LicensePlate.ToLower() == licensePlate.ToLower());
        }
    }
}
