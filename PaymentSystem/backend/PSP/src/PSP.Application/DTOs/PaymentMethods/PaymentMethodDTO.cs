namespace PSP.Application.DTOs.PaymentMethods;

public class PaymentMethodDTO
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string Type { get; set; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
}
