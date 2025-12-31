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
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            PaymentTransactionRepository transactionRepo,
            BankAccountRepository accountRepo,
            CardRepository cardRepo,
            CardTokenRepository tokenRepo,
            IConfiguration configuration,
            ILogger<PaymentService> logger, 
            CardService cardService)
        {
            _transactionRepo = transactionRepo;
            _accountRepo = accountRepo;
            _cardRepo = cardRepo;
            _tokenRepo = tokenRepo;
            _cardService = cardService;
            _configuration = configuration;
            _logger = logger;
        }

          //  Kreiranje PAYMENT_URL i PAYMENT_ID za PSP (specifikacija tačka 2)
        public PaymentResponse CreatePaymentUrl(PaymentRequest request)
        {
            try
            {
                // Validacija PSP autentifikacije (HMAC ili sertifikat)
                if (!ValidatePspRequest(request))
                {
                    _logger.LogWarning($"Invalid PSP request from Merchant: {request.MerchantId}");
                    throw new UnauthorizedAccessException("Invalid PSP credentials");
                }

                //  validacija MERCHANT_ID
                var merchantAccount = _accountRepo.GetMerchantByMerchantId(request.MerchantId);
                if (merchantAccount == null)
                {
                    _logger.LogWarning($"Merchant not found: {request.MerchantId}");
                    throw new ArgumentException("Merchant not found or inactive");
                }

                // 2. Provera duple transakcije po STAN-u (jedinstven između PSP-a i banke)
                if (_transactionRepo.HasDuplicateTransactionByStan(request.Stan))
                {
                    _logger.LogWarning($"Duplicate STAN detected: {request.Stan}");

                    // Pronađi postojeću transakciju sa ovim STAN-om
                    var existingTransaction = _transactionRepo.GetByStan(request.Stan);
                    if (existingTransaction != null && !string.IsNullOrEmpty(existingTransaction.PaymentId))
                    {
                        // Vrati postojeću ako je duplikat (idempotentnost)
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

                // Generiše PAYMENT_ID i PAYMENT_URL 
                var paymentId = GenerateSecurePaymentId();
                var paymentUrl = GeneratePaymentUrl(paymentId);

                // Kreira transakciju (STAN za praćenje transakcije)
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
                    errorUrl: request.ErrorUrl);

                

                // 6. Sačuvaj PAYMENT_ID u transakciju
                transaction.PaymentId = paymentId;
                _transactionRepo.UpdatePaymentId(transaction.Id, paymentId);

                _logger.LogInformation($"Payment URL created: {paymentId} for Merchant: {request.MerchantId}");

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
                _logger.LogError(ex, $"Error creating payment URL for Merchant: {request.MerchantId}");
                throw;
            }
        }

        //dobijanje forme za placanje
        public PaymentFormResponse GetPaymentForm(string paymentId)
        {
            try
            {
                var transaction = _transactionRepo.GetByPaymentId(paymentId);
                if (transaction == null)
                    throw new ArgumentException("Invalid payment ID");

                // Proveri da li je transakcija još uvek validna 
                if (!_transactionRepo.IsTransactionValid(transaction.Id))
                {
                    _logger.LogWarning($"Expired or invalid payment form requested: {paymentId}");
                    throw new InvalidOperationException("Payment form has expired");
                }

                var merchantAccount = _accountRepo.GetMerchantByMerchantId(transaction.MerchantId);
                if (merchantAccount == null)
                    throw new ArgumentException("Merchant not found");

                return new PaymentFormResponse
                {
                    PaymentId = paymentId,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    MerchantName = merchantAccount.Customer?.FullName ?? "Unknown Merchant",
                    ExpiresAt = transaction.ExpiresAt,
                    IsQrPayment = false // Za sada samo kartica
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payment form for PaymentId: {paymentId}");
                throw;
            }
        }
        
        //Karticno placanje
        public PaymentStatusResponse ProcessCardPayment(CardInformation cardInfo)
        {
            try
            {
                // Validacija PAYMENT_ID i transakcije
                var transaction = _transactionRepo.GetByPaymentId(cardInfo.PaymentId);
                if (transaction == null)
                    throw new ArgumentException("Invalid payment ID");

                // Proverava da li je expired i da li je status Pending
               // if (!_transactionRepo.IsTransactionValid(transaction.Id))
               //     throw new InvalidOperationException("Transaction has expired");

                // Validacija kartice, dodati posle proveru da se iznost nije promenio
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

                var cardHash = _cardService.GenerateCardHash(cardInfo.CardNumber);
                 var card = _cardRepo.FindCardByHash(cardHash); 
               

                if (card == null)
                    throw new InvalidOperationException("Card not found");

                cardInfo.Cvv = null;

                //Tokenizacija kartice (PCI DSS compliance)
                var cardToken = _tokenRepo.CreateToken(card.Id, transaction.Id);

                // 6. Pronađi customer account (pošto je sve u istoj banci)
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
                if (!_accountRepo.ReserveFunds(customerAccount.Id, transaction.Amount))
                    throw new InvalidOperationException("Insufficient funds");

                // Autorizacija transakcije
                _transactionRepo.UpdateStatus(transaction.Id, PaymentTransaction.TransactionStatus.AUTHORIZED);

                //  Postavi GlobalTransactionId
                var globalTransactionId = GenerateGlobalTransactionId();
                _transactionRepo.SetGlobalTransactionId(transaction.Id, globalTransactionId);


                _logger.LogInformation($"Card payment authorized: {globalTransactionId} for Amount: {transaction.Amount}");

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
                    RedirectUrl = transaction.SuccessUrl + $"?paymentId={transaction.PaymentId}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing card payment for PaymentId: {cardInfo.PaymentId}");

                // Ažuriraj status na FAILED ako je došlo do greške
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
                    _logger.LogError(innerEx, "Error updating transaction status to FAILED");
                }

                throw;
            }
        }

        //Ovo ce trebati za PSP i WebShop da moze da vidi status
        public PaymentStatusResponse GetPaymentStatus(string paymentId)
        {
            try
            {
                var transaction = _transactionRepo.GetByPaymentId(paymentId);
                if (transaction == null)
                    throw new ArgumentException("Payment not found");

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
                _logger.LogError(ex, $"Error getting payment status for PaymentId: {paymentId}");
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
                if (transaction == null || transaction.Status != PaymentTransaction.TransactionStatus.PENDING)
                    return false;

                // Ako su sredstva već rezervisana, oslobodi ih
                if (transaction.Status == PaymentTransaction.TransactionStatus.AUTHORIZED &&
                    transaction.CustomerAccountId.HasValue)
                {
                    _accountRepo.ReleaseReservedFunds(transaction.CustomerAccountId.Value, transaction.Amount);
                }

                // Ažuriraj status na CANCELLED
                _transactionRepo.UpdateStatus(transaction.Id, PaymentTransaction.TransactionStatus.CANCELLED);

                _logger.LogInformation($"Payment cancelled: {paymentId}");

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
            var baseUrl = _configuration["PaymentSettings:BaseUrl"] ?? "http://localhost:5172";
            return $"{baseUrl}/payment/{paymentId}";
        }

        private bool ValidateCardInformation(CardInformation cardInfo, PaymentTransaction transaction)
        {

            // Provera CVV formata
            if (string.IsNullOrEmpty(cardInfo.Cvv) || cardInfo.Cvv.Length < 3 || cardInfo.Cvv.Length > 4)
                return false;

            // Provera cardholder name
            if (string.IsNullOrEmpty(cardInfo.CardholderName) || cardInfo.CardholderName.Length < 2)
                return false;

            return true;
        }

        private string GenerateGlobalTransactionId()
        {
            // Format koji koriste mnoge banke: prefiks + timestamp + unique ID
            var bankPrefix = _configuration["BankSettings:Prefix"] ?? "ACQ";
            var timestamp = DateTime.UtcNow.AddHours(-1).ToString("yyyyMMdd");
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();

            return $"{bankPrefix}{timestamp}{uniqueId}";
        }

    }
}
