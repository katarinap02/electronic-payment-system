using PayPal.API.DTOs;
using PayPal.API.Models;
using PayPal.API.Repositories;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;

namespace PayPal.API.Service
{
    public class PayPalService
    {
        private readonly PayPalHttpClient _payPalClient;
        private readonly EncryptionService _encryption;
        private readonly PaypalTransactionRepository _transactionRepo;
        private readonly AuditLogRepository _auditRepo;
        private readonly ILogger<PayPalService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PayPalService(
        IConfiguration config,
        EncryptionService encryption,
        PaypalTransactionRepository transactionRepo,
        AuditLogRepository auditRepo,
        ILogger<PayPalService> logger,
        IHttpContextAccessor httpContextAccessor)
        {
            _encryption = encryption;
            _transactionRepo = transactionRepo;
            _auditRepo = auditRepo;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

            var clientId = config["PAYPAL_CLIENT_ID"]
                ?? throw new Exception("PAYPAL_CLIENT_ID nije postavljen");
            var clientSecret = config["PAYPAL_CLIENT_SECRET"]
                ?? throw new Exception("PAYPAL_CLIENT_SECRET nije postavljen");

            // Sandbox environment - za produkciju koristi LiveEnvironment
            var environment = new SandboxEnvironment(clientId, clientSecret);
            _payPalClient = new PayPalHttpClient(environment);
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, string ipAddress, string userAgent)
        {
            var correlationId = GetCorrelationId();
            var startTime = DateTime.UtcNow;

            try
            {
                var merchantId = request.MerchantId.ToLowerInvariant() + "@example.com";

                // Provera idempotency-ja
                var existingTx = await _transactionRepo.GetByPspTransactionIdAsync(request.PspTransactionId);
                if (existingTx != null && existingTx.Status != PaypalTransaction.PaypalStatus.FAILED)
                {
                    _logger.LogWarning(
                        "[PAYPAL-ORDER] DUPLICATE_DETECTED | Event: IDEMPOTENCY_HIT | Description: Transaction already exists, returning previous order | PspTxId: {PspTxId} | ExistingStatus: {Status} | CreatedAt: {CreatedAt} | CorrId: {CorrId} | IP: {IP}",
                        request.PspTransactionId,
                        existingTx.Status,
                        existingTx.CreatedAt,
                        correlationId,
                        ipAddress);

                    var decryptedOrderId = _encryption.Decrypt(existingTx.EncryptedPayPalOrderId);

                    return new CreateOrderResponse
                    {
                        PayPalOrderId = decryptedOrderId,
                        Status = existingTx.Status.ToString(),
                        ApprovalUrl = ""
                    };
                }

                var payPalRequest = new OrdersCreateRequest();
                payPalRequest.Headers.Add("PayPal-Request-Id", Guid.NewGuid().ToString());

                payPalRequest.RequestBody(new OrderRequest
                {
                    CheckoutPaymentIntent = "CAPTURE",
                    PurchaseUnits = new List<PurchaseUnitRequest>
            {
                new()
                {
                    AmountWithBreakdown = new AmountWithBreakdown
                    {
                        CurrencyCode = request.Currency,
                        Value = request.Amount.ToString("F2")
                    },
                    ReferenceId = request.PspTransactionId,
                    Payee = new Payee { Email = merchantId }
                }
            },
                    ApplicationContext = new ApplicationContext
                    {
                        ReturnUrl = request.ReturnUrl,
                        CancelUrl = request.CancelUrl,
                        BrandName = "Vehicle Rental Agency",
                        LandingPage = "LOGIN",
                        UserAction = "PAY_NOW"
                    }
                });

                _logger.LogInformation(
                    "[PAYPAL-ORDER] REQUEST_SENT | Description: Initiating PayPal order creation | PspTxId: {PspTxId} | MerchantId: {MerchantId} | Amount: {Amount} {Currency} | Intent: {Intent} | CorrId: {CorrId}",
                    request.PspTransactionId,
                    request.MerchantId,
                    request.Amount,
                    request.Currency,
                    "CAPTURE",
                    correlationId);

                var response = await _payPalClient.Execute(payPalRequest);
                var order = response.Result<Order>();
                var encryptedOrderId = _encryption.Encrypt(order.Id);
                var duration = DateTime.UtcNow - startTime;

                _logger.LogInformation(
                    "[PAYPAL-ORDER] CREATED_SUCCESS | Description: PayPal order created and stored | PayPalOrderId: {PayPalOrderId} | PspTxId: {PspTxId} | MerchantId: {MerchantId} | Amount: {Amount} {Currency} | PayPalStatus: {PayPalStatus} | DurationMs: {DurationMs} | CorrId: {CorrId} | IP: {IP}",
                    order.Id,
                    request.PspTransactionId,
                    request.MerchantId,
                    request.Amount,
                    request.Currency,
                    order.Status,
                    duration.TotalMilliseconds,
                    correlationId,
                    ipAddress);

                var transaction = new PaypalTransaction
                {
                    PspTransactionId = request.PspTransactionId,
                    EncryptedPayPalOrderId = encryptedOrderId,
                    MerchantId = request.MerchantId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Status = PaypalTransaction.PaypalStatus.PENDING,
                    CreatedByIp = ipAddress,
                    UserAgent = userAgent,
                    CreatedAt = DateTime.UtcNow
                };

                await _transactionRepo.CreateAsync(transaction);

                _logger.LogDebug(
                    "[PAYPAL-ORDER] PERSISTED | Description: Transaction saved to database | PspTxId: {PspTxId} | TxId: {TxId} | Status: {Status}",
                    request.PspTransactionId,
                    transaction.Id,
                    transaction.Status);

                var approvalUrl = order.Links.FirstOrDefault(l => l.Rel == "approve")?.Href;

                if (string.IsNullOrEmpty(approvalUrl))
                {
                    _logger.LogError(
                        "[PAYPAL-ORDER] APPROVAL_URL_MISSING | Description: PayPal response valid but approval URL not found in links | PayPalOrderId: {PayPalOrderId} | PspTxId: {PspTxId} | AvailableLinks: {Links} | CorrId: {CorrId}",
                        order.Id,
                        request.PspTransactionId,
                        string.Join(",", order.Links.Select(l => l.Rel)),
                        correlationId);

                    throw new Exception("PayPal nije vratio approval URL");
                }

                _logger.LogInformation(
                    "[PAYPAL-ORDER] COMPLETED | Description: Order creation flow finished successfully, returning approval URL to client | PayPalOrderId: {PayPalOrderId} | PspTxId: {PspTxId} | TotalDurationMs: {TotalDurationMs} | CorrId: {CorrId}",
                    order.Id,
                    request.PspTransactionId,
                    (DateTime.UtcNow - startTime).TotalMilliseconds,
                    correlationId);

                return new CreateOrderResponse
                {
                    PayPalOrderId = order.Id,
                    ApprovalUrl = approvalUrl,
                    Status = "PENDING"
                };
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;

                _logger.LogError(
                    ex,
                    "[PAYPAL-ORDER] FAILED | Description: Order creation failed with exception | PspTxId: {PspTxId} | MerchantId: {MerchantId} | ErrorPhase: {Phase} | ErrorType: {ErrorType} | ErrorMessage: {ErrorMessage} | DurationMs: {DurationMs} | CorrId: {CorrId} | IP: {IP}",
                    request.PspTransactionId,
                    request.MerchantId,
                    ex.StackTrace?.Contains("Execute") == true ? "PAYPAL_API_CALL" : "INTERNAL_PROCESSING",
                    ex.GetType().Name,
                    ex.Message,
                    duration.TotalMilliseconds,
                    correlationId,
                    ipAddress);

                throw;
            }
        }

