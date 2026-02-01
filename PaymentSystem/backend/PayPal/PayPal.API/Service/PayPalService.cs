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

        public PayPalService(
        IConfiguration config,
        EncryptionService encryption,
        PaypalTransactionRepository transactionRepo,
        AuditLogRepository auditRepo,
        ILogger<PayPalService> logger)
        {
            _encryption = encryption;
            _transactionRepo = transactionRepo;
            _auditRepo = auditRepo;
            _logger = logger;

            var clientId = config["PAYPAL_CLIENT_ID"]
                ?? throw new Exception("PAYPAL_CLIENT_ID nije postavljen");
            var clientSecret = config["PAYPAL_CLIENT_SECRET"]
                ?? throw new Exception("PAYPAL_CLIENT_SECRET nije postavljen");

            // Sandbox environment - za produkciju koristi LiveEnvironment
            var environment = new SandboxEnvironment(clientId, clientSecret);
            _payPalClient = new PayPalHttpClient(environment);
        }

        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request,string ipAddress,string userAgent)
        {
            try
            {
                // sprečavanje duplog plaćanja 
                // Ako već postoji transakcija sa istim PSP ID-jem, vrati postojeću
                var existingTx = await _transactionRepo.GetByPspTransactionIdAsync(request.PspTransactionId);
                if (existingTx != null && existingTx.Status != PaypalTransaction.PaypalStatus.FAILED)
                {
                    _logger.LogWarning("Duplikat transakcije detektovan: {PspId}", request.PspTransactionId);

                    // Vrati postojeći approval URL (idempotency)
                    var decryptedOrderId = _encryption.Decrypt(existingTx.EncryptedPayPalOrderId);
                    return new CreateOrderResponse
                    {
                        PayPalOrderId = decryptedOrderId,
                        Status = existingTx.Status.ToString(),
                        // Vratiti approval URL iz baze ili generisati ponovo?
                        ApprovalUrl = "" // Ovo ćemo doraditi kasnije
                    };
                }

                var payPalRequest = new OrdersCreateRequest();

                //  PayPal garantuje da će sa istim ključem vratiti istu transakciju
                payPalRequest.Headers.Add("PayPal-Request-Id", Guid.NewGuid().ToString());

                payPalRequest.RequestBody(new OrderRequest
                {
                    CheckoutPaymentIntent = "CAPTURE", // Odmah naplati 
                    PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new()
                    {
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = request.Currency,
                            Value = request.Amount.ToString("F2")
                        },
                        ReferenceId = request.PspTransactionId // Veza sa našim sistemom
                    }
                },
                    ApplicationContext = new ApplicationContext
                    {
                        ReturnUrl = request.ReturnUrl,
                        CancelUrl = request.CancelUrl,
                        BrandName = "Vehicle Rental Agency", // Ime agencije iz specifikacije
                        LandingPage = "LOGIN", // Direktno na PayPal login
                        UserAction = "PAY_NOW"
                    }
                });

                // Izvrši poziv ka PayPal-u
                var response = await _payPalClient.Execute(payPalRequest);
                var order = response.Result<Order>();

                // PayPal Order ID enkriptunje
                var encryptedOrderId = _encryption.Encrypt(order.Id);

                _logger.LogInformation("PayPal Order kreiran: {OrderId}, PSP: {PspId}",
                    encryptedOrderId, request.PspTransactionId);

                //  ČUVANJE U BAZI
                var transaction = new PaypalTransaction
                {
                    PspTransactionId = request.PspTransactionId,
                    EncryptedPayPalOrderId = encryptedOrderId,
                    MerchantId = request.MerchantId, // Izdvoji merchant deo
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Status = PaypalTransaction.PaypalStatus.PENDING,
                    CreatedByIp = ipAddress,      
                    UserAgent = userAgent,        
                    CreatedAt = DateTime.UtcNow
                };

                await _transactionRepo.CreateAsync(transaction);

                // 5.1 zahtev
                await _auditRepo.LogAsync(
                    action: "CREATE_ORDER",
                    transactionId: request.PspTransactionId,
                    ipAddress: ipAddress,
                    result: "SUCCESS",
                    details: $"PayPal OrderId: {encryptedOrderId}, Amount: {request.Amount} {request.Currency}"
                );

                // Pronađi approval URL (gde korisnik treba da ode da plati)
                var approvalUrl = order.Links.FirstOrDefault(l => l.Rel == "approve")?.Href;

                if (string.IsNullOrEmpty(approvalUrl))
                    throw new Exception("PayPal nije vratio approval URL");

                return new CreateOrderResponse
                {
                    PayPalOrderId = order.Id, // Plain text - vraćamo PSP-u privremeno
                    ApprovalUrl = approvalUrl,
                    Status = "PENDING"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri kreiranju PayPal ordere za PSP: {PspId}",
                    request.PspTransactionId);

                // Loguj neuspeh u audit 
                await _auditRepo.LogAsync(
                    action: "CREATE_ORDER",
                    transactionId: request.PspTransactionId,
                    ipAddress: ipAddress,
                    result: "FAILURE",
                    details: ex.Message
                );

                throw;
            }
        }

        //Provera sta se desilo sa placanjem
        public async Task<bool> CaptureOrderAsync(string payPalOrderId, string pspTransactionId, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Capture attempt for PayPal Order: {OrderId}, PSP: {PspId}",
                    payPalOrderId, pspTransactionId);

                //  Poziva PayPal API da potvrdi plaćanje
                var request = new OrdersCaptureRequest(payPalOrderId);
                request.Headers.Add("PayPal-Request-Id", Guid.NewGuid().ToString());
                request.RequestBody(new OrderActionRequest());

                // Ako korisnik već platio, PayPal će vratiti "UNPROCESSABLE_ENTITY" i poruku da je već captured

                var response = await _payPalClient.Execute(request);
                var order = response.Result<Order>();

                //  Proverava da li je uspešno
                if (order.Status == "COMPLETED")
                {
                    // Pronađi PayerID i CaptureID za našu bazu
                    var payerId = order.Payer?.PayerId;
                    var captureId = order.PurchaseUnits?.FirstOrDefault()?.Payments?.Captures?.FirstOrDefault()?.Id;

                    //  Ažuriraj transakciju u bazi
                    await UpdateTransactionStatusAsync(
                        pspTransactionId,
                        PaypalTransaction.PaypalStatus.CAPTURED,
                        payerId,
                        captureId
                    );

                    await _auditRepo.LogAsync(
                        action: "CAPTURE_ORDER",
                        transactionId: pspTransactionId,
                        ipAddress: ipAddress,
                        result: "SUCCESS",
                        details: $"PayPal Order completed. PayerID: {payerId}, CaptureID: {captureId}"
                    );

                    _logger.LogInformation("Payment captured successfully: {PspId}", pspTransactionId);
                    return true;
                }

                await _auditRepo.LogAsync(
                    action: "CAPTURE_ORDER",
                    transactionId: pspTransactionId,
                    ipAddress: ipAddress,
                    result: "FAILED",
                    details: $"PayPal status: {order.Status}"
                );

                return false;
            }
            catch (HttpException ex) when (ex.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                //uradjen refresh stranice
                _logger.LogWarning("Order already captured or invalid: {OrderId}", payPalOrderId);

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
                        return true;
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Failed to get order status after capture error");
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing PayPal order: {OrderId}", payPalOrderId);

                await _auditRepo.LogAsync(
                    action: "CAPTURE_ORDER",
                    transactionId: pspTransactionId,
                    ipAddress: ipAddress,
                    result: "ERROR",
                    details: ex.Message
                );

                throw; 
            }
        }

        //dohvati status transakcije ako treba za psp
        public async Task<PaypalTransaction?> GetTransactionAsync(string pspTransactionId)
        {
            var transaction = await _transactionRepo.GetByPspTransactionIdAsync(pspTransactionId);

            if (transaction != null)
            {
                await _auditRepo.LogAsync(
                    action: "VIEW_TRANSACTION",
                    transactionId: pspTransactionId,
                    ipAddress: "SYSTEM", // Ovo ćemo doraditi da uzima IP iz kontrolera
                    result: "SUCCESS"
                );
            }

            return transaction;
        }

        //ovo se poziva kad paypal vrati callback
        public async Task UpdateTransactionStatusAsync(
        string pspTransactionId,
        PaypalTransaction.PaypalStatus status,
        string? payerId = null,
        string? captureId = null)
        {
            var transaction = await _transactionRepo.GetByPspTransactionIdAsync(pspTransactionId);
            if (transaction == null) return;

            transaction.Status = status;
            transaction.CompletedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(payerId))
                transaction.EncryptedPayerId = _encryption.Encrypt(payerId);

            if (!string.IsNullOrEmpty(captureId))
                transaction.EncryptedCaptureId = _encryption.Encrypt(captureId);

            await _transactionRepo.UpdateAsync(transaction);

            await _auditRepo.LogAsync(
                action: "UPDATE_STATUS",
                transactionId: pspTransactionId,
                ipAddress: "CALLBACK",
                result: "SUCCESS",
                details: $"New status: {status}"
            );
        }
    }
}
