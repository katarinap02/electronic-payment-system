namespace Bank.API.DTOs
{
    public class CardInformation
    {
        public string PaymentId { get; set; }
        public string CardNumber { get; set; } //tokenizuje se PAN
        public string CardholderName { get; set; }
        public string ExpiryDate { get; set; } // "12/25"
        public string Cvv { get; set; }
        public bool IsQrPayment { get; set; }

    }
}
