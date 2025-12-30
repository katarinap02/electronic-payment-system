namespace PSP.Domain.Entities;

public class WebShopAdmin
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int WebShopId { get; set; }
    public WebShop WebShop { get; set; } = null!;
    
    public DateTime AssignedAt { get; set; }
}
