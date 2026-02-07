using Bank.API.DTOs;
using Bank.API.Models;
using Bank.API.Repositories;
using System.Security.Cryptography;

namespace Bank.API.Services
{
    public class PaymentService
    {
        private readonly PaymentTransactionRepository _transactionRepo;
        private readonly BankAccountRepository _accountRepo;
        private readonly CardRepository _cardRepo;
        private readonly CardTokenRepository _tokenRepo;
        private readonly CardService _cardService;
        private readonly NbsQrCodeService _qrCodeService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentService(
            PaymentTransactionRepository transactionRepo,
            BankAccountRepository accountRepo,
            CardRepository cardRepo,
            CardTokenRepository tokenRepo,
            IConfiguration configuration,
            ILogger<PaymentService> logger, 
            CardService cardService,
            NbsQrCodeService qrCodeService,
            IHttpContextAccessor httpContextAccessor)
        {
            _transactionRepo = transactionRepo;
            _accountRepo = accountRepo;
            _cardRepo = cardRepo;
            _tokenRepo = tokenRepo;
            _cardService = cardService;
            _qrCodeService = qrCodeService;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public PaymentResponse CreatePaymentUrl(PaymentRequest request)
        {
            var correlationId = GetCorrelationId();
            var startTime = DateTime.UtcNow;
            try
            {
                // Validacija PSP autentifikacije (HMAC ili sertifikat)
                
                if (!ValidatePspRequest(request))
                {
                    _logger.LogWarning(
                "[BANK-AUDIT] Authentication failed | " +
                "EventType: PSP_AUTH_FAILURE | " +
                "MerchantId: {MerchantId} | " +
                "Timestamp: {Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} | " +
                "CorrelationId: {CorrelationId} | " +
                "SourceIP: {SourceIP} | " +
                "FailureReason: HMAC_INVALID",
                request.MerchantId,
                DateTime.UtcNow,
                correlationId,
                GetClientIp());
                    throw new UnauthorizedAccessException("Invalid PSP credentials");
                }

                //  validacija MERCHANT_ID
                var merchantAccount = _accountRepo.GetMerchantByMerchantId(request.MerchantId);
                if (merchantAccount == null)
                {
                    _logger.LogWarning(
                "[BANK-AUDIT] Merchant validation failed | " +
                "EventType: MERCHANT_NOT_FOUND | " +
                "MerchantId: {MerchantId} | " +
                "Timestamp: {Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} | " +
                "CorrelationId: {CorrelationId} | " +
                "SourceIP: {SourceIP}",
                request.MerchantId,
                DateTime.UtcNow,
                correlationId,
                GetClientIp());
                    throw new ArgumentException("Merchant not found or inactive");
                }

                // 2. Provera duple transakcije po STAN-u (jedinstven izmedu PSP-a i banke)
                if (_transactionRepo.HasDuplicateTransactionByStan(request.Stan))
                {
                    _logger.LogWarning(
                "[BANK-AUDIT] Duplicate transaction detected | " +
                "EventType: DUPLICATE_STAN | " +
                "Stan: {Stan} | " +
                "MerchantId: {MerchantId} | " +
                "Timestamp: {Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} | " +
                "CorrelationId: {CorrelationId} | " +
                "SourceIP: {SourceIP}",
                request.Stan,
                request.MerchantId,
                DateTime.UtcNow,
                correlationId,
                GetClientIp());

                    // Pronadi postojecu transakciju sa ovim STAN-om
                    var existingTransaction = _transactionRepo.GetByStan(request.Stan);
                    if (existingTransaction != null && !string.IsNullOrEmpty(existingTransaction.PaymentId))
                    {
                        // Vrati postojecu ako je duplikat (idempotentnost)
                        _logger.LogInformation(
                    "[BANK-AUDIT] Returning existing payment URL (idempotent) | " +
                    "EventType: PAYMENT_URL_IDEMPOTENT | " +
                    "OriginalPaymentId: {OriginalPaymentId} | " +
                    "Stan: {Stan} | " +
                    "MerchantId: {MerchantId} | " +
                    "Timestamp: {Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} | " +
                    "CorrelationId: {CorrelationId}",
                    existingTransaction.PaymentId,
                    request.Stan,
                    request.MerchantId,
                    DateTime.UtcNow,
                    correlationId);
                        return new PaymentResponse
                        {
                            PaymentUrl = GeneratePaymentUrl(existingTransaction.PaymentId),
                            PaymentId = existingTransaction.PaymentId,
                            ExpiresAt = existingTransaction.ExpiresAt,
                            Message = "Duplicate STAN detected. Returning existing payment URL."
                        };
                    }

                    throw new InvalidOperationException($"Duplicate STAN: {request.Stan}");
                }

                // Generi�e PAYMENT_ID i PAYMENT_URL 
                var paymentId = GenerateSecurePaymentId();
                var paymentUrl = GeneratePaymentUrl(paymentId);

                // Kreira transakciju (STAN za pracenje transakcije)
                var transaction = _transactionRepo.CreateTransaction(
                    merchantId: request.MerchantId,
                    amount: request.Amount,
                    currency: request.Currency,
                    stan: request.Stan,
                    merchantTimestamp: request.PspTimestamp,
                    paymentId: paymentId,
                    merchantAccountId: merchantAccount.Id,
                    successUrl: request.SuccessUrl,
                    failedUrl: request.FailedUrl,
                    errorUrl: request.ErrorUrl,
                    paymentMethodCode: request.PaymentMethodCode);

                

                // 6. Sacuvaj PAYMENT_ID u transakciju
                transaction.PaymentId = paymentId;
                _transactionRepo.UpdatePaymentId(transaction.Id, paymentId);

                var duration = DateTime.UtcNow - startTime;

                _logger.LogInformation(
                    "[BANK-AUDIT] Payment URL created successfully | " +
                    "EventType: PAYMENT_URL_CREATED | " +
                    "PaymentId: {PaymentId} | " +
                    "MerchantId: {MerchantId} | " +
                    "MerchantAccountId: {MerchantAccountId} | " +
                    "Stan: {Stan} | " +
                    "Amount: {Amount} | " +
                    "Currency: {Currency} | " +
                    "PaymentMethod: {PaymentMethod} | " +
                    "Timestamp: {Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} | " +
                    "CorrelationId: {CorrelationId} | " +
                    "SourceIP: {SourceIP} | " +
                    "DurationMs: {DurationMs} | " +
                    "ExpiresAt: {ExpiresAt:yyyy-MM-ddTHH:mm:ss.fffZ}",
                    paymentId,
                    request.MerchantId,
                    merchantAccount.Id,
                    request.Stan,
                    request.Amount,
                    request.Currency,
                    request.PaymentMethodCode ?? "CARD",
                    DateTime.UtcNow,
                    correlationId,
                    GetClientIp(),
                    duration.TotalMilliseconds,
                    transaction.ExpiresAt);

                return new PaymentResponse
                {
                    PaymentUrl = paymentUrl,
                    PaymentId = paymentId,
                    ExpiresAt = transaction.ExpiresAt,
                    Message = "Payment URL created successfully"
                };
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;

                _logger.LogError(
                    ex,
                    "[BANK-AUDIT] Payment URL creation failed | " +
                    "EventType: PAYMENT_URL_ERROR | " +
                    "MerchantId: {MerchantId} | " +
                    "Stan: {Stan} | " +
                    "Timestamp: {Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} | " +
                    "CorrelationId: {CorrelationId} | " +
                    "SourceIP: {SourceIP} | " +
                    "DurationMs: {DurationMs} | " +
                    "ErrorType: {ErrorType} | " +
                    "ErrorMessage: {ErrorMessage}",
                    request.MerchantId,
                    request.Stan,
                    DateTime.UtcNow,
                    correlationId,
                    GetClientIp(),
                    duration.TotalMilliseconds,
                    ex.GetType().Name,
                    ex.Message);
                throw;
            }
        }

        //dobijanje forme za placanje
        public PaymentFormResponse GetPaymentForm(string paymentId)
        {
            var correlationId = GetCorrelationId();
            var startTime = DateTime.UtcNow;
            try
            {
                var transaction = _transactionRepo.GetByPaymentId(paymentId);
                if (transaction == null)
                {
                    _logger.LogWarning(
                "[BANK-AUDIT] Payment form request failed | " +
                "EventType: PAYMENT_FORM_NOT_FOUND | " +
                "PaymentId: {PaymentId} | " +
                "Timestamp: {Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} | " +
                "CorrelationId: {CorrelationId} | " +
                "SourceIP: {SourceIP} | " +
                "FailureReason: INVALID_PAYMENT_ID",
                paymentId,
                DateTime.UtcNow,
                correlationId,
                GetClientIp());
                    throw new ArgumentException("Invalid payment ID");
                }
                    

                // Proveri da li je transakcija jos uvek validna 
                if (!_transactionRepo.IsTransactionValid(transaction.Id))
                {
                    _logger.LogWarning(
                "[BANK-AUDIT] Payment form request failed | " +
                "EventType: PAYMENT_FORM_EXPIRED | " +
                "PaymentId: {PaymentId} | " +
                "TransactionId: {TransactionId} | " +
                "MerchantId: {MerchantId} | " +
                "Timestamp: {Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} | " +
                "CorrelationId: {CorrelationId} | " +
                "SourceIP: {SourceIP} | " +
                "FailureReason: TRANSACTION_EXPIRED | " +
                "ExpiresAt: {ExpiresAt:yyyy-MM-ddTHH:mm:ss.fffZ}",
                paymentId,
                transaction.Id,
                transaction.MerchantId,
                DateTime.UtcNow,
                correlationId,
                GetClientIp(),
                transaction.ExpiresAt);
                    throw new InvalidOperationException("Payment form has expired");
                }

                var merchantAccount = _accountRepo.GetMerchantByMerchantId(transaction.MerchantId);
                if (merchantAccount == null)
                {
                    _logger.LogWarning(
                "[BANK-AUDIT] Payment form request failed | " +
                "EventType: PAYMENT_FORM_MERCHANT_NOT_FOUND | " +
                "PaymentId: {PaymentId} | " +
                "TransactionId: {TransactionId} | " +
                "MerchantId: {MerchantId} | " +
                "Timestamp: {Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} | " +
                "CorrelationId: {CorrelationId} | " +
                "SourceIP: {SourceIP} | " +
                "FailureReason: MERCHANT_NOT_FOUND",
                paymentId,
                transaction.Id,
                transaction.MerchantId,
                DateTime.UtcNow,
                correlationId,
                GetClientIp());
                    throw new ArgumentException("Merchant not found");
                }
                    

                // Detektuj da li je QR placanje na osnovu PaymentMethodCode
                bool isQrPayment = transaction.PaymentMethodCode == "IPS_SCAN";
                var duration = DateTime.UtcNow - startTime;

                _logger.LogInformation(
            "[BANK-AUDIT] Payment form displayed | " +
            "EventType: PAYMENT_FORM_DISPLAYED | " +
            "PaymentId: {PaymentId} | " +
            "TransactionId: {TransactionId} | " +
            "MerchantId: {MerchantId} | " +
            "Amount: {Amount} | " +
            "Currency: {Currency} | " +
            "PaymentMethod: {PaymentMethod} | " +
            "TransactionStatus: {TransactionStatus} | " +
            "Timestamp: {Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} | " +
            "CorrelationId: {CorrelationId} | " +
            "SourceIP: {SourceIP} | " +
            "DurationMs: {DurationMs} | " +
            "ExpiresAt: {ExpiresAt:yyyy-MM-ddTHH:mm:ss.fffZ} | " +
            "TimeToExpireMinutes: {TimeToExpireMinutes}",
            paymentId,
            transaction.Id,
            transaction.MerchantId,
            transaction.Amount,
            transaction.Currency,
            isQrPayment ? "QR" : "CARD",
            transaction.Status,
            DateTime.UtcNow,
            correlationId,
            GetClientIp(),
            duration.TotalMilliseconds,
            transaction.ExpiresAt,
            (transaction.ExpiresAt - DateTime.UtcNow).TotalMinutes);

                return new PaymentFormResponse
                {
                    PaymentId = paymentId,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    MerchantName = merchantAccount.Customer?.FullName ?? "Unknown Merchant",
                    ExpiresAt = transaction.ExpiresAt,
                    IsQrPayment = isQrPayment
                };


            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;

                _logger.LogError(
                    ex,
                    "[BANK-AUDIT] Payment form request error | " +
                    "EventType: PAYMENT_FORM_ERROR | " +
                    "PaymentId: {PaymentId} | " +
                    "Timestamp: {Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} | " +
                    "CorrelationId: {CorrelationId} | " +
                    "SourceIP: {SourceIP} | " +
                    "DurationMs: {DurationMs} | " +
                    "ErrorType: {ErrorType} | " +
                    "ErrorMessage: {ErrorMessage}",
                    paymentId,
                    DateTime.UtcNow,
                    correlationId,
                    GetClientIp(),
                    duration.TotalMilliseconds,
                    ex.GetType().Name,
                    ex.Message);
                throw;
            }
        }
        
        //Karticno placanje
        public PaymentStatusResponse ProcessCardPayment(CardInformation cardInfo)
        {
            var correlationId = GetCorrelationId();
            var startTime = DateTime.UtcNow;
            var cardHash = _cardService.GenerateCardHash(cardInfo.CardNumber);
            try
            {
                
                // Validacija PAYMENT_ID i transakcije
                var transaction = _transactionRepo.GetByPaymentId(cardInfo.PaymentId);
                if (transaction == null)
                {
                    _logger.LogWarning("[BANK-PAYMENT] FAILED | Event: TX_NOT_FOUND | PaymentId: {PaymentId} | CardHash: {CardHash} | CorrId: {CorrId} | IP: {IP}",
                cardInfo.PaymentId, cardHash, correlationId, GetClientIp());
                    throw new ArgumentException("Invalid payment ID");
                }
                    

                // Proverava da li je expired i da li je status Pending
                if (!_transactionRepo.IsTransactionValid(transaction.Id))
                {
                    _logger.LogWarning("[BANK-PAYMENT] FAILED | Event: TX_EXPIRED | PaymentId: {PaymentId} | TxId: {TxId} | MerchantId: {MerchantId} | CardHash: {CardHash} | CorrId: {CorrId}",
                cardInfo.PaymentId, transaction.Id, transaction.MerchantId, cardHash, correlationId);
                    throw new InvalidOperationException("Transaction has expired");
                }


                // Validacija kartice
                _logger.LogInformation("[BANK-PAYMENT] VALIDATING | PaymentId: {PaymentId} | TxId: {TxId} | MerchantId: {MerchantId} | Amount: {Amount} {Currency} | CardHash: {CardHash} | CorrId: {CorrId}",
            cardInfo.PaymentId, transaction.Id, transaction.MerchantId, transaction.Amount, transaction.Currency, cardHash, correlationId);
                if (!ValidateCardInformation(cardInfo, transaction))
                    throw new InvalidOperationException("Invalid card information");

                // Provera Luhn algoritma 
                if (!_cardRepo.ValidateCardNumber(cardInfo.CardNumber))
                    throw new InvalidOperationException("Invalid card number");

                // Provera datuma ( format MM/YY)
                var expiryParts = cardInfo.ExpiryDate.Split('/');
                if (expiryParts.Length != 2 ||
                    !_cardRepo.ValidateExpiryDate(expiryParts[0], expiryParts[1]))
                    throw new InvalidOperationException("Card has expired or invalid date format");

                
                 var card = _cardRepo.FindCardByHash(cardHash); 
               

                if (card == null)
                {
                    _logger.LogWarning("[BANK-PAYMENT] FAILED | Event: CARD_NOT_FOUND | PaymentId: {PaymentId} | CardHash: {CardHash} | CorrId: {CorrId}",
                cardInfo.PaymentId, cardHash, correlationId);
                    throw new InvalidOperationException("Card not found");
                }
                    

                if (!ValidateCardholderName(cardInfo.CardholderName, card.CardholderName))
                {
                    throw new InvalidOperationException("Cardholder name does not match");
                }

                if (!ValidateExpiryDate(expiryParts[0], expiryParts[1], card.ExpiryMonth, card.ExpiryYear))
                {
                    throw new InvalidOperationException("Card expiry date does not match");
                }

                cardInfo.Cvv = null;

                //Tokenizacija kartice (PCI DSS compliance)
                var cardToken = _tokenRepo.CreateToken(card.Id, transaction.Id);

                // 6. Pronadi customer account (posto je sve u istoj banci)
                // lookup po PAN-u ili customer ID-u
                var customerAccount = _accountRepo.FindCustomerAccount(card.CustomerId);
                if (customerAccount == null)
                    throw new InvalidOperationException("Account not found");

                _transactionRepo.LinkCardAndAccountToTransaction(
                    transaction.Id,
                    card.Id,
                    customerAccount.Id,
                    card.CustomerId);

                // Rezervacija sredstava 
                if (!_accountRepo.ReserveFunds(customerAccount.Id, transaction.Amount, transaction.Currency))
                {
                    _logger.LogWarning("[BANK-PAYMENT] FAILED | Event: INSUFFICIENT_FUNDS | PaymentId: {PaymentId} | TxId: {TxId} | CardHash: {CardHash} | AccountId: {AccountId} | CorrId: {CorrId}",
                cardInfo.PaymentId, transaction.Id, cardHash, customerAccount.Id, correlationId);
                    throw new InvalidOperationException("Insufficient funds");
                }
                    

                // Autorizacija transakcije
                _transactionRepo.UpdateStatus(transaction.Id, PaymentTransaction.TransactionStatus.AUTHORIZED);

                //  Postavi GlobalTransactionId
                var globalTransactionId = GenerateGlobalTransactionId();
                _transactionRepo.SetGlobalTransactionId(transaction.Id, globalTransactionId);


                var duration = DateTime.UtcNow - startTime;

                _logger.LogInformation("[BANK-PAYMENT] AUTHORIZED | PaymentId: {PaymentId} | GlobalTxId: {GlobalTxId} | TxId: {TxId} | MerchantId: {MerchantId} | Amount: {Amount} {Currency} | CardHash: {CardHash} | TokenId: {TokenId} | DurationMs: {DurationMs} | CorrId: {CorrId}",
                    cardInfo.PaymentId, globalTransactionId, transaction.Id, transaction.MerchantId, transaction.Amount, transaction.Currency, cardHash, cardToken.Id, duration.TotalMilliseconds, correlationId);

                // Extract PSP Payment ID from STAN (format: PSP-{id}-{timestamp})
                var stanParts = transaction.Stan.Split('-');
                var pspPaymentId = stanParts.Length > 1 ? stanParts[1] : "0";

                return new PaymentStatusResponse
                {
                    PaymentId = cardInfo.PaymentId,
                    Status = "AUTHORIZED",
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    GlobalTransactionId = globalTransactionId,
                    AcquirerTimestamp = DateTime.UtcNow.AddHours(-1),
                    AuthorizedAt = DateTime.UtcNow.AddHours(-1),
                    Message = "Payment authorized successfully",
                    RedirectUrl = _configuration["PSPFrontendUrl"] + $"/payment/{pspPaymentId}?status=success&bankPaymentId={transaction.PaymentId}"
                };
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "[BANK-PAYMENT] ERROR | PaymentId: {PaymentId} | CardHash: {CardHash} | ErrorType: {ErrorType} | DurationMs: {DurationMs} | CorrId: {CorrId}",
                    cardInfo?.PaymentId ?? "unknown", cardHash, ex.GetType().Name, duration.TotalMilliseconds, correlationId);

                // A�uriraj status na FAILED ako je do�lo do gre�ke
                try
                {
                    var transaction = _transactionRepo.GetByPaymentId(cardInfo.PaymentId);
                    if (transaction != null)
                    {
                        _transactionRepo.UpdateStatus(transaction.Id, PaymentTransaction.TransactionStatus.FAILED);
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "[BANK-PAYMENT] FAILED_STATUS_UPDATE | PaymentId: {PaymentId}", cardInfo?.PaymentId ?? "unknown");
                }

                throw;
            }
        }

