namespace Bank.API.DTOs
{
    public class PaymentStatusResponse
    {
        public string PaymentId { get; set; }
        public string Status { get; set; }  // "PENDING", "AUTHORIZED", "FAILED"
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string GlobalTransactionId { get; set; }
        public DateTime AcquirerTimestamp { get; set; }
        public DateTime? AuthorizedAt { get; set; }
        public string Message { get; set; }
    }
}
