using Bank.API.DTOs;
using Bank.API.Models;
using Bank.API.Repositories;

namespace Bank.API.Services
{
    public class PaymentService
    {
        private readonly BankAccountRepository _accountRepo;
        private readonly PaymentTransactionRepository _transactionRepo;
        private readonly CardTokenRepository _cardTokenRepo;
        private readonly ILogger<PaymentService> _logger;
        private readonly CardService _cardService;

        public PaymentService(BankAccountRepository accountRepo, PaymentTransactionRepository transactionRepo, CardTokenRepository cardTokenRepo, ILogger<PaymentService> logger, CardService cardService)
        {
            _accountRepo = accountRepo;
            _transactionRepo = transactionRepo;
            _cardTokenRepo = cardTokenRepo;
            _logger = logger;
            _cardService = cardService;
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
