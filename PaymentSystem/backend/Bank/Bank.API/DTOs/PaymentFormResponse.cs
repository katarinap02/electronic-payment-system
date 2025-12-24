namespace Bank.API.DTOs
{
    public class PaymentFormResponse
    {
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string MerchantName { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsQrPayment { get; set; }
    }
}
