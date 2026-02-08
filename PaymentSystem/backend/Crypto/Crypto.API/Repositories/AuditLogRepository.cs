using Crypto.API.Data;
using Crypto.API.Models;

namespace Crypto.API.Repositories
{
    public class AuditLogRepository
    {
        private readonly CryptoDbContext _context;

        public AuditLogRepository(CryptoDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            string action,
            string? transactionId,
            string ipAddress,
            string result = "SUCCESS",
            string? details = null)
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
    }
}
