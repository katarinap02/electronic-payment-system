namespace Bank.API.Models
{
    public class BankAccount
    {
        public long Id { get; set; }
        public string AccountNumber { get; set; } //svi racuni u istoj banci, IBAN javan podatak
        public string SwiftCode { get; set; } = "BACXRSBG"; //11 karaktera, javan
        public decimal Balance { get; set; } //stanje na racunu
        public decimal AvailableBalance { get; set; }
        public decimal ReservedBalance { get; set; }
        public decimal PendingCaptureBalance { get; set; }
        public string Currency { get; set; } = "EUR";
        public bool IsMerchantAccount { get; set; } // da li je racun prodavca ili kupca
        public string? MerchantId { get; set; } // Ovo je BANKIN merchant_id koji banka daje PSP-u
        public string CustomerId { get; set; }
        public Customer Customer {get; set;}
        public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    }
}
