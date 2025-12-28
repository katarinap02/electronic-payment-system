using WebShop.API.DTOs;

namespace WebShop.API.Services
{
    public interface IInsurancePackageService
    {
        List<InsurancePackageDto> GetAllPackages();
        InsurancePackageDto? GetPackageById(long id);
        List<InsurancePackageDto> GetActivePackages();
    }
}
