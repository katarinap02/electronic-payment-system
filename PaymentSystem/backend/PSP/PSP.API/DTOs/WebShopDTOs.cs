namespace PSP.API.DTOs
{
    public class WebShopResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Url { get; set; }
        public required string MerchantId { get; set; }
        public required string ApiKey { get; set; }
        public required string Status { get; set; }
        public List<PaymentMethodDTO> PaymentMethods { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class PaymentMethodDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public required string Type { get; set; }
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class UpdateWebShopPaymentMethodsRequest
    {
        public required List<int> PaymentMethodIds { get; set; }
    }

    public class CreateWebShopRequest
    {
        public required string Name { get; set; }
        public required string Url { get; set; }
        public required string MerchantId { get; set; }
    }

    public class UpdateWebShopRequest
    {
        public required string Name { get; set; }
        public required string Url { get; set; }
        public required string Status { get; set; }
    }
}
