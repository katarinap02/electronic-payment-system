using Microsoft.EntityFrameworkCore;
using PayPal.API.Data;
using PayPal.API.Models;

namespace PayPal.API.Repositories
{
    public class PaypalTransactionRepository
    {
        private readonly PayPalDbContext _context;

        public PaypalTransactionRepository(PayPalDbContext context)
        {
            _context = context;
        }

        public async Task<PaypalTransaction> CreateAsync(PaypalTransaction transaction)
        {
            _context.PaypalTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<PaypalTransaction?> GetByIdAsync(long id)
        {
            return await _context.PaypalTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<PaypalTransaction?> GetByPspTransactionIdAsync(string pspTransactionId)
        {
            return await _context.PaypalTransactions
                .FirstOrDefaultAsync(t => t.PspTransactionId == pspTransactionId);
        }

        public async Task<IEnumerable<PaypalTransaction>> GetByMerchantIdAsync(string merchantId)
        {
            return await _context.PaypalTransactions
                .Where(t => t.MerchantId == merchantId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(PaypalTransaction transaction)
        {
            _context.PaypalTransactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByPspTransactionIdAsync(string pspTransactionId)
        {
            return await _context.PaypalTransactions
                .AnyAsync(t => t.PspTransactionId == pspTransactionId);
        }
    }
}