        //Ovo ce trebati za PSP i WebShop da moze da vidi status
        public PaymentStatusResponse GetPaymentStatus(string paymentId)
        {
            var correlationId = GetCorrelationId();
            var startTime = DateTime.UtcNow;
            try
            {
                var transaction = _transactionRepo.GetByPaymentId(paymentId);
                if (transaction == null)
                    throw new ArgumentException("Payment not found");
                _logger.LogDebug("[BANK-STATUS] Checked | PaymentId: {PaymentId} | Status: {Status} | CorrId: {CorrId}",
    paymentId, transaction.Status, correlationId);
                return new PaymentStatusResponse
                {
                    PaymentId = paymentId,
                    Status = transaction.Status.ToString(),
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    GlobalTransactionId = transaction.GlobalTransactionId,
                    AuthorizedAt = transaction.AuthorizedAt,
                    Message = GetStatusMessage(transaction.Status)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BANK-STATUS] ERROR | PaymentId: {PaymentId} | CorrId: {CorrId}",
    paymentId, correlationId);
                throw;
            }
        }

        private string GetStatusMessage(PaymentTransaction.TransactionStatus status)
        {
            return status switch
            {
                PaymentTransaction.TransactionStatus.PENDING => "Payment is pending",
                PaymentTransaction.TransactionStatus.AUTHORIZED => "Payment authorized, funds reserved",
                PaymentTransaction.TransactionStatus.CAPTURED => "Payment completed successfully",
                PaymentTransaction.TransactionStatus.FAILED => "Payment failed",
                PaymentTransaction.TransactionStatus.EXPIRED => "Payment expired",
                PaymentTransaction.TransactionStatus.CANCELLED => "Payment cancelled",
                _ => "Unknown status"
            };
        }

