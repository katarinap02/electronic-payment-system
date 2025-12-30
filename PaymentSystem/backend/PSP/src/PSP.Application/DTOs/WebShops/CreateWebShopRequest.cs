namespace PSP.Application.DTOs.WebShops;

public class CreateWebShopRequest
{
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string MerchantId { get; set; }
}
