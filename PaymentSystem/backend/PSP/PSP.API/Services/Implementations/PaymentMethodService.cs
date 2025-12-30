using PSP.API.Common;
using PSP.API.DTOs;
using PSP.API.Repositories.Interfaces;
using PSP.API.Services.Interfaces;

namespace PSP.API.Services.Implementations;

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