        //Ako korisnik odustane ili se nesto desi da se sredstva vrate nazad
        public bool CancelPayment(string paymentId)
        {
            try
            {
                var transaction = _transactionRepo.GetByPaymentId(paymentId);

                if (transaction == null ||
                    (transaction.Status != PaymentTransaction.TransactionStatus.PENDING &&
                     transaction.Status != PaymentTransaction.TransactionStatus.AUTHORIZED))
                    return false;

                // AKO JE AUTHORIZED - OSLOBODI REZERVISANA SREDSTVA
                if (transaction.Status == PaymentTransaction.TransactionStatus.AUTHORIZED &&
                    transaction.CustomerAccountId.HasValue)
                {
                    _accountRepo.ReleaseReservedFunds(
                        transaction.CustomerAccountId.Value,
                        transaction.Amount,
                        transaction.Currency);

                }

                _transactionRepo.UpdateStatus(transaction.Id, PaymentTransaction.TransactionStatus.CANCELLED);

                _logger.LogInformation($"Payment cancelled: {paymentId}, Previous status: {transaction.Status}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling payment: {paymentId}");
                return false;
            }
        }

        private bool ValidatePspRequest(PaymentRequest request)
        {
            // Ovde treba implementacija HMAC ili sertifikat validacije
            return !string.IsNullOrEmpty(request.MerchantId) &&
                   request.Amount > 0 &&
                   !string.IsNullOrEmpty(request.Currency) &&
                   !string.IsNullOrEmpty(request.Stan) &&
                   request.PspTimestamp > DateTime.UtcNow.AddMinutes(-5) &&
                   request.PspTimestamp <= DateTime.UtcNow;
        }

