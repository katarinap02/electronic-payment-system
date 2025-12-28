using WebShop.API.Models;

namespace WebShop.API.Repositories
{
    public interface IVehicleRepository
    {
        List<Vehicle> GetAll();
        Vehicle? GetById(long id);
        List<Vehicle> Search(
            VehicleCategory? category = null,
            TransmissionType? transmission = null,
            FuelType? fuelType = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? minSeats = null,
            int? maxSeats = null,
            VehicleStatus? status = null,
            string? brand = null,
            int? year = null
        );
        Vehicle Create(Vehicle vehicle);
        Vehicle? Update(Vehicle vehicle);
        bool Delete(long id);
        bool Exists(long id);
        bool LicensePlateExists(string licensePlate);
    }
}
