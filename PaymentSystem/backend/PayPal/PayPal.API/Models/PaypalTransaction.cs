namespace PayPal.API.Models
{
    public class PaypalTransaction
    {
        public long Id { get; set; }
        public string PspTransactionId { get; set; } = string.Empty;
        public string EncryptedPayPalOrderId { get; set; } = string.Empty;
        public string? MerchantId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public PaypalStatus Status { get; set; } = 0;
        public string? EncryptedPayerId { get; set; }
        public string? EncryptedCaptureId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // PCI DSS Zahtev 5.1 - Audit podaci (ko je inicirao transakciju)
        public string CreatedByIp { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;

        public enum PaypalStatus
        {
            PENDING,      // Kreirana, čeka unos kartice
            AUTHORIZED,   // Kartica validna, sredstva rezervisana
            CAPTURED,     // Sredstva prebačena merchantu
            FAILED,       // Greška
            EXPIRED,      // Isteklo vreme
            CANCELLED     // Korisnik otkazao
        }
    }
}
