namespace Crypto.API.Models
{
    public class AuditLog
    {
        public long Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? Result { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
