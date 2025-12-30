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

    public PaymentService(
        IPaymentRepository paymentRepository,
        IWebShopRepository webShopRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IConfiguration configuration)
    {
        _paymentRepository = paymentRepository;
        _webShopRepository = webShopRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _configuration = configuration;
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

    public async Task SelectPaymentMethodAsync(int paymentId, int paymentMethodId)
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

        payment.PaymentMethodId = paymentMethodId;
        payment.Status = PaymentStatus.Processing;
        await _paymentRepository.UpdateAsync(payment);
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
}
