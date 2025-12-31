using Microsoft.Extensions.Configuration;
using PSP.Application.DTOs.Payments;
using PSP.Application.Interfaces.Repositories;
using PSP.Domain.Entities;
using PSP.Domain.Enums;

namespace PSP.Infrastructure.Services;

public class PaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWebShopRepository _webShopRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IWebShopRepository webShopRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _paymentRepository = paymentRepository;
        _webShopRepository = webShopRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<PaymentInitializationResponse> InitializePaymentAsync(PaymentInitializationRequest request)
    {
        // Validate merchant credentials
        var webShop = await _webShopRepository.GetByMerchantIdAsync(request.MerchantId);
        if (webShop == null)
        {
            throw new UnauthorizedAccessException("Invalid merchant ID");
        }

        if (webShop.ApiKey != request.MerchantPassword)
        {
            throw new UnauthorizedAccessException("Invalid merchant password");
        }

        if (webShop.Status != WebShopStatus.Active)
        {
            throw new InvalidOperationException("WebShop is not active");
        }

        // Check if merchant order ID already exists
        var existingPayment = await _paymentRepository.GetByMerchantOrderIdAsync(webShop.Id, request.MerchantOrderId);
        if (existingPayment != null)
        {
            throw new InvalidOperationException("Payment with this merchant order ID already exists");
        }

        // Create payment (PaymentMethodId will be set later when user selects on PSP page)
        var accessToken = Guid.NewGuid().ToString("N");
        var payment = new Payment
        {
            WebShopId = webShop.Id,
            PaymentMethodId = null,
            MerchantOrderId = request.MerchantOrderId,
            Amount = request.Amount,
            Currency = request.Currency,
            MerchantTimestamp = request.MerchantTimestamp,
            Status = PaymentStatus.Pending,
            AccessToken = accessToken
        };

        var createdPayment = await _paymentRepository.CreateAsync(payment);

        // Generate payment URL with access token
        var pspFrontendUrl = _configuration["PSPFrontendUrl"] ?? "http://localhost:5174";
        var paymentUrl = $"{pspFrontendUrl}/payment/{createdPayment.Id}?token={accessToken}";

        // Update payment with URL
        createdPayment.PaymentUrl = paymentUrl;
        await _paymentRepository.UpdateAsync(createdPayment);

        return new PaymentInitializationResponse
        {
            PaymentId = createdPayment.Id,
            PaymentUrl = paymentUrl,
            Status = createdPayment.Status
        };
    }

    public async Task<Payment?> GetPaymentByIdAsync(int id)
    {
        return await _paymentRepository.GetByIdAsync(id);
    }

    public async Task<PaymentMethodSelectionResponse> SelectPaymentMethodAsync(int paymentId, int paymentMethodId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
        {
            throw new InvalidOperationException("Payment not found");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("Payment method can only be selected for pending payments");
        }

        // Verify that the payment method is available for this webshop
        var webShop = await _webShopRepository.GetByIdWithPaymentMethodsAsync(payment.WebShopId);
        if (webShop == null)
        {
            throw new InvalidOperationException("WebShop not found");
        }

        var hasPaymentMethod = webShop.WebShopPaymentMethods.Any(wpm => wpm.PaymentMethodId == paymentMethodId);
        if (!hasPaymentMethod)
        {
            throw new InvalidOperationException("This payment method is not available for this webshop");
        }

        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentMethodId);
        if (paymentMethod == null)
        {
            throw new InvalidOperationException("Payment method not found");
        }

        payment.PaymentMethodId = paymentMethodId;
        payment.Status = PaymentStatus.Processing;
        await _paymentRepository.UpdateAsync(payment);

        // Prepare data for frontend to call Bank API
        var stan = $"PSP-{payment.Id}-{DateTime.UtcNow.Ticks}";
        var pspBackendUrl = _configuration["PSPBackendUrl"] ?? "http://localhost:5002";
        
        return new PaymentMethodSelectionResponse
        {
            MerchantId = webShop.MerchantId,
            Amount = payment.Amount,
            Currency = payment.Currency.ToString(),
            Stan = stan,
            PspTimestamp = DateTime.UtcNow,
            SuccessUrl = $"{pspBackendUrl}/api/payments/{payment.Id}/bank-callback?status=success",
            FailedUrl = $"{pspBackendUrl}/api/payments/{payment.Id}/bank-callback?status=failed",
            ErrorUrl = $"{pspBackendUrl}/api/payments/{payment.Id}/bank-callback?status=error"
        };
    }

    public class PaymentMethodSelectionResponse
    {
        public string MerchantId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Stan { get; set; } = string.Empty;
        public DateTime PspTimestamp { get; set; }
        public string SuccessUrl { get; set; } = string.Empty;
        public string FailedUrl { get; set; } = string.Empty;
        public string ErrorUrl { get; set; } = string.Empty;
    }

    public async Task CancelPaymentAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
        {
            throw new InvalidOperationException("Payment not found");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return; // Only cancel pending payments
        }

        payment.Status = PaymentStatus.Failed;
        await _paymentRepository.UpdateAsync(payment);
    }

    public async Task<string> HandleBankCallbackAsync(int paymentId, string status, string? bankPaymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
        {
            throw new InvalidOperationException("Payment not found");
        }

        var webShop = await _webShopRepository.GetByIdAsync(payment.WebShopId);
        if (webShop == null)
        {
            throw new InvalidOperationException("WebShop not found");
        }

        // Update payment status based on bank callback
        switch (status.ToLower())
        {
            case "success":
                payment.Status = PaymentStatus.Success;
                break;
            case "failed":
                payment.Status = PaymentStatus.Failed;
                break;
            case "error":
                payment.Status = PaymentStatus.Error;
                break;
            default:
                throw new InvalidOperationException($"Invalid status: {status}");
        }

        await _paymentRepository.UpdateAsync(payment);

        // Redirect to WebShop based on status
        string redirectUrl;
        switch (status.ToLower())
        {
            case "success":
                redirectUrl = $"{webShop.SuccessUrl}?paymentId={paymentId}&orderId={payment.MerchantOrderId}&amount={payment.Amount}&currency={payment.Currency}";
                break;
            case "failed":
                redirectUrl = $"{webShop.FailedUrl}?paymentId={paymentId}&orderId={payment.MerchantOrderId}";
                break;
            case "error":
                redirectUrl = $"{webShop.ErrorUrl}?paymentId={paymentId}&orderId={payment.MerchantOrderId}";
                break;
            default:
                throw new InvalidOperationException($"Invalid status: {status}");
        }

        return redirectUrl;
    }
}
