using PSP.Application.DTOs.PaymentMethods;

namespace PSP.Application.DTOs.WebShops;

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