        private string GenerateSecurePaymentId()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[16];
            rng.GetBytes(bytes);
            return $"pay_" + Convert.ToBase64String(bytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .Substring(0, 16);
        }
        //ovo promeniti kad bude trebalo
        private string GeneratePaymentUrl(string paymentId)
        {
            var baseUrl = _configuration["PaymentSettings:BaseUrl"] ?? "https://localhost:5172";
            return $"{baseUrl}/payment/{paymentId}";
        }

        private bool ValidateCardInformation(CardInformation cardInfo, PaymentTransaction transaction)
        {

            // Provera CVV formata
            if (string.IsNullOrEmpty(cardInfo.Cvv) || cardInfo.Cvv.Length < 3 || cardInfo.Cvv.Length > 4)
                return false;
            if (!ValidateCardWithCvv(cardInfo.CardNumber, cardInfo.Cvv))
                return false;

            // Provera cardholder name
            if (string.IsNullOrEmpty(cardInfo.CardholderName) || cardInfo.CardholderName.Length < 2)
                return false;

            return true;
        }

            public bool ValidateCardWithCvv(string cardNumber, string cvv)
            {

                // 2. U realnom sistemu - poziv ka Visa/Mastercard mre�i
                //    ili internom sistemu banke
                //var response = _cardNetworkApi.ValidateCvv(cardNumber, cvv);

                // 3. Za demo - test kartice
                return IsTestCardCvvValid(cardNumber, cvv);
            }

