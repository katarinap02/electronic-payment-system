using PSP.Application.Common;
using PSP.Application.DTOs.PaymentMethods;

namespace PSP.Application.Interfaces.Services;

public interface IPaymentMethodService
{
    Task<Result<IEnumerable<PaymentMethodDTO>>> GetAllPaymentMethodsAsync();
    Task<Result<IEnumerable<PaymentMethodDTO>>> GetActivePaymentMethodsAsync();
}
