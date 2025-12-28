using WebShop.API.Models;

namespace WebShop.API.Repositories
{
    public interface IInsurancePackageRepository
    {
        List<InsurancePackage> GetAll();
        InsurancePackage? GetById(long id);
        List<InsurancePackage> GetActive();
    }
}
