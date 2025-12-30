namespace PSP.API.Models
{
    public enum PaymentMethodType
    {
        CreditCard = 1,
        IPSScan = 2
    }

    public class PaymentMethod
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public PaymentMethodType Type { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<WebShopPaymentMethod> WebShopPaymentMethods { get; set; } = new List<WebShopPaymentMethod>();
    }
}
