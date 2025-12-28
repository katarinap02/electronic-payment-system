using WebShop.API.DTOs;
using WebShop.API.Models;
using WebShop.API.Repositories;

namespace WebShop.API.Services
{
    public class AdditionalServiceService : IAdditionalServiceService
    {
        private readonly IAdditionalServiceRepository _repository;

        public AdditionalServiceService(IAdditionalServiceRepository repository)
        {
            _repository = repository;
        }

        public List<AdditionalServiceDto> GetAllServices()
        {
            var services = _repository.GetAll();
            return services.Select(MapToDto).ToList();
        }

        public AdditionalServiceDto? GetServiceById(long id)
        {
            var service = _repository.GetById(id);
            return service == null ? null : MapToDto(service);
        }

        public List<AdditionalServiceDto> GetAvailableServices()
        {
            var services = _repository.GetAvailable();
            return services.Select(MapToDto).ToList();
        }

        private AdditionalServiceDto MapToDto(AdditionalService service)
        {
            return new AdditionalServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                PricePerDay = service.PricePerDay,
                IsAvailable = service.IsAvailable,
                IconUrl = service.IconUrl
            };
        }
    }
}
