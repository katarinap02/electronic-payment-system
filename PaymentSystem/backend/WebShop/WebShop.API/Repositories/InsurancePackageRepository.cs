using WebShop.API.Data;
using WebShop.API.Models;

namespace WebShop.API.Repositories
{
    public class InsurancePackageRepository : IInsurancePackageRepository
    {
        private readonly AppDbContext _context;

        public InsurancePackageRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<InsurancePackage> GetAll()
        {
            return _context.InsurancePackages
                .OrderBy(i => i.PricePerDay)
                .ToList();
        }

        public InsurancePackage? GetById(long id)
        {
            return _context.InsurancePackages.Find(id);
        }

        public List<InsurancePackage> GetActive()
        {
            return _context.InsurancePackages
                .Where(i => i.IsActive)
                .OrderBy(i => i.PricePerDay)
                .ToList();
        }
    }
}
