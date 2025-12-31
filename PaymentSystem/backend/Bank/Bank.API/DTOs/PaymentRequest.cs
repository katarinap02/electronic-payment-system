namespace Bank.API.DTOs
{
    public class PaymentRequest
    {
        public string MerchantId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Stan { get; set; }
        public DateTime PspTimestamp { get; set; }
        public string SuccessUrl { get; set; }
        public string FailedUrl { get; set; }
        public string ErrorUrl { get; set; }
    }
}
