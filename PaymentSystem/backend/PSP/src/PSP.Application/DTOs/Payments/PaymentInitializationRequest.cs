using PSP.Domain.Enums;

namespace PSP.Application.DTOs.Payments;

public class PaymentInitializationRequest
{
    public required string MerchantId { get; set; }
    public required string MerchantPassword { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public required string MerchantOrderId { get; set; }
    public DateTime MerchantTimestamp { get; set; }
}
