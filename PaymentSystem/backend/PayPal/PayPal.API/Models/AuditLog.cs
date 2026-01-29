namespace PayPal.API.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Šta se desilo (READ, CREATE, UPDATE, DELETE)
        public string Action { get; set; } = string.Empty;
        public string? TransactionId { get; set; }

        // Ko je pristupio 
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Rezultat (SUCCESS, FAILURE)
        public string Result { get; set; } = "SUCCESS";

        // Detalji (ako ima greške)
        public string? Details { get; set; }
    }
}
