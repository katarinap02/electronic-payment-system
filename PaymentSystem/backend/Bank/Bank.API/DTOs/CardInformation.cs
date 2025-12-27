namespace Bank.API.DTOs
{
    public class CardInformation
    {
        public string PaymentId { get; set; }
        public string CardNumber { get; set; } // PAN
        public string CardholderName { get; set; }
        public string ExpiryDate { get; set; } // "12/25"
        public string Cvv { get; set; } //security code
        public bool IsQrPayment { get; set; }

    }
}
