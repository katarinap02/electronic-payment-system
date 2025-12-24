namespace Bank.API.Models
{
    public class BankAccount
    {
        public long Id { get; set; }
        public string AccountNumber { get; set; } //svi racuni u istoj banci
        public string SwiftCode { get; set; } = "BACXRSBG"; //11 karaktera
        public decimal Balance { get; set; } //stanje na racunu
        public string Currency { get; set; } = "EUR";
        public bool IsMerchantAccount { get; set; } // da li je racun prodavca ili kupca
        public string? MerchantId { get; set; } // Ovo je BANKIN merchant_id koji banka daje PSP-u
        public string? ExternalUserId { get; set; } //userId iz webShopa, POKRIVA: Link ka WebShop korisniku (bez direktne veze sa bazom)
        public ICollection<PaymentTransaction> MerchantTransactions { get; set; }
        public ICollection<PaymentTransaction> CustomerTransactions { get; set; }
    }
}
