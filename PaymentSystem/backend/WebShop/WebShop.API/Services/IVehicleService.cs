using WebShop.API.DTOs;

namespace WebShop.API.Services
{
    public interface IVehicleService
    {
        List<VehicleDto> GetAllVehicles();
        VehicleDto? GetVehicleById(long id);
        List<VehicleDto> SearchVehicles(VehicleSearchDto searchDto);
        VehicleDto CreateVehicle(CreateVehicleDto createDto);
        VehicleDto? UpdateVehicle(long id, UpdateVehicleDto updateDto);
        bool DeleteVehicle(long id);
    }
}
