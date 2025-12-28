using WebShop.API.DTOs;

namespace WebShop.API.Services
{
    public interface IAdditionalServiceService
    {
        List<AdditionalServiceDto> GetAllServices();
        AdditionalServiceDto? GetServiceById(long id);
        List<AdditionalServiceDto> GetAvailableServices();
    }
}
