namespace Bank.API.DTOs
{
    public class PaymentResponse
    {
        public string PaymentUrl { get; set; }
        public string PaymentId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Message { get; set; }
    }
}