        //Provera sta se desilo sa placanjem
        public async Task<bool> CaptureOrderAsync(string payPalOrderId, string pspTransactionId, string ipAddress)
        {
            var correlationId = GetCorrelationId();
            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("[PAYPAL-CAPTURE] STARTED | Desc: Initiating capture | PayPalOrderId: {PayPalOrderId} | PspTxId: {PspTxId} | CorrId: {CorrId} | IP: {IP}",
                    payPalOrderId, pspTransactionId, correlationId, ipAddress);

                var request = new OrdersCaptureRequest(payPalOrderId);
                request.Headers.Add("PayPal-Request-Id", Guid.NewGuid().ToString());
                request.RequestBody(new OrderActionRequest());

                var response = await _payPalClient.Execute(request);
                var order = response.Result<Order>();
                var duration = DateTime.UtcNow - startTime;

                if (order.Status == "COMPLETED")
                {
                    var payerId = order.Payer?.PayerId;
                    var captureId = order.PurchaseUnits?.FirstOrDefault()?.Payments?.Captures?.FirstOrDefault()?.Id;

                    await UpdateTransactionStatusAsync(pspTransactionId, PaypalTransaction.PaypalStatus.CAPTURED, payerId, captureId);

                    _logger.LogInformation("[PAYPAL-CAPTURE] SUCCESS | Desc: Payment captured and stored | PayPalOrderId: {PayPalOrderId} | PspTxId: {PspTxId} | CaptureId: {CaptureId} | PayerId: {PayerId} | DurationMs: {DurationMs} | CorrId: {CorrId}",
                        payPalOrderId, pspTransactionId, captureId, payerId, duration.TotalMilliseconds, correlationId);

                    return true;
                }

                _logger.LogWarning("[PAYPAL-CAPTURE] INCOMPLETE | Desc: Capture executed but status not COMPLETED | PayPalOrderId: {PayPalOrderId} | PspTxId: {PspTxId} | ActualStatus: {Status} | CorrId: {CorrId}",
                    payPalOrderId, pspTransactionId, order.Status, correlationId);

                return false;
            }
            catch (HttpException ex) when (ex.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                _logger.LogWarning("[PAYPAL-CAPTURE] ALREADY_CAPTURED | Desc: Duplicate capture attempt detected, verifying status | PayPalOrderId: {PayPalOrderId} | PspTxId: {PspTxId} | CorrId: {CorrId}",
                    payPalOrderId, pspTransactionId, correlationId);

                try
                {
                    var getRequest = new OrdersGetRequest(payPalOrderId);
                    var getResponse = await _payPalClient.Execute(getRequest);
                    var order = getResponse.Result<Order>();

                    if (order.Status == "COMPLETED")
                    {
                        var existingTx = await _transactionRepo.GetByPspTransactionIdAsync(pspTransactionId);
                        if (existingTx?.Status != PaypalTransaction.PaypalStatus.CAPTURED)
                        {
                            await UpdateTransactionStatusAsync(pspTransactionId, PaypalTransaction.PaypalStatus.CAPTURED);
                        }

                        _logger.LogInformation("[PAYPAL-CAPTURE] RECONCILED | Desc: Status synced with PayPal | PayPalOrderId: {PayPalOrderId} | PspTxId: {PspTxId} | CorrId: {CorrId}",
                            payPalOrderId, pspTransactionId, correlationId);

                        return true;
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "[PAYPAL-CAPTURE] RECONCILE_FAILED | Desc: Could not verify status after duplicate capture error | PayPalOrderId: {PayPalOrderId} | PspTxId: {PspTxId} | CorrId: {CorrId}",
                        payPalOrderId, pspTransactionId, correlationId);
                }

                return false;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;

                _logger.LogError(ex, "[PAYPAL-CAPTURE] FAILED | Desc: Capture failed with exception | PayPalOrderId: {PayPalOrderId} | PspTxId: {PspTxId} | ErrorType: {ErrorType} | DurationMs: {DurationMs} | CorrId: {CorrId} | IP: {IP}",
                    payPalOrderId, pspTransactionId, ex.GetType().Name, duration.TotalMilliseconds, correlationId, ipAddress);

                throw;
            }
        }