            private bool IsTestCardCvvValid(string cardNumber, string cvv)
            {
                var cleanNumber = cardNumber.Replace(" ", "");

                // Test Visa/Mastercard: CVV = 123
                if ((cleanNumber.StartsWith("4") || cleanNumber.StartsWith("5"))
                    && cvv == "123")
                    return true;

                // Test Amex: CVV = 1234
                if (cleanNumber.StartsWith("34") || cleanNumber.StartsWith("37"))
                    return cvv == "1234";

                return false;
            }
        

        private string GenerateGlobalTransactionId()
        {
            // Format koji koriste mnoge banke: prefiks + timestamp + unique ID
            var bankPrefix = _configuration["BankSettings:Prefix"] ?? "ACQ";
            var timestamp = DateTime.UtcNow.AddHours(-1).ToString("yyyyMMdd");
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();

            return $"{bankPrefix}{timestamp}{uniqueId}";
        }

        private bool ValidateCardholderName(string enteredName, string storedName)
        {
            if (string.IsNullOrWhiteSpace(enteredName) || string.IsNullOrWhiteSpace(storedName))
                return false;

            // Ukloni vi�ak razmaka i konvertuj u uppercase 
            var entered = enteredName.Trim().ToUpperInvariant();
            var stored = storedName.Trim().ToUpperInvariant();

            return entered == stored;
        }

