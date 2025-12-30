namespace PSP.Application.DTOs.PaymentMethods;

public class EnablePaymentMethodRequest
{
    public int WebShopId { get; set; }
    public int PaymentMethodId { get; set; }
}
