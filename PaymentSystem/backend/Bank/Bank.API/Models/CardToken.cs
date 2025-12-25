namespace Bank.API.Models
{
    public class CardToken
    {
        public long Id { get; set; }
        public string Token { get; set; } = "tok_" + Guid.NewGuid().ToString("N").Substring(0, 16);
        public long CardId { get; set; }
        public Card Card { get; set; }
        public string? CvvHash { get; set; }
        public DateTime? CvvValidatedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }  // Soft delete za PCI
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(15);
        public bool IsUsed { get; set; } = false;
        public long? TransactionId { get; set; } // Jedna kartica = jedna transakcija (stavka 4a: "ograničiti je na samo jedan pokušaj plaćanja")
        public PaymentTransaction? Transaction { get; set; }

        public void ValidateAndDeleteCvv()
        {
            CvvHash = null;
            CvvValidatedAt = DateTime.UtcNow;
        }
    }
}
