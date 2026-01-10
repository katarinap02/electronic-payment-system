using Microsoft.EntityFrameworkCore;
using WebShop.API.Data;
using WebShop.API.Models;

namespace WebShop.API.Repositories
{
    public class RentalRepository
    {
        private readonly AppDbContext _context;

        public RentalRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Kreira novu kupovinu/rental
        /// </summary>
        public async Task<Rental> CreateAsync(Rental rental)
        {
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();
            return rental;
        }

        /// <summary>
        /// Vraća rental po ID-u sa related data
        /// </summary>
        public async Task<Rental?> GetByIdAsync(int id)
        {
            return await _context.Rentals
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// Vraća rental po PaymentId
        /// </summary>
        public async Task<Rental?> GetByPaymentIdAsync(string paymentId)
        {
            return await _context.Rentals
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.PaymentId == paymentId);
        }

        /// <summary>
        /// Vraća sve rentale za korisnika
        /// </summary>
        public async Task<List<Rental>> GetByUserIdAsync(int userId)
        {
            return await _context.Rentals
                .Include(r => r.Vehicle)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Vraća aktivne rentale za korisnika
        /// </summary>
        public async Task<List<Rental>> GetActiveByUserIdAsync(int userId)
        {
            return await _context.Rentals
                .Include(r => r.Vehicle)
                .Where(r => r.UserId == userId && r.Status == "Active")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Vraća istoriju rentala (završeni i otkazani)
        /// </summary>
        public async Task<List<Rental>> GetHistoryByUserIdAsync(int userId)
        {
            return await _context.Rentals
                .Include(r => r.Vehicle)
                .Where(r => r.UserId == userId && 
                       (r.Status == "Completed" || r.Status == "Cancelled"))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Vraća sve rentale (admin)
        /// </summary>
        public async Task<List<Rental>> GetAllAsync(int skip = 0, int take = 50)
        {
            return await _context.Rentals
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// Ažurira rental
        /// </summary>
        public async Task<Rental> UpdateAsync(Rental rental)
        {
            _context.Rentals.Update(rental);
            await _context.SaveChangesAsync();
            return rental;
        }

        /// <summary>
        /// Briše rental (soft delete - menja status)
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var rental = await GetByIdAsync(id);
            if (rental == null) return false;

            rental.Status = "Cancelled";
            rental.CancelledAt = DateTime.UtcNow;
            await UpdateAsync(rental);
            return true;
        }

        /// <summary>
        /// Proverava da li vozilo ima aktivne rentale u datom periodu
        /// </summary>
        public async Task<bool> IsVehicleAvailableAsync(long vehicleId, DateTime startDate, DateTime endDate, int? excludeRentalId = null)
        {
            var query = _context.Rentals
                .Where(r => r.VehicleId == vehicleId && 
                           r.Status == "Active" &&
                           ((r.StartDate <= endDate && r.EndDate >= startDate)));

            if (excludeRentalId.HasValue)
            {
                query = query.Where(r => r.Id != excludeRentalId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}

