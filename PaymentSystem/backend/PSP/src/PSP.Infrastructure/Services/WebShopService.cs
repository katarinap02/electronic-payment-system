using PSP.Application.Common;
using PSP.Application.DTOs.PaymentMethods;
using PSP.Application.DTOs.WebShops;
using PSP.Application.Extensions;
using PSP.Application.Interfaces.Repositories;
using PSP.Application.Interfaces.Services;
using PSP.Domain.Entities;
using PSP.Domain.Enums;

namespace PSP.Infrastructure.Services;

public class WebShopService : IWebShopService
{
    private readonly IWebShopRepository _webShopRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public WebShopService(
        IWebShopRepository webShopRepository,
        IPaymentMethodRepository paymentMethodRepository)
    {
        _webShopRepository = webShopRepository;
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<Result<IEnumerable<WebShopResponse>>> GetAllWebShopsAsync()
    {
        var webShops = await _webShopRepository.GetAllAsync();
        var result = webShops.Select(ws => ws.ToWebShopResponse());
        return Result.Success(result.AsEnumerable());
    }

    public async Task<Result<WebShopResponse>> GetWebShopByIdAsync(int id)
    {
        var webShop = await _webShopRepository.GetByIdAsync(id);
        
        if (webShop == null)
            return Result.Failure<WebShopResponse>("WebShop not found");

        return Result.Success(webShop.ToWebShopResponse());
    }

    public async Task<Result<WebShopResponse>> CreateWebShopAsync(CreateWebShopRequest request)
    {
        var existingWebShop = await _webShopRepository.GetByMerchantIdAsync(request.MerchantId);
        if (existingWebShop != null)
            return Result.Failure<WebShopResponse>("WebShop with this MerchantId already exists");

        var webShop = new WebShop
        {
            Name = request.Name,
            Url = request.Url,
            MerchantId = request.MerchantId,
            ApiKey = Guid.NewGuid().ToString("N"),
            Status = WebShopStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var createdWebShop = await _webShopRepository.CreateAsync(webShop);
        
        // Automatically add Credit Card payment method by default
        var creditCardMethod = (await _paymentMethodRepository.GetAllAsync())
            .FirstOrDefault(pm => pm.Type == PaymentMethodType.CreditCard);
        
        if (creditCardMethod != null)
        {
            await _paymentMethodRepository.AddWebShopPaymentMethodAsync(new WebShopPaymentMethod
            {
                WebShopId = createdWebShop.Id,
                PaymentMethodId = creditCardMethod.Id,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        return Result.Success(createdWebShop.ToWebShopResponse());
    }

    public async Task<Result<WebShopResponse>> UpdateWebShopAsync(int id, UpdateWebShopRequest request)
    {
        var webShop = await _webShopRepository.GetByIdAsync(id);
        if (webShop == null)
            return Result.Failure<WebShopResponse>("WebShop not found");

        webShop.Name = request.Name;
        webShop.Url = request.Url;
        webShop.Status = Enum.Parse<WebShopStatus>(request.Status);

        var updatedWebShop = await _webShopRepository.UpdateAsync(webShop);
        return Result.Success(updatedWebShop.ToWebShopResponse());
    }

    public async Task<Result> DeleteWebShopAsync(int id)
    {
        var deleted = await _webShopRepository.DeleteAsync(id);
        
        if (!deleted)
            return Result.Failure("WebShop not found");

        return Result.Success();
    }

    public async Task<Result> AddPaymentMethodToWebShopAsync(int webShopId, int paymentMethodId)
    {
        var webShop = await _webShopRepository.GetByIdAsync(webShopId);
        if (webShop == null)
            return Result.Failure("WebShop not found");

        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentMethodId);
        if (paymentMethod == null)
            return Result.Failure("Payment method not found");

        var existing = await _paymentMethodRepository.GetWebShopPaymentMethodAsync(webShopId, paymentMethodId);
        if (existing != null)
            return Result.Failure("Payment method already assigned to this webshop");

        await _paymentMethodRepository.AddWebShopPaymentMethodAsync(new WebShopPaymentMethod
        {
            WebShopId = webShopId,
            PaymentMethodId = paymentMethodId,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success();
    }

    public async Task<Result> RemovePaymentMethodFromWebShopAsync(int webShopId, int paymentMethodId)
    {
        var webShop = await _webShopRepository.GetByIdAsync(webShopId);
        if (webShop == null)
            return Result.Failure("WebShop not found");

        var existing = await _paymentMethodRepository.GetWebShopPaymentMethodAsync(webShopId, paymentMethodId);
        if (existing == null)
            return Result.Failure("Payment method not found for this webshop");

        // Check if this is the last payment method
        var enabledPaymentMethodsCount = webShop.WebShopPaymentMethods.Count(wp => wp.IsEnabled);
        if (enabledPaymentMethodsCount <= 1)
            return Result.Failure("Cannot remove the last payment method. WebShop must have at least one payment method.");

        await _paymentMethodRepository.RemoveWebShopPaymentMethodAsync(existing);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<PaymentMethodDTO>>> GetWebShopPaymentMethodsAsync(int webShopId)
    {
        var webShop = await _webShopRepository.GetByIdAsync(webShopId);
        if (webShop == null)
            return Result.Failure<IEnumerable<PaymentMethodDTO>>("WebShop not found");

        var methods = webShop.WebShopPaymentMethods
            .Where(wp => wp.IsEnabled)
            .Select(wp => wp.PaymentMethod.ToPaymentMethodDTO(wp.IsEnabled));

        return Result.Success(methods.AsEnumerable());
    }
}
