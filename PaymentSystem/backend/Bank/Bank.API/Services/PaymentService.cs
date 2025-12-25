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
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            PaymentTransactionRepository transactionRepo,
            BankAccountRepository accountRepo,
            CardRepository cardRepo,
            CardTokenRepository tokenRepo,
            IConfiguration configuration,
            ILogger<PaymentService> logger)
        {
            _transactionRepo = transactionRepo;
            _accountRepo = accountRepo;
            _cardRepo = cardRepo;
            _tokenRepo = tokenRepo;
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

                // Provera duple transakcije (specifikacija: "dvostruko plaćanje")
                if (_transactionRepo.HasDuplicateTransaction(request.MerchantId, "", request.Amount))
                {
                    _logger.LogWarning($"Duplicate transaction detected for Merchant: {request.MerchantId}");
                    throw new InvalidOperationException("Duplicate transaction detected");
                }

                // Kreira transakciju (STAN za praćenje transakcije)
                var transaction = _transactionRepo.CreateTransaction(
                    merchantId: request.MerchantId,
                    amount: request.Amount,
                    currency: request.Currency,
                    stan: request.Stan,
                    merchantTimestamp: request.PspTimestamp,
                    merchantAccountId: merchantAccount.Id);

                // Generiše PAYMENT_ID i PAYMENT_URL 
                var paymentId = GenerateSecurePaymentId();
                var paymentUrl = GeneratePaymentUrl(paymentId);

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
        /*
        //Karticno placanje
        public PaymentStatusResponse ProcessCardPayment(CardInformation cardInfo)
        {
            try
            {
                // 1. Validacija PAYMENT_ID i transakcije
                var transaction = _transactionRepo.GetByPaymentId(cardInfo.PaymentId);
                if (transaction == null)
                    throw new ArgumentException("Invalid payment ID");

                if (!_transactionRepo.IsTransactionValid(transaction.Id))
                    throw new InvalidOperationException("Transaction has expired");

                // Tokenizacija kartice (PCI DSS compliance)
                var cardToken = _tokenRepo.CreateToken(0, transaction.Id); // CardId će se dodati kasnije

                // Validacija kartice 
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

                // 6. Pronađi ili kreiraj customer account (pošto je sve u istoj banci)
                // U realnoj implementaciji, ovde bi bio lookup po PAN-u ili customer ID-u
                var customerAccount = FindOrCreateCustomerAccount(cardInfo, transaction);

                // Rezervacija sredstava 
                if (!_accountRepo.ReserveFunds(customerAccount.Id, transaction.Amount))
                    throw new InvalidOperationException("Insufficient funds");

                // Autorizacija transakcije
                _transactionRepo.UpdateStatus(transaction.Id, PaymentTransaction.TransactionStatus.AUTHORIZED);

                //  Postavi GlobalTransactionId
                var globalTransactionId = GenerateGlobalTransactionId();
                _transactionRepo.SetGlobalTransactionId(transaction.Id, globalTransactionId);

                // 10. Link kartice sa transakcijom
                 _transactionRepo.LinkCardToTransaction(transaction.Id, cardId);

                _logger.LogInformation($"Card payment authorized: {globalTransactionId} for Amount: {transaction.Amount}");

                return new PaymentStatusResponse
                {
                    PaymentId = cardInfo.PaymentId,
                    Status = "AUTHORIZED",
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    GlobalTransactionId = globalTransactionId,
                    AuthorizedAt = DateTime.UtcNow,
                    Message = "Payment authorized successfully"
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
        }*/

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
            var baseUrl = _configuration["PaymentSettings:BaseUrl"] ?? "https://bank.example.com";
            return $"{baseUrl}/payment/{paymentId}";
        }

        private bool ValidateCardInformation(CardInformation cardInfo, PaymentTransaction transaction)
        {
            // Provera da se iznos nije promenio
            // Ova provera bi trebala da bude i na frontendu

            // Provera CVV formata
            if (string.IsNullOrEmpty(cardInfo.Cvv) || cardInfo.Cvv.Length < 3 || cardInfo.Cvv.Length > 4)
                return false;

            // Provera cardholder name
            if (string.IsNullOrEmpty(cardInfo.CardholderName) || cardInfo.CardholderName.Length < 2)
                return false;

            return true;
        }

        /*
        public PaymentResponse InitiatePayment(PaymentRequest request)
        {
            _logger.LogInformation($"InitiatePayment: {request.MerchantId}, {request.Amount} {request.Currency}");

            //Validacija merchant-a
            var merchantAccount = _accountRepo.GetByMerchantId(request.MerchantId);
            if (merchantAccount == null)
                throw new Exception($"Merchant not found: {request.MerchantId}");

            //Proverava duplikat STAN-a
            if (_transactionRepo.StanExists(request.Stan))
                throw new Exception($"Duplicate STAN: {request.Stan}");

            //Generiše payment_id
            var paymentId = Guid.NewGuid().ToString();

            //Kreira transakciju
            var transaction = new PaymentTransaction
            {
                PaymentId = paymentId,
                MerchantId = request.MerchantId,
                MerchantTimestamp = DateTime.UtcNow,
                Stan = request.Stan,
                Amount = request.Amount,
                Currency = request.Currency,
                PspTimestamp = request.PspTimestamp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10), //ako korisnik izadje i zaboravi na placanje
                Status = PaymentTransaction.TransactionStatus.PENDING,
                AcquirerTimestamp = DateTime.UtcNow,
                MerchantAccountId = merchantAccount.Id
            };

            _transactionRepo.Create(transaction);

            //Generiše payment_url
            var paymentUrl = $"http://localhost:5174/pay/{paymentId}";

            return new PaymentResponse
            {
                PaymentUrl = paymentUrl,
                PaymentId = paymentId,
                ExpiresAt = transaction.ExpiresAt,
                Message = "Payment URL generated successfully"
            };
        }

        public PaymentStatusResponse AuthorizeCardPayment(CardInformation request)
        {
            //Pronađe transakciju
            var transaction = _transactionRepo.GetByPaymentId(request.PaymentId);
            if (transaction == null)
                throw new Exception("Transaction not found");

            //Proveri status i expiry
            if (transaction.Status != PaymentTransaction.TransactionStatus.PENDING)
                throw new Exception($"Transaction is already {transaction.Status}");

            if (transaction.ExpiresAt < DateTime.UtcNow)
            {
                transaction.Status = PaymentTransaction.TransactionStatus.EXPIRED;
                _transactionRepo.Update(transaction);
                throw new Exception("Payment expired");
            }

            // Lunov algoritam
            if (!_cardService.ValidateByLuhn(request.CardNumber))
                throw new Exception("Invalid card number");

            if (!_cardService.ValidateExpiryDate(request.ExpiryDate))
                throw new Exception("Card expired or invalid expiry date");

            // Tokenizacija (PCI DSS)
            var cardToken = new CardToken
            {
                Token = _cardService.TokenizeCard(request.CardNumber),
                CardHash = BCrypt.Net.BCrypt.HashPassword(request.CardNumber),
                MaskedPan = "**** **** **** " + request.CardNumber.Substring(request.CardNumber.Length - 4),
                CardholderName = request.CardholderName,
                ExpiryMonth = request.ExpiryDate.Split('/')[0],
                ExpiryYear = request.ExpiryDate.Split('/')[1],
                CreatedAt = DateTime.UtcNow,
                TransactionId = transaction.Id
            };

            _cardTokenRepo.Create(cardToken);

            // Simulacija provere stanja (mock)
            var merchantAccount = _accountRepo.GetById(transaction.MerchantAccountId);
            // U stvarnom sistemu ovde bi proverio stanje kupca, ovde simulacija

            //Ažurirati transakciju
            transaction.Status = PaymentTransaction.TransactionStatus.AUTHORIZED;
            transaction.AuthorizedAt = DateTime.UtcNow;
            transaction.GlobalTransactionId = "BANK_" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
            transaction.CardTokenId = cardToken.Id;
            _transactionRepo.Update(transaction);

            //Simulacija rezervacije sredstava
            //merchantAccount.Balance += transaction.Amount;
            //customerAccount.Balance -= transaction.Amount;

            _logger.LogInformation($"Payment authorized: {transaction.PaymentId}, {transaction.Amount} {transaction.Currency}");

            return new PaymentStatusResponse
            {
                PaymentId = transaction.PaymentId,
                Status = transaction.Status.ToString(),
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                GlobalTransactionId = transaction.GlobalTransactionId,
                AuthorizedAt = transaction.AuthorizedAt,
                Message = "Payment authorized successfully"
            };
        }

        public PaymentFormResponse GetPaymentForm(string paymentId)
        {
            var transaction = _transactionRepo.GetByPaymentId(paymentId);

            if (transaction == null)
                throw new Exception("Payment not found");

            if (transaction.Status != PaymentTransaction.TransactionStatus.PENDING)
                throw new Exception($"Payment is already {transaction.Status}");

            if (transaction.ExpiresAt < DateTime.UtcNow)
            {
                transaction.Status = PaymentTransaction.TransactionStatus.EXPIRED;
                _transactionRepo.Update(transaction);
                throw new Exception("Payment expired");
            }

            return new PaymentFormResponse
            {
                PaymentId = transaction.PaymentId,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                MerchantName = "WebShop Example",
                ExpiresAt = transaction.ExpiresAt,
                IsQrPayment = false
            }; 
        }*/
    }
    }
