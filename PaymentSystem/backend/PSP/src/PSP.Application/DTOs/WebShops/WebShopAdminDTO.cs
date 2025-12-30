namespace PSP.Application.DTOs.WebShops;

public class AssignWebShopAdminRequest
{
    public required int UserId { get; set; }
    public required int WebShopId { get; set; }
}

public class WebShopAdminResponse
{
    public int UserId { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public DateTime AssignedAt { get; set; }
}