        //dohvati status transakcije ako treba za psp
        public async Task<PaypalTransaction?> GetTransactionAsync(string pspTransactionId)
        {
            var correlationId = GetCorrelationId();

            _logger.LogDebug("[PAYPAL-GETTX] LOOKUP | Desc: Fetching transaction | PspTxId: {PspTxId} | CorrId: {CorrId}",
                pspTransactionId, correlationId);

            var transaction = await _transactionRepo.GetByPspTransactionIdAsync(pspTransactionId);

            if (transaction != null)
            {
                _logger.LogDebug("[PAYPAL-GETTX] FOUND | Desc: Transaction retrieved | PspTxId: {PspTxId} | Status: {Status} | CreatedAt: {CreatedAt} | CorrId: {CorrId}",
                    pspTransactionId, transaction.Status, transaction.CreatedAt, correlationId);
            }
            else
            {
                _logger.LogWarning("[PAYPAL-GETTX] NOT_FOUND | Desc: Transaction does not exist | PspTxId: {PspTxId} | CorrId: {CorrId}",
                    pspTransactionId, correlationId);
            }

            return transaction;
        }

        public async Task UpdateTransactionStatusAsync(string pspTransactionId, PaypalTransaction.PaypalStatus status, string? payerId = null, string? captureId = null)
        {
            var correlationId = GetCorrelationId();

            _logger.LogInformation("[PAYPAL-UPDATE] START | Desc: Updating transaction status | PspTxId: {PspTxId} | NewStatus: {NewStatus} | HasPayerId: {HasPayer} | HasCaptureId: {HasCapture} | CorrId: {CorrId}",
                pspTransactionId, status, !string.IsNullOrEmpty(payerId), !string.IsNullOrEmpty(captureId), correlationId);

            var transaction = await _transactionRepo.GetByPspTransactionIdAsync(pspTransactionId);

            if (transaction == null)
            {
                _logger.LogError("[PAYPAL-UPDATE] FAILED | Desc: Transaction not found for update | PspTxId: {PspTxId} | IntendedStatus: {Status} | CorrId: {CorrId}",
                    pspTransactionId, status, correlationId);
                return;
            }

            var oldStatus = transaction.Status;

            transaction.Status = status;
            transaction.CompletedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(payerId))
                transaction.EncryptedPayerId = _encryption.Encrypt(payerId);

            if (!string.IsNullOrEmpty(captureId))
                transaction.EncryptedCaptureId = _encryption.Encrypt(captureId);

            await _transactionRepo.UpdateAsync(transaction);

            _logger.LogInformation("[PAYPAL-UPDATE] SUCCESS | Desc: Transaction status changed | PspTxId: {PspTxId} | StatusChange: {OldStatus} -> {NewStatus} | CompletedAt: {CompletedAt} | CorrId: {CorrId}",
                pspTransactionId, oldStatus, status, transaction.CompletedAt, correlationId);
        }

        private string GetCorrelationId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return Guid.NewGuid().ToString("N")[..12];

            // Prvo proveri da li postoji u header-u (od drugog servisa)
            var correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault();

            if (!string.IsNullOrEmpty(correlationId))
                return correlationId;

            // Ako ne postoji, proveri da li ga je middleware već stavio u Items
            if (httpContext.Items.TryGetValue("CorrelationId", out var existingId))
                return existingId?.ToString() ?? Guid.NewGuid().ToString("N")[..12];

            // Generiši novi
            var newId = Guid.NewGuid().ToString("N")[..12];
            httpContext.Items["CorrelationId"] = newId;
            return newId;
        }

        
    }
}
