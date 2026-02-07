using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PSP.Application.DTOs.Payments;
using PSP.Application.Interfaces.Repositories;
using PSP.Domain.Entities;
using PSP.Domain.Enums;
using System.Net.Http.Json;

namespace PSP.Infrastructure.Services;

public class PaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWebShopRepository _webShopRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IWebShopRepository webShopRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<PaymentService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _paymentRepository = paymentRepository;
        _webShopRepository = webShopRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PaymentInitializationResponse> InitializePaymentAsync(PaymentInitializationRequest request)
    {
        var correlationId = GetCorrelationId();
        var ipAddress = GetClientIp();
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("[PSP-PAYMENT] INIT_ATTEMPT | Desc: Payment initialization started | MerchantId: {MerchantId} | MerchantOrderId: {MerchantOrderId} | Amount: {Amount} {Currency} | CorrId: {CorrId} | IP: {IP}",
        request.MerchantId, request.MerchantOrderId, request.Amount, request.Currency, correlationId, ipAddress);
        // Validate merchant credentials
        var webShop = await _webShopRepository.GetByMerchantIdAsync(request.MerchantId);
        if (webShop == null)
        {
            _logger.LogWarning("[PSP-PAYMENT] INIT_FAILED | Desc: Merchant not found | MerchantId: {MerchantId} | FailReason: MERCHANT_NOT_FOUND | CorrId: {CorrId} | IP: {IP}",
                request.MerchantId, correlationId, ipAddress);
            throw new UnauthorizedAccessException("Invalid merchant ID");
        }

        if (webShop.ApiKey != request.MerchantPassword)
        {
            _logger.LogWarning("[PSP-PAYMENT] INIT_FAILED | Desc: Invalid merchant credentials | WebShopId: {WebShopId} | MerchantId: {MerchantId} | FailReason: INVALID_API_KEY | CorrId: {CorrId} | IP: {IP}",
                webShop.Id, request.MerchantId, correlationId, ipAddress);
            throw new UnauthorizedAccessException("Invalid merchant password");
        }

        if (webShop.Status != WebShopStatus.Active)
        {
            _logger.LogWarning("[PSP-PAYMENT] INIT_FAILED | Desc: Merchant not active | WebShopId: {WebShopId} | MerchantId: {MerchantId} | Status: {Status} | FailReason: MERCHANT_INACTIVE | CorrId: {CorrId} | IP: {IP}",
                webShop.Id, request.MerchantId, webShop.Status, correlationId, ipAddress);
            throw new InvalidOperationException("WebShop is not active");
        }

        // Check if merchant order ID already exists
        var existingPayment = await _paymentRepository.GetByMerchantOrderIdAsync(webShop.Id, request.MerchantOrderId);
        if (existingPayment != null)
        {
            _logger.LogWarning("[PSP-PAYMENT] INIT_FAILED | Desc: Duplicate merchant order ID | WebShopId: {WebShopId} | MerchantId: {MerchantId} | MerchantOrderId: {MerchantOrderId} | ExistingPaymentId: {ExistingPaymentId} | FailReason: DUPLICATE_ORDER_ID | CorrId: {CorrId} | IP: {IP}",
                webShop.Id, request.MerchantId, request.MerchantOrderId, existingPayment.Id, correlationId, ipAddress);
            throw new InvalidOperationException("Payment with this merchant order ID already exists");
        }
        _logger.LogInformation("[PSP-PAYMENT] MERCHANT_VALIDATED | Desc: Merchant authentication successful | WebShopId: {WebShopId} | MerchantId: {MerchantId} | ValidationDurationMs: {ValidationMs} | CorrId: {CorrId}",
        webShop.Id, request.MerchantId, (DateTime.UtcNow - startTime).TotalMilliseconds, correlationId);

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
        var pspFrontendUrl = _configuration["PSPFrontendUrl"] ?? "https://localhost:5174";
        var paymentUrl = $"{pspFrontendUrl}/payment/{createdPayment.Id}?token={accessToken}";

        // Update payment with URL
        createdPayment.PaymentUrl = paymentUrl;
        await _paymentRepository.UpdateAsync(createdPayment);
        var duration = DateTime.UtcNow - startTime;

        _logger.LogInformation("[PSP-PAYMENT] INIT_SUCCESS | Desc: Payment initialized, URL generated | PaymentId: {PaymentId} | WebShopId: {WebShopId} | MerchantId: {MerchantId} | MerchantOrderId: {MerchantOrderId} | Amount: {Amount} {Currency} | Status: {Status} | TotalDurationMs: {DurationMs} | CorrId: {CorrId} | IP: {IP}",
            createdPayment.Id, webShop.Id, request.MerchantId, request.MerchantOrderId, request.Amount, request.Currency, createdPayment.Status, duration.TotalMilliseconds, correlationId, ipAddress);

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
        var correlationId = GetCorrelationId();
        var ipAddress = GetClientIp();
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("[PSP-PAYMENT] METHOD_SELECT_ATTEMPT | Desc: Payment method selection started | PaymentId: {PaymentId} | SelectedMethodId: {MethodId} | CorrId: {CorrId} | IP: {IP}",
        paymentId, paymentMethodId, correlationId, ipAddress);
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
        {
            _logger.LogWarning("[PSP-PAYMENT] METHOD_SELECT_FAILED | Desc: Payment not found | PaymentId: {PaymentId} | FailReason: PAYMENT_NOT_FOUND | CorrId: {CorrId} | IP: {IP}",
                paymentId, correlationId, ipAddress);
            throw new InvalidOperationException("Payment not found");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            _logger.LogWarning("[PSP-PAYMENT] METHOD_SELECT_FAILED | Desc: Invalid payment status | PaymentId: {PaymentId} | CurrentStatus: {Status} | FailReason: INVALID_STATUS | CorrId: {CorrId} | IP: {IP}",
                paymentId, payment.Status, correlationId, ipAddress);
            throw new InvalidOperationException("Payment method can only be selected for pending payments");
        }

        // Verify that the payment method is available for this webshop
        var webShop = await _webShopRepository.GetByIdWithPaymentMethodsAsync(payment.WebShopId);
        if (webShop == null)
        {
            _logger.LogWarning("[PSP-PAYMENT] METHOD_SELECT_FAILED | Desc: Webshop not found | PaymentId: {PaymentId} | WebShopId: {WebShopId} | FailReason: WEBSHOP_NOT_FOUND | CorrId: {CorrId} | IP: {IP}",
                paymentId, payment.WebShopId, correlationId, ipAddress);
            throw new InvalidOperationException("WebShop not found");
        }

        var hasPaymentMethod = webShop.WebShopPaymentMethods.Any(wpm => wpm.PaymentMethodId == paymentMethodId);
        if (!hasPaymentMethod)
        {
            _logger.LogWarning("[PSP-PAYMENT] METHOD_SELECT_FAILED | Desc: Payment method not available for merchant | PaymentId: {PaymentId} | WebShopId: {WebShopId} | MerchantId: {MerchantId} | RequestedMethodId: {MethodId} | AvailableMethods: {AvailableMethods} | FailReason: METHOD_NOT_AVAILABLE | CorrId: {CorrId} | IP: {IP}",
                paymentId, webShop.Id, webShop.MerchantId, paymentMethodId, string.Join(",", webShop.WebShopPaymentMethods.Select(wpm => wpm.PaymentMethodId)), correlationId, ipAddress);
            throw new InvalidOperationException("This payment method is not available for this webshop");
        }

        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentMethodId);
        if (paymentMethod == null)
        {
            _logger.LogWarning("[PSP-PAYMENT] METHOD_SELECT_FAILED | Desc: Payment method not found in system | PaymentId: {PaymentId} | MethodId: {MethodId} | FailReason: METHOD_NOT_FOUND | CorrId: {CorrId} | IP: {IP}",
                paymentId, paymentMethodId, correlationId, ipAddress);
            throw new InvalidOperationException("Payment method not found");
        }
        _logger.LogInformation("[PSP-PAYMENT] METHOD_VALIDATED | Desc: Payment method selected and validated | PaymentId: {PaymentId} | WebShopId: {WebShopId} | MerchantId: {MerchantId} | MethodId: {MethodId} | MethodCode: {MethodCode} | MethodType: {MethodType} | CorrId: {CorrId}",
        paymentId, webShop.Id, webShop.MerchantId, paymentMethodId, paymentMethod.Code, paymentMethod.Type, correlationId);

        payment.PaymentMethodId = paymentMethodId;
        payment.Status = PaymentStatus.Processing;
        await _paymentRepository.UpdateAsync(payment);

        _logger.LogInformation("[PSP-PAYMENT] STATUS_UPDATED | Desc: Payment status changed to Processing | PaymentId: {PaymentId} | OldStatus: Pending | NewStatus: Processing | CorrId: {CorrId}",
        paymentId, correlationId);

        // Prepare data for frontend to call Bank API
        var stan = $"PSP-{payment.Id}-{DateTime.UtcNow.Ticks}";
        var pspBackendUrl = _configuration["PSPBackendUrl"] ?? "https://localhost:5442";
        var pspFrontendUrl = _configuration["PSPFrontendUrl"] ?? "https://localhost:5174";

        //BACK POZIVA BACK ZBOG SIGURNIH KLJUCEVA
        if (paymentMethod.Code == "PAYPAL")
        {
            _logger.LogInformation("[PSP-PAYMENT] PAYPAL_FLOW | Desc: Initiating PayPal order creation | PaymentId: {PaymentId} | MerchantId: {MerchantId} | Amount: {Amount} {Currency} | CorrId: {CorrId}",
            paymentId, webShop.MerchantId, payment.Amount, payment.Currency, correlationId);
            try
            {
                var payPalRequest = new
                {
                    PspTransactionId = payment.Id.ToString(),
                    Amount = payment.Amount,
                    Currency = payment.Currency.ToString(),
                    MerchantId = webShop.MerchantId,
                    ReturnUrl = $"{pspFrontendUrl}/payment/{payment.Id}", // PayPal će dodati ?token=xxx&PayerID=yyy
                    CancelUrl = $"{pspFrontendUrl}/payment/{payment.Id}?status=cancelled"
                };

                var payPalStart = DateTime.UtcNow;
                var payPalResponse = await CallPayPalAsync(payPalRequest);
                var payPalDuration = DateTime.UtcNow - payPalStart;

                payment.PaymentUrl = payPalResponse.ApprovalUrl;
                await _paymentRepository.UpdateAsync(payment);
                _logger.LogInformation("[PSP-PAYMENT] PAYPAL_SUCCESS | Desc: PayPal order created, approval URL received | PaymentId: {PaymentId} | PayPalOrderId: {PayPalOrderId} | PayPalDurationMs: {PayPalDurationMs} | TotalDurationMs: {TotalDurationMs} | CorrId: {CorrId}",
                paymentId, payPalResponse.PayPalOrderId, payPalDuration.TotalMilliseconds, (DateTime.UtcNow - startTime).TotalMilliseconds, correlationId);

                return new PaymentMethodSelectionResponse
                {
                    MerchantId = webShop.MerchantId,
                    Amount = payment.Amount,
                    Currency = payment.Currency.ToString(),
                    ApprovalUrl = payPalResponse.ApprovalUrl,  // Frontend očekuje ovo polje
                    PayPalOrderId = payPalResponse.PayPalOrderId
                };
            }
            catch (Exception ex)
            {
                payment.Status = PaymentStatus.Failed;
                await _paymentRepository.UpdateAsync(payment);
                _logger.LogError(ex, "[PSP-PAYMENT] PAYPAL_FAILED | Desc: PayPal order creation failed | PaymentId: {PaymentId} | MerchantId: {MerchantId} | ErrorType: {ErrorType} | TotalDurationMs: {DurationMs} | CorrId: {CorrId} | IP: {IP}",
               paymentId, webShop.MerchantId, ex.GetType().Name, (DateTime.UtcNow - startTime).TotalMilliseconds, correlationId, ipAddress);
                throw new InvalidOperationException($"Failed to initiate PayPal payment: {ex.Message}");
            }
        }
        else
        {
            _logger.LogInformation("[PSP-PAYMENT] BANK_FLOW | Desc: Initiating bank payment | PaymentId: {PaymentId} | MerchantId: {MerchantId} | Amount: {Amount} {Currency} | MethodCode: {MethodCode} | Stan: {Stan} | CorrId: {CorrId}",
            paymentId, webShop.MerchantId, payment.Amount, payment.Currency, paymentMethod.Code, stan, correlationId);

            try
            {
                // Pripremi podatke za banku
                var bankRequestData = new
                {
                    MerchantId = webShop.MerchantId,
                    Amount = payment.Amount,
                    Currency = payment.Currency.ToString(),
                    Stan = stan,
                    PspTimestamp = DateTime.UtcNow,
                    PaymentMethodCode = paymentMethod.Code, // CREDIT_CARD ili IPS_SCAN
                    SuccessUrl = $"{pspBackendUrl}/api/payments/{payment.Id}/bank-callback?status=success",
                    FailedUrl = $"{pspBackendUrl}/api/payments/{payment.Id}/bank-callback?status=failed",
                    ErrorUrl = $"{pspBackendUrl}/api/payments/{payment.Id}/bank-callback?status=error"
                };

                // Pozovi banku sa HMAC-om
                var bankStart = DateTime.UtcNow;
                var bankResponse = await CallBankWithHmacAsync(bankRequestData);
                var bankDuration = DateTime.UtcNow - bankStart;

                // Sacuvaj bank podatke
                payment.PaymentUrl = bankResponse.PaymentUrl;
                await _paymentRepository.UpdateAsync(payment);

                _logger.LogInformation("[PSP-PAYMENT] BANK_SUCCESS | Desc: Bank payment URL received | PaymentId: {PaymentId} | BankPaymentId: {BankPaymentId} | Stan: {Stan} | BankDurationMs: {BankDurationMs} | TotalDurationMs: {TotalDurationMs} | CorrId: {CorrId}",
                paymentId, bankResponse.PaymentId, stan, bankDuration.TotalMilliseconds, (DateTime.UtcNow - startTime).TotalMilliseconds, correlationId);

                // Vrati bank payment_url frontendu
                return new PaymentMethodSelectionResponse
                {
                    MerchantId = webShop.MerchantId,
                    Amount = payment.Amount,
                    Currency = payment.Currency.ToString(),
                    Stan = stan,
                    PspTimestamp = DateTime.UtcNow,
                    SuccessUrl = $"{pspBackendUrl}/api/payments/{payment.Id}/bank-callback?status=success",
                    FailedUrl = $"{pspBackendUrl}/api/payments/{payment.Id}/bank-callback?status=failed",
                    ErrorUrl = $"{pspBackendUrl}/api/payments/{payment.Id}/bank-callback?status=error",

                    BankPaymentId = bankResponse.PaymentId,
                    BankPaymentUrl = bankResponse.PaymentUrl  // OVO FRONTEND KORISTI!
                };
            }
            catch (Exception ex)
            {
                // Rollback ako banka odbije
                payment.Status = PaymentStatus.Failed;
                await _paymentRepository.UpdateAsync(payment);
                _logger.LogError(ex, "[PSP-PAYMENT] BANK_FAILED | Desc: Bank payment initiation failed | PaymentId: {PaymentId} | MerchantId: {MerchantId} | Stan: {Stan} | ErrorType: {ErrorType} | TotalDurationMs: {DurationMs} | CorrId: {CorrId} | IP: {IP}",
                paymentId, webShop.MerchantId, stan, ex.GetType().Name, (DateTime.UtcNow - startTime).TotalMilliseconds, correlationId, ipAddress);
                throw new InvalidOperationException($"Failed to initiate payment with bank: {ex.Message}");
            }
        }
    }

    private async Task<BankPaymentResponse> CallBankWithHmacAsync(object requestData)
    {
        try
        {
            var bankConfig = _configuration.GetSection("BankSettings:TestBank");
            var baseUrl = bankConfig["BaseUrl"] ?? "https://localhost:5441";
            var merchantId = bankConfig["MerchantId"];
            var secretKey = bankConfig["SecretKey"];

            var requestBody = System.Text.Json.JsonSerializer.Serialize(requestData);
            var timestamp = DateTime.UtcNow.ToString("o");

            // Generi�i HMAC
            var signature = Utilities.HmacHelper.GenerateSignature(merchantId, timestamp, requestBody, secretKey);

            // Po�alji zahtev banci
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/payment/initiate")
            {
                Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json")
            };

            httpRequest.Headers.Add("X-Merchant-ID", merchantId);
            httpRequest.Headers.Add("X-Timestamp", timestamp);
            httpRequest.Headers.Add("X-Signature", signature);
            httpRequest.Headers.Add("X-Request-ID", Guid.NewGuid().ToString());

            var response = await _httpClient.SendAsync(httpRequest);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Bank API returned {response.StatusCode}");
            }

            // Deserijalizuj sa case-insensitive opcijama
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };

            var bankResponse = System.Text.Json.JsonSerializer.Deserialize<BankPaymentResponse>(responseContent, options);

            if (bankResponse == null)
            {
                throw new InvalidOperationException("Bank response deserialization failed");
            }

            return bankResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, " Error calling bank API");
            throw;
        }
    }

    private async Task<PayPalCreateOrderResponse> CallPayPalAsync(object requestData)
    {
        try
        {
            var payPalConfig = _configuration.GetSection("PayPalSettings");
            var baseUrl = payPalConfig["ApiUrl"] ?? "https://localhost:5443";

            var requestBody = System.Text.Json.JsonSerializer.Serialize(requestData);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/paypal/create-order")
            {
                Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json")
            };

            httpRequest.Headers.Add("X-Request-ID", Guid.NewGuid().ToString()); 

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal API error: {StatusCode}, Response: {Content}",
                    response.StatusCode, responseContent);
                throw new HttpRequestException($"PayPal API returned {response.StatusCode}");
            }

            // Deserializuj odgovor (CamelCase kao kod banke)
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };

            var payPalResponse = System.Text.Json.JsonSerializer.Deserialize<PayPalCreateOrderResponse>(responseContent, options);

            if (payPalResponse == null)
            {
                throw new InvalidOperationException("PayPal response deserialization failed");
            }

            return payPalResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling PayPal API");
            throw;
        }
    }

    // DODAJ OVU KLASU
    public class BankPaymentResponse
    {
        public string PaymentId { get; set; }
        public string PaymentUrl { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Message { get; set; }
    }

    public class PayPalCreateOrderResponse
    {
        public string PayPalOrderId { get; set; } = string.Empty;
        public string ApprovalUrl { get; set; } = string.Empty;  
        public string Status { get; set; } = string.Empty;
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
        public string BankPaymentId { get; set; } = string.Empty;
        public string BankPaymentUrl { get; set; } = string.Empty;
        public string ApprovalUrl { get; set; } = string.Empty;
        public string PayPalOrderId { get; set; } = string.Empty;
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

        // Get payment method name if available
        string paymentMethodCode = "UNKNOWN";
        if (payment.PaymentMethodId.HasValue)
        {
            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(payment.PaymentMethodId.Value);
            if (paymentMethod != null)
            {
                // Map payment method code to frontend format
                paymentMethodCode = paymentMethod.Code switch
                {
                    "CREDIT_CARD" => "CreditCard",
                    "IPS_SCAN" => "QR_CODE",
                    _ => paymentMethod.Code
                };
            }
        }

        // Redirect to WebShop based on status
        string redirectUrl;
        switch (status.ToLower())
        {
            case "success":
                redirectUrl = $"{webShop.SuccessUrl}?paymentId={paymentId}&orderId={payment.MerchantOrderId}&amount={payment.Amount}&currency={payment.Currency}&paymentMethod={paymentMethodCode}";
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

    public async Task<string> HandlePayPalCallbackAsync(int paymentId, string payPalOrderId, string payerId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null) throw new InvalidOperationException("Payment not found");

        var webShop = await _webShopRepository.GetByIdAsync(payment.WebShopId);
        if (webShop == null) throw new InvalidOperationException("WebShop not found");

        // Pozovi PayPal mikroservis da capture-uje
        var payPalConfig = _configuration.GetSection("PayPalSettings");
        var baseUrl = payPalConfig["ApiUrl"] ?? "https://localhost:5443";

        var captureRequest = new
        {
            PayPalOrderId = payPalOrderId,
            PspTransactionId = payment.Id.ToString() 
        };

        var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/paypal/capture-order", captureRequest);
        var result = await response.Content.ReadFromJsonAsync<PayPalCaptureResult>();

        // Ažuriraj status
        if (result?.Success == true)
        {
            payment.Status = PaymentStatus.Success;
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
        }
        await _paymentRepository.UpdateAsync(payment);

        // Generiši redirect URL ka merchantu
        if (payment.Status == PaymentStatus.Success)
        {
            return $"{webShop.SuccessUrl}?paymentId={paymentId}&orderId={payment.MerchantOrderId}&amount={payment.Amount}&currency={payment.Currency}&paymentMethod=PAYPAL";
        }
        else
        {
            return $"{webShop.FailedUrl}?paymentId={paymentId}&orderId={payment.MerchantOrderId}";
        }
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

    public class PayPalCaptureResult
    {
        public bool Success { get; set; }
        public string Status { get; set; }
    }
}

