using PSP.API.Common;
using PSP.API.DTOs;

namespace PSP.API.Services.Interfaces;

public interface IPaymentMethodService
{
    Task<Result<IEnumerable<PaymentMethodDTO>>> GetAllPaymentMethodsAsync();
    Task<Result<IEnumerable<PaymentMethodDTO>>> GetActivePaymentMethodsAsync();
}
