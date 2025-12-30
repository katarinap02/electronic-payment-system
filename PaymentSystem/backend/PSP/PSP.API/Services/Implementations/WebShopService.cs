using PSP.API.Common;
using PSP.API.DTOs;
using PSP.API.Models;
using PSP.API.Repositories.Interfaces;
using PSP.API.Services.Interfaces;

namespace PSP.API.Services.Implementations;

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
        var result = webShops.Select(MapToResponse);
        return Result.Success(result.AsEnumerable());
    }

    public async Task<Result<WebShopResponse>> GetWebShopByIdAsync(int id)
    {
        var webShop = await _webShopRepository.GetByIdAsync(id);
        
        if (webShop == null)
            return Result.Failure<WebShopResponse>("WebShop not found");

        return Result.Success(MapToResponse(webShop));
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
        return Result.Success(MapToResponse(createdWebShop));
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
        return Result.Success(MapToResponse(updatedWebShop));
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
        var existing = await _paymentMethodRepository.GetWebShopPaymentMethodAsync(webShopId, paymentMethodId);
        if (existing == null)
            return Result.Failure("Payment method not found for this webshop");

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
            .Select(wp => new PaymentMethodDTO
            {
                Id = wp.PaymentMethod.Id,
                Name = wp.PaymentMethod.Name,
                Code = wp.PaymentMethod.Code,
                Type = wp.PaymentMethod.Type.ToString(),
                Description = wp.PaymentMethod.Description,
                IsEnabled = wp.IsEnabled
            });

        return Result.Success(methods.AsEnumerable());
    }

    public async Task<bool> UpdateWebShopPaymentMethodsAsync(int webShopId, UpdateWebShopPaymentMethodsRequest request)
    {
        var webShop = await _webShopRepository.GetByIdAsync(webShopId);
        if (webShop == null) return false;

        // Get all payment methods for this webshop
        var existingMethods = webShop.WebShopPaymentMethods.ToList();

        // Remove unchecked payment methods
        foreach (var existing in existingMethods)
        {
            if (!request.PaymentMethodIds.Contains(existing.PaymentMethodId))
            {
                await _paymentMethodRepository.RemoveWebShopPaymentMethodAsync(existing);
            }
        }

        // Add new payment methods
        foreach (var paymentMethodId in request.PaymentMethodIds)
        {
            var existing = await _paymentMethodRepository.GetWebShopPaymentMethodAsync(webShopId, paymentMethodId);
            if (existing == null)
            {
                await _paymentMethodRepository.AddWebShopPaymentMethodAsync(new WebShopPaymentMethod
                {
                    WebShopId = webShopId,
                    PaymentMethodId = paymentMethodId,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return true;
    }

    private WebShopResponse MapToResponse(WebShop webShop)
    {
        return new WebShopResponse
        {
            Id = webShop.Id,
            Name = webShop.Name,
            Url = webShop.Url,
            MerchantId = webShop.MerchantId,
            ApiKey = webShop.ApiKey,
            Status = webShop.Status.ToString(),
            PaymentMethods = webShop.WebShopPaymentMethods
                .Where(wp => wp.IsEnabled)
                .Select(wp => new PaymentMethodDTO
                {
                    Id = wp.PaymentMethod.Id,
                    Name = wp.PaymentMethod.Name,
                    Code = wp.PaymentMethod.Code,
                    Type = wp.PaymentMethod.Type.ToString(),
                    Description = wp.PaymentMethod.Description,
                    IsEnabled = wp.IsEnabled
                }).ToList(),
            CreatedAt = webShop.CreatedAt
        };
    }
}
