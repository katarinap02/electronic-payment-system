namespace PSP.Application.DTOs.WebShops;

public class UpdateWebShopRequest
{
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string Status { get; set; }
}
