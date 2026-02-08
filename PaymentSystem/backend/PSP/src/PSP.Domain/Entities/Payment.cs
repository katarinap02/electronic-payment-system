using PSP.Domain.Enums;

namespace PSP.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int WebShopId { get; set; }
    public int? PaymentMethodId { get; set; }
    public required string MerchantOrderId { get; set; }
    public string? CustomerId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime MerchantTimestamp { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? PaymentUrl { get; set; }
    public required string AccessToken { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public WebShop WebShop { get; set; } = null!;
    public PaymentMethod? PaymentMethod { get; set; }
}
