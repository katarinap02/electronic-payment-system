using WebShop.API.Models;

namespace WebShop.API.Repositories
{
    public interface IAdditionalServiceRepository
    {
        List<AdditionalService> GetAll();
        AdditionalService? GetById(long id);
        List<AdditionalService> GetAvailable();
    }
}
