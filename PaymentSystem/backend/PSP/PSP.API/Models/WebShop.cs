namespace PSP.API.Models
{
    public enum WebShopStatus
    {
        Active,
        Inactive,
        Suspended
    }

    public class WebShop
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Url { get; set; }
        public required string ApiKey { get; set; }
        public required string MerchantId { get; set; }
        public WebShopStatus Status { get; set; } = WebShopStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<WebShopPaymentMethod> WebShopPaymentMethods { get; set; } = new List<WebShopPaymentMethod>();
    }
}
