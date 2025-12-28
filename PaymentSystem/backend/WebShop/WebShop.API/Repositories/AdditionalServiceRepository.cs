using WebShop.API.Data;
using WebShop.API.Models;

namespace WebShop.API.Repositories
{
    public class AdditionalServiceRepository : IAdditionalServiceRepository
    {
        private readonly AppDbContext _context;

        public AdditionalServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<AdditionalService> GetAll()
        {
            return _context.AdditionalServices
                .OrderBy(s => s.Name)
                .ToList();
        }

        public AdditionalService? GetById(long id)
        {
            return _context.AdditionalServices.Find(id);
        }

        public List<AdditionalService> GetAvailable()
        {
            return _context.AdditionalServices
                .Where(s => s.IsAvailable)
                .OrderBy(s => s.Name)
                .ToList();
        }
    }
}
