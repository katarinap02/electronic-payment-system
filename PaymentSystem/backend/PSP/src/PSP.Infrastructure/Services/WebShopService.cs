using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<WebShopService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebShopService(
        IWebShopRepository webShopRepository,
        IPaymentMethodRepository paymentMethodRepository,
        ILogger<WebShopService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _webShopRepository = webShopRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<IEnumerable<WebShopResponse>>> GetAllWebShopsAsync()
    {
        var correlationId = GetCorrelationId();

        _logger.LogDebug("[PSP-WEBSHOP] LIST | Desc: Fetching all webshops | CorrId: {CorrId}", correlationId);
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
        var correlationId = GetCorrelationId();
        var ipAddress = GetClientIp();
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("[PSP-WEBSHOP] CREATE_ATTEMPT | Desc: Creating new webshop | Name: {Name} | MerchantId: {MerchantId} | CorrId: {CorrId} | IP: {IP}",
            request.Name, request.MerchantId, correlationId, ipAddress);
        var existingWebShop = await _webShopRepository.GetByMerchantIdAsync(request.MerchantId);
        if (existingWebShop != null)
        {
            _logger.LogWarning("[PSP-WEBSHOP] CREATE_FAILED | Desc: Duplicate MerchantId | MerchantId: {MerchantId} | ExistingWebShopId: {ExistingId} | CorrId: {CorrId} | IP: {IP}",
                request.MerchantId, existingWebShop.Id, correlationId, ipAddress);
            return Result.Failure<WebShopResponse>("WebShop with this MerchantId already exists");
        }

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
        var duration = DateTime.UtcNow - startTime;

        _logger.LogInformation("[PSP-WEBSHOP] CREATED | Desc: Webshop created successfully | WebShopId: {WebShopId} | MerchantId: {MerchantId} | Name: {Name} | DurationMs: {DurationMs} | CorrId: {CorrId} | IP: {IP}",
            createdWebShop.Id, createdWebShop.MerchantId, createdWebShop.Name, duration.TotalMilliseconds, correlationId, ipAddress);

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
            _logger.LogInformation("[PSP-WEBSHOP] PM_AUTO_ADDED | Desc: Default payment method assigned | WebShopId: {WebShopId} | PaymentMethodId: {PmId} | Type: {Type} | CorrId: {CorrId}",
                createdWebShop.Id, creditCardMethod.Id, creditCardMethod.Type, correlationId);
        }
        
        return Result.Success(createdWebShop.ToWebShopResponse());
    }

    public async Task<Result<WebShopResponse>> UpdateWebShopAsync(int id, UpdateWebShopRequest request)
    {
        var correlationId = GetCorrelationId();
        var ipAddress = GetClientIp();

        _logger.LogInformation("[PSP-WEBSHOP] UPDATE_ATTEMPT | Desc: Updating webshop | WebShopId: {WebShopId} | NewStatus: {NewStatus} | CorrId: {CorrId} | IP: {IP}",
            id, request.Status, correlationId, ipAddress);
        var webShop = await _webShopRepository.GetByIdAsync(id);
        if (webShop == null)
        {
            _logger.LogWarning("[PSP-WEBSHOP] UPDATE_FAILED | Desc: Webshop not found | WebShopId: {WebShopId} | CorrId: {CorrId} | IP: {IP}",
                id, correlationId, ipAddress);
            return Result.Failure<WebShopResponse>("WebShop not found");
        }
        webShop.Name = request.Name;
        webShop.Url = request.Url;
        webShop.Status = Enum.Parse<WebShopStatus>(request.Status);

        var updatedWebShop = await _webShopRepository.UpdateAsync(webShop);
        _logger.LogInformation("[PSP-WEBSHOP] UPDATED | Desc: Webshop modified | WebShopId: {WebShopId} | MerchantId: {MerchantId}  | CorrId: {CorrId} | IP: {IP}",
            id,
            updatedWebShop.MerchantId,
            correlationId,
            ipAddress);
        return Result.Success(updatedWebShop.ToWebShopResponse());
    }

    public async Task<Result> DeleteWebShopAsync(int id)
    {
        var correlationId = GetCorrelationId();
        var ipAddress = GetClientIp();

        _logger.LogWarning("[PSP-WEBSHOP] DELETE_ATTEMPT | Desc: Deleting webshop | WebShopId: {WebShopId} | CorrId: {CorrId} | IP: {IP}",
            id, correlationId, ipAddress);
        var deleted = await _webShopRepository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("[PSP-WEBSHOP] DELETE_FAILED | Desc: Webshop not found | WebShopId: {WebShopId} | CorrId: {CorrId} | IP: {IP}",
                id, correlationId, ipAddress);
            return Result.Failure("WebShop not found");
        }
        _logger.LogWarning("[PSP-WEBSHOP] DELETED | Desc: Webshop removed | WebShopId: {WebShopId} | CorrId: {CorrId} | IP: {IP}",
           id, correlationId, ipAddress);

        return Result.Success();
    }

    public async Task<Result> AddPaymentMethodToWebShopAsync(int webShopId, int paymentMethodId)
    {
        var correlationId = GetCorrelationId();
        var ipAddress = GetClientIp();

        _logger.LogInformation("[PSP-WEBSHOP] PM_ADD_ATTEMPT | Desc: Adding payment method | WebShopId: {WebShopId} | PaymentMethodId: {PmId} | CorrId: {CorrId} | IP: {IP}",
            webShopId, paymentMethodId, correlationId, ipAddress);
        var webShop = await _webShopRepository.GetByIdAsync(webShopId);
        if (webShop == null)
        {
            _logger.LogWarning("[PSP-WEBSHOP] PM_ADD_FAILED | Desc: Webshop not found | WebShopId: {WebShopId} | CorrId: {CorrId} | IP: {IP}",
                webShopId, correlationId, ipAddress);
            return Result.Failure("WebShop not found");
        }

        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentMethodId);
        if (paymentMethod == null)
        {
            _logger.LogWarning("[PSP-WEBSHOP] PM_ADD_FAILED | Desc: Payment method not found | PaymentMethodId: {PmId} | CorrId: {CorrId} | IP: {IP}",
                paymentMethodId, correlationId, ipAddress);
            return Result.Failure("Payment method not found");
        }

        var existing = await _paymentMethodRepository.GetWebShopPaymentMethodAsync(webShopId, paymentMethodId);
        if (existing != null)
        {
            _logger.LogWarning("[PSP-WEBSHOP] PM_ADD_FAILED | Desc: Duplicate assignment | WebShopId: {WebShopId} | PaymentMethodId: {PmId} | CorrId: {CorrId} | IP: {IP}",
                webShopId, paymentMethodId, correlationId, ipAddress);
            return Result.Failure("Payment method already assigned to this webshop");
        }

        await _paymentMethodRepository.AddWebShopPaymentMethodAsync(new WebShopPaymentMethod
        {
            WebShopId = webShopId,
            PaymentMethodId = paymentMethodId,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        });
        _logger.LogInformation("[PSP-WEBSHOP] PM_ADDED | Desc: Payment method enabled | WebShopId: {WebShopId} | MerchantId: {MerchantId} | PaymentMethodId: {PmId} | Type: {Type} | CorrId: {CorrId} | IP: {IP}",
            webShopId, webShop.MerchantId, paymentMethodId, paymentMethod.Type, correlationId, ipAddress);

        return Result.Success();
    }

    public async Task<Result> RemovePaymentMethodFromWebShopAsync(int webShopId, int paymentMethodId)
    {
        var correlationId = GetCorrelationId();
        var ipAddress = GetClientIp();

        _logger.LogWarning("[PSP-WEBSHOP] PM_REMOVE_ATTEMPT | Desc: Removing payment method | WebShopId: {WebShopId} | PaymentMethodId: {PmId} | CorrId: {CorrId} | IP: {IP}",
            webShopId, paymentMethodId, correlationId, ipAddress);
        var webShop = await _webShopRepository.GetByIdAsync(webShopId);
        if (webShop == null)
        {
            _logger.LogWarning("[PSP-WEBSHOP] PM_REMOVE_FAILED | Desc: Webshop not found | WebShopId: {WebShopId} | CorrId: {CorrId} | IP: {IP}",
                webShopId, correlationId, ipAddress);
            return Result.Failure("WebShop not found");
        }

        var existing = await _paymentMethodRepository.GetWebShopPaymentMethodAsync(webShopId, paymentMethodId);
        if (existing == null)
        {
            _logger.LogWarning("[PSP-WEBSHOP] PM_REMOVE_FAILED | Desc: Payment method not found | WebShopId: {WebShopId} | PaymentMethodId: {PmId} | CorrId: {CorrId} | IP: {IP}",
                webShopId, paymentMethodId, correlationId, ipAddress);
            return Result.Failure("Payment method not found for this webshop");
        }

        // Check if this is the last payment method
        var enabledPaymentMethodsCount = webShop.WebShopPaymentMethods.Count(wp => wp.IsEnabled);
        if (enabledPaymentMethodsCount <= 1)
        {
            _logger.LogWarning("[PSP-WEBSHOP] PM_REMOVE_BLOCKED | Desc: Cannot remove last payment method | WebShopId: {WebShopId} | PaymentMethodId: {PmId} | EnabledCount: {Count} | CorrId: {CorrId} | IP: {IP}",
                webShopId, paymentMethodId, enabledPaymentMethodsCount, correlationId, ipAddress);
            return Result.Failure("Cannot remove the last payment method. WebShop must have at least one payment method.");
        }

        await _paymentMethodRepository.RemoveWebShopPaymentMethodAsync(existing);
        _logger.LogWarning("[PSP-WEBSHOP] PM_REMOVED | Desc: Payment method disabled | WebShopId: {WebShopId} | MerchantId: {MerchantId} | PaymentMethodId: {PmId} | RemainingMethods: {Remaining} | CorrId: {CorrId} | IP: {IP}",
            webShopId, webShop.MerchantId, paymentMethodId, enabledPaymentMethodsCount - 1, correlationId, ipAddress);
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

    private string GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N")[..12];
    }

    private string GetClientIp()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return "internal";

        var forwarded = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded)) return forwarded.Split(',')[0].Trim();

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
