using PSP.Application.Common;
using PSP.Application.DTOs.PaymentMethods;
using PSP.Application.Interfaces.Repositories;
using PSP.Application.Interfaces.Services;

namespace PSP.Infrastructure.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public PaymentMethodService(IPaymentMethodRepository paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<Result<IEnumerable<PaymentMethodDTO>>> GetAllPaymentMethodsAsync()
    {
        var methods = await _paymentMethodRepository.GetAllAsync();
        var result = methods.Select(m => new PaymentMethodDTO
        {
            Id = m.Id,
            Name = m.Name,
            Code = m.Code,
            Type = m.Type.ToString(),
            Description = m.Description,
            IsEnabled = m.IsActive
        });

        return Result.Success(result.AsEnumerable());
    }

    public async Task<Result<IEnumerable<PaymentMethodDTO>>> GetActivePaymentMethodsAsync()
    {
        var methods = await _paymentMethodRepository.GetActiveAsync();
        var result = methods.Select(m => new PaymentMethodDTO
        {
            Id = m.Id,
            Name = m.Name,
            Code = m.Code,
            Type = m.Type.ToString(),
            Description = m.Description,
            IsEnabled = m.IsActive
        });

        return Result.Success(result.AsEnumerable());
    }
}
