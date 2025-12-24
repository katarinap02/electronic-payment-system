namespace Bank.API.Models
{
    public class CardToken
    {
        public long Id { get; set; }
        public string Token { get; set; } //token umesto PAN-a
        public string CardHash { get; set; }  // Hash(PAN + salt)
        public string MaskedPan { get; set; }
        public string CardholderName { get; set; } //stavka 4
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string? CvvHash { get; set; } // Security Code hashovan za proveru pa se brise
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }  // Soft delete za PCI
        public long? TransactionId { get; set; } // Jedna kartica = jedna transakcija (stavka 4a: "ograničiti je na samo jedan pokušaj plaćanja")
        public PaymentTransaction Transaction { get; set; }
    }
}
