namespace PayPal.API.DTOs
{
    public class CreateOrderResponse
    {
        public string PayPalOrderId { get; set; } = string.Empty;
        public string ApprovalUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "PENDING";
    }
}
