namespace PSP.Domain.Entities;

public class WebShopPaymentMethod
{
    public int Id { get; set; }
    public int WebShopId { get; set; }
    public int PaymentMethodId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public WebShop WebShop { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; } = null!;
}
