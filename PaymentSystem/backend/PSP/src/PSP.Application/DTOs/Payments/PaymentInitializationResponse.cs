using PSP.Domain.Enums;

namespace PSP.Application.DTOs.Payments;

public class PaymentInitializationResponse
{
    public int PaymentId { get; set; }
    public required string PaymentUrl { get; set; }
    public PaymentStatus Status { get; set; }
}
