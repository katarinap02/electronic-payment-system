using WebShop.API.DTOs;
using WebShop.API.Models;
using WebShop.API.Repositories;

namespace WebShop.API.Services
{
    public class InsurancePackageService : IInsurancePackageService
    {
        private readonly IInsurancePackageRepository _repository;

        public InsurancePackageService(IInsurancePackageRepository repository)
        {
            _repository = repository;
        }

        public List<InsurancePackageDto> GetAllPackages()
        {
            var packages = _repository.GetAll();
            return packages.Select(MapToDto).ToList();
        }

        public InsurancePackageDto? GetPackageById(long id)
        {
            var package = _repository.GetById(id);
            return package == null ? null : MapToDto(package);
        }

        public List<InsurancePackageDto> GetActivePackages()
        {
            var packages = _repository.GetActive();
            return packages.Select(MapToDto).ToList();
        }

        private InsurancePackageDto MapToDto(InsurancePackage package)
        {
            return new InsurancePackageDto
            {
                Id = package.Id,
                Name = package.Name,
                Description = package.Description,
                PricePerDay = package.PricePerDay,
                CoverageLimit = package.CoverageLimit,
                Deductible = package.Deductible,
                IsActive = package.IsActive
            };
        }
    }
}
