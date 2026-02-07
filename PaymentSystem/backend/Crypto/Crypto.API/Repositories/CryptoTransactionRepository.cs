using Crypto.API.Data;
using Crypto.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Crypto.API.Repositories
{
    public class CryptoTransactionRepository
    {
        private readonly CryptoDbContext _context;

        public CryptoTransactionRepository(CryptoDbContext context)
        {
            _context = context;
        }

        public async Task<CryptoTransaction> CreateAsync(CryptoTransaction transaction)
        {
            _context.CryptoTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<CryptoTransaction?> GetByPspTransactionIdAsync(string pspTransactionId)
        {
            return await _context.CryptoTransactions
                .FirstOrDefaultAsync(t => t.PspTransactionId == pspTransactionId);
        }

        public async Task<CryptoTransaction?> GetByCryptoPaymentIdAsync(string cryptoPaymentId)
        {
            return await _context.CryptoTransactions
                .FirstOrDefaultAsync(t => t.CryptoPaymentId == cryptoPaymentId);
        }

        public async Task<List<CryptoTransaction>> GetPendingTransactionsAsync()
        {
            return await _context.CryptoTransactions
                .Where(t => (t.Status == CryptoTransaction.CryptoStatus.PENDING ||
                             t.Status == CryptoTransaction.CryptoStatus.CANCELLED) &&
                            t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }
        public async Task<List<CryptoTransaction>> GetConfirmingTransactionsAsync()
        {
            return await _context.CryptoTransactions
                .Where(t => t.Status == CryptoTransaction.CryptoStatus.CONFIRMING)
                .ToListAsync();
        }

        public async Task UpdateAsync(CryptoTransaction transaction)
        {
            _context.CryptoTransactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CryptoTransaction>> GetExpiredTransactionsAsync()
        {
            return await _context.CryptoTransactions
                .Where(t => t.Status == CryptoTransaction.CryptoStatus.PENDING && 
                           t.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();
        }
    }
}
