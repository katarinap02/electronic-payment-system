using Microsoft.EntityFrameworkCore;
using PayPal.API.Data;
using PayPal.API.Models;

namespace PayPal.API.Repositories
{
    public class AuditLogRepository
    {
        private readonly PayPalDbContext _context;

        public AuditLogRepository(PayPalDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string action, string? transactionId, string ipAddress, string result, string? details = null)
        {
            var log = new AuditLog
            {
                Action = action,
                TransactionId = transactionId,
                IpAddress = ipAddress,
                Result = result,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByTransactionIdAsync(string transactionId)
        {
            return await _context.AuditLogs
                .Where(l => l.TransactionId == transactionId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _context.AuditLogs
                .Where(l => l.Timestamp >= from && l.Timestamp <= to)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }
    }
}
