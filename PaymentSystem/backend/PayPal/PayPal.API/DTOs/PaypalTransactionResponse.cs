namespace PayPal.API.DTOs
{
    public class PaypalTransactionResponse
    {
        public string PspTransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;  
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
    }
}
