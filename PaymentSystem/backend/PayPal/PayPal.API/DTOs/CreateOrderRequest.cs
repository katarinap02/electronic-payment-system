namespace PayPal.API.DTOs
{
    public class CreateOrderRequest
    {
        // Veza ka PSP sistemu ( MERCHANT_ORDER_ID)
        public string PspTransactionId { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty ;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }
}