        private bool ValidateExpiryDate(string enteredMonth, string enteredYear, string storedMonth, string storedYear)
        {
            try
            {
                enteredMonth = enteredMonth.Trim().PadLeft(2, '0');
                enteredYear = enteredYear.Trim();

                // Proveri format (MM i YY kao stringovi)
                if (enteredMonth.Length != 2 || enteredYear.Length != 2)
                    return false;

                if (storedMonth.Length != 2 || storedYear.Length != 2)
                    return false;

                // Proveri da li su brojevi
                if (!int.TryParse(enteredMonth, out int enteredMonthInt) ||
                    !int.TryParse(enteredYear, out int enteredYearInt) ||
                    !int.TryParse(storedMonth, out int storedMonthInt) ||
                    !int.TryParse(storedYear, out int storedYearInt))
                    return false;

                if (enteredMonthInt < 1 || enteredMonthInt > 12)
                    return false;



                return true;
            }
            catch
            {
                return false;
            }
        }

        // ==================== QR CODE PAYMENT METHODS ====================

        public async Task<QrCodeGenerationResult> GenerateQrCodeForPaymentAsync(string paymentId, int size = 300)
        {
            var correlationId = GetCorrelationId();
            var startTime = DateTime.UtcNow;
            try
            {
                var transaction = _transactionRepo.GetByPaymentId(paymentId);
                if (transaction == null)
                {
                    _logger.LogWarning("[BANK-QR] FAILED | Event: TX_NOT_FOUND | PaymentId: {PaymentId} | CorrId: {CorrId} | IP: {IP}",
                paymentId, correlationId, GetClientIp());
                    throw new ArgumentException("Invalid payment ID");
                }
                    

                if (!_transactionRepo.IsTransactionValid(transaction.Id))
                {
                    _logger.LogWarning("[BANK-QR] FAILED | Event: TX_EXPIRED | PaymentId: {PaymentId} | TxId: {TxId} | MerchantId: {MerchantId} | CorrId: {CorrId}",
                        paymentId, transaction.Id, transaction.MerchantId, correlationId);
                    throw new InvalidOperationException("Transaction has expired");
                }

                var merchantAccount = _accountRepo.GetMerchantByMerchantId(transaction.MerchantId);
                if (merchantAccount == null)
                    if (merchantAccount == null)
                    {
                        _logger.LogWarning("[BANK-QR] FAILED | Event: MERCHANT_NOT_FOUND | PaymentId: {PaymentId} | MerchantId: {MerchantId} | CorrId: {CorrId}",
                            paymentId, transaction.MerchantId, correlationId);
                        throw new ArgumentException("Merchant not found");
                    }

                var qrPayload = new QrCodePayload
                {
                    PaymentId = paymentId,
                    ReceiverAccountNumber = merchantAccount.AccountNumber,
                    ReceiverName = merchantAccount.Customer?.FullName ?? "Merchant",
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    PaymentPurpose = $"Payment #{transaction.Stan}"
                };

                var result = await _qrCodeService.GenerateQrCodeAsync(qrPayload, size);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("[BANK-QR] GENERATED | PaymentId: {PaymentId} | TxId: {TxId} | MerchantId: {MerchantId} | Amount: {Amount} {Currency} | Size: {Size} | Success: {Success} | DurationMs: {DurationMs} | CorrId: {CorrId}",
                    paymentId, transaction.Id, transaction.MerchantId, transaction.Amount, transaction.Currency, size, result.Success, duration.TotalMilliseconds, correlationId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BANK-QR] ERROR | PaymentId: {PaymentId} | ErrorType: {ErrorType} | CorrId: {CorrId}",
            paymentId, ex.GetType().Name, correlationId);
                return new QrCodeGenerationResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<PaymentStatusResponse> ConfirmQrPayment(string paymentId)
        {
            var correlationId = GetCorrelationId();
            var startTime = DateTime.UtcNow;
            try
            {
                var transaction = _transactionRepo.GetByPaymentId(paymentId);
                if (transaction == null)
                    if (transaction == null)
                    {
                        _logger.LogWarning("[BANK-QR-CONFIRM] FAILED | Event: TX_NOT_FOUND | PaymentId: {PaymentId} | CorrId: {CorrId} | IP: {IP}",
                            paymentId, correlationId, GetClientIp());
                        throw new ArgumentException("Invalid payment ID");
                    }

                // Pronađi merchant račun (webshop)
                var merchantAccount = _accountRepo.GetMerchantByMerchantId(transaction.MerchantId);
                if (merchantAccount == null)
                {
                    _logger.LogWarning("[BANK-QR-CONFIRM] FAILED | Event: MERCHANT_NOT_FOUND | PaymentId: {PaymentId} | MerchantId: {MerchantId} | CorrId: {CorrId}",
                        paymentId, transaction.MerchantId, correlationId);
                    throw new InvalidOperationException("Merchant account not found");
                }

                // Dinamički pronađi kupčev račun koji ima dovoljno sredstava
                // U realnom scenariju, ovo bi bio authenticated user iz mBanking aplikacije
                // Za demo - koristimo CUST_BUYER001 kao simuliranog kupca
                var customerAccount = _accountRepo.FindCustomerAccount("CUST_BUYER001");
                
                if (customerAccount == null)
                {
                    _logger.LogWarning("[BANK-QR-CONFIRM] FAILED | Event: CUSTOMER_NOT_FOUND | PaymentId: {PaymentId} | CorrId: {CorrId}",
                        paymentId, correlationId);
                    throw new InvalidOperationException("Customer account not found");
                }

                // Provera sredstava - da li kupac može da plati
                if (customerAccount.Balance < transaction.Amount)
                {
                    _logger.LogWarning("[BANK-QR-CONFIRM] FAILED | Event: INSUFFICIENT_FUNDS | PaymentId: {PaymentId} | TxId: {TxId} | CustomerId: {CustomerId} | Required: {Required} {Currency} | Available: {Available} | CorrId: {CorrId}",
                        paymentId, transaction.Id, customerAccount.CustomerId, transaction.Amount, transaction.Currency, customerAccount.Balance, correlationId);
                    throw new InvalidOperationException($"Insufficient funds. Required: {transaction.Amount} {transaction.Currency}, Available: {customerAccount.Balance} {customerAccount.Currency}");
                }

                // Provera valute - moraju biti iste
                if (customerAccount.Currency != transaction.Currency)
                {
                    _logger.LogWarning("[BANK-QR-CONFIRM] FAILED | Event: CURRENCY_MISMATCH | PaymentId: {PaymentId} | TxId: {TxId} | TxCurrency: {TxCurrency} | AccountCurrency: {AccountCurrency} | CorrId: {CorrId}",
                        paymentId, transaction.Id, transaction.Currency, customerAccount.Currency, correlationId);
                    throw new InvalidOperationException($"Currency mismatch. Transaction requires {transaction.Currency}, but account is in {customerAccount.Currency}");
                }

                _logger.LogInformation("[BANK-QR-CONFIRM] PROCESSING | PaymentId: {PaymentId} | TxId: {TxId} | MerchantId: {MerchantId} | CustomerId: {CustomerId} | Amount: {Amount} {Currency} | CorrId: {CorrId}",
            paymentId, transaction.Id, transaction.MerchantId, customerAccount.CustomerId, transaction.Amount, transaction.Currency, correlationId);

                // Prebaci pare sa kupčevog računa na webshop račun
                customerAccount.Balance -= transaction.Amount;
                customerAccount.AvailableBalance -= transaction.Amount; // Skini i sa AvailableBalance za QR placanje
                merchantAccount.Balance += transaction.Amount;
                merchantAccount.AvailableBalance += transaction.Amount; //DODATO NAKNADNO
                _accountRepo.UpdateAccount(customerAccount);
                _accountRepo.UpdateAccount(merchantAccount);

                // Ažuriraj transakciju
                transaction.Status = PaymentTransaction.TransactionStatus.AUTHORIZED;
                transaction.GlobalTransactionId = Guid.NewGuid().ToString();
                transaction.CustomerAccountId = customerAccount.Id;
                transaction.CustomerId = customerAccount.CustomerId;
                _transactionRepo.UpdateTransaction(transaction);

                var duration = DateTime.UtcNow - startTime;

                _logger.LogInformation("[BANK-QR-CONFIRM] AUTHORIZED | PaymentId: {PaymentId} | GlobalTxId: {GlobalTxId} | TxId: {TxId} | MerchantId: {MerchantId} | CustomerId: {CustomerId} | Amount: {Amount} {Currency} | DurationMs: {DurationMs} | CorrId: {CorrId}",
                    paymentId, transaction.GlobalTransactionId, transaction.Id, transaction.MerchantId, customerAccount.CustomerId, transaction.Amount, transaction.Currency, duration.TotalMilliseconds, correlationId);

                // Extract PSP Payment ID from STAN (format: PSP-{id}-{timestamp})
                var stanParts = transaction.Stan.Split('-');
                var pspPaymentId = stanParts.Length > 1 ? stanParts[1] : "0";

                return new PaymentStatusResponse
                {
                    PaymentId = paymentId,
                    Status = PaymentTransaction.TransactionStatus.AUTHORIZED.ToString(),
                    Success = true,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    GlobalTransactionId = transaction.GlobalTransactionId,
                    Message = "Payment confirmed successfully",
                    RedirectUrl = _configuration["PSPFrontendUrl"] + $"/payment/{pspPaymentId}?status=success&bankPaymentId={transaction.PaymentId}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BANK-QR-CONFIRM] ERROR | PaymentId: {PaymentId} | ErrorType: {ErrorType} | CorrId: {CorrId}",
            paymentId, ex.GetType().Name, correlationId);
                return new PaymentStatusResponse
                {
                    PaymentId = paymentId,
                    Status = PaymentTransaction.TransactionStatus.FAILED.ToString(),
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public PaymentStatusResponse GetPaymentStatusResponse(string paymentId)
        {
            var correlationId = GetCorrelationId();
            var startTime = DateTime.UtcNow;
            try
            {
                var transaction = _transactionRepo.GetByPaymentId(paymentId);
                _logger.LogDebug("[BANK-STATUS] Checked | PaymentId: {PaymentId} | Status: {Status} | CorrId: {CorrId}",
    paymentId, transaction.Status, correlationId);
                if (transaction == null)
                {

                    return new PaymentStatusResponse
                    {
                        PaymentId = paymentId,
                        Status = "NOT_FOUND",
                        Success = false,
                        ErrorMessage = "Payment not found"
                    };
                }

                return new PaymentStatusResponse
                {
                    PaymentId = paymentId,
                    Status = transaction.Status.ToString(),
                    Success = transaction.Status == PaymentTransaction.TransactionStatus.AUTHORIZED || transaction.Status == PaymentTransaction.TransactionStatus.CAPTURED,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    GlobalTransactionId = transaction.GlobalTransactionId,
                    Message = transaction.Status == PaymentTransaction.TransactionStatus.PENDING ? "Waiting for payment" : "Payment processed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BANK-STATUS] ERROR | PaymentId: {PaymentId} | CorrId: {CorrId}",
    paymentId, correlationId);
                return new PaymentStatusResponse
                {
                    PaymentId = paymentId,
                    Status = "ERROR",
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
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

        private string GetClientIp()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return "background-job";

            // Proveri X-Forwarded-For (ako je iza reverse proxy-ja)
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // Uzmi prvu IP (klijentovu, ne proxy chain)
                return forwardedFor.Split(',')[0].Trim();
            }

            // X-Real-IP (alternativa)
            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
                return realIp;

            // Direktna konekcija
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }



    }
}


