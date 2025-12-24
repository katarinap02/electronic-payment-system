namespace Bank.API.DTOs
{
    public class TransactionStatus
    {
        public string Stan { get; set; }
        public string Status { get; set; }  // "AUTHORIZED", "CAPTURED", "FAILED"
        public string GlobalTransactionId { get; set; }
        public DateTime AcquirerTimestamp { get; set; }
        public string ErrorMessage { get; set; }
    }
}
