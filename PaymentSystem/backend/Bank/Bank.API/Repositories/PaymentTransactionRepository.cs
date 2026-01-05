using Bank.API.Data;
using Bank.API.Models;
using Microsoft.EntityFrameworkCore;
using static Bank.API.Models.PaymentTransaction;

namespace Bank.API.Repositories
{
    public class PaymentTransactionRepository
    {
        private readonly AppDbContext _context;

        public PaymentTransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public PaymentTransaction CreateTransaction(
            string merchantId,
            decimal amount,
            string currency,
            string stan,
            DateTime merchantTimestamp,
            long merchantAccountId,
            string paymentId,
            string? customerId = null,
            string? successUrl = null,
            string? failedUrl = null,
            string? errorUrl = null
            )
        {
            var merchantAccount = _context.BankAccounts
                .FirstOrDefault(a => a.Id == merchantAccountId && a.IsMerchantAccount);

            if (merchantAccount == null)
                throw new ArgumentException("Merchant account not found");

            if (_context.PaymentTransactions.Any(t => t.Stan == stan))
                throw new ArgumentException("STAN must be unique");

            var transaction = new PaymentTransaction
            {
                MerchantId = merchantId,
                PaymentId = paymentId,
                Amount = amount,
                Currency = currency,
                Stan = stan,
                MerchantTimestamp = merchantTimestamp,
                MerchantAccountId = merchantAccountId,
                PspTimestamp = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                CustomerId = customerId,
                SuccessUrl = successUrl,
                FailedUrl = failedUrl,
                ErrorUrl = errorUrl
            };

            _context.PaymentTransactions.Add(transaction);
            _context.SaveChanges();

            return transaction;
        }

        public PaymentTransaction? GetByStan(string stan)
        {
            return _context.PaymentTransactions
                .Include(t => t.MerchantAccount)
                .Include(t => t.CustomerAccount)
                .Include(t => t.CardToken)
                .ThenInclude(ct => ct.Card)
                .FirstOrDefault(t => t.Stan == stan);
        }

        public PaymentTransaction? GetByPaymentId(string paymentId)
        {
            return _context.PaymentTransactions
                .Include(t => t.MerchantAccount)
                .Include(t => t.CustomerAccount)
                .FirstOrDefault(t => t.PaymentId == paymentId);
        }

        public bool UpdateStatus(long transactionId, PaymentTransaction.TransactionStatus status)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var paymentTransaction = _context.PaymentTransactions
                    .FirstOrDefault(t => t.Id == transactionId);

                if (paymentTransaction == null)
                    return false;

                var validTransitions = new Dictionary<PaymentTransaction.TransactionStatus, List<PaymentTransaction.TransactionStatus>>
                {
                    [PaymentTransaction.TransactionStatus.PENDING] = new() {
                        PaymentTransaction.TransactionStatus.AUTHORIZED,
                        PaymentTransaction.TransactionStatus.FAILED,
                        PaymentTransaction.TransactionStatus.EXPIRED,
                        PaymentTransaction.TransactionStatus.CANCELLED
                    },
                    [PaymentTransaction.TransactionStatus.AUTHORIZED] = new() {
                        PaymentTransaction.TransactionStatus.CAPTURED,
                        PaymentTransaction.TransactionStatus.FAILED,
                        PaymentTransaction.TransactionStatus.CANCELLED
                    },
                    [PaymentTransaction.TransactionStatus.CAPTURED] = new() { },
                    [PaymentTransaction.TransactionStatus.FAILED] = new() { },
                    [PaymentTransaction.TransactionStatus.EXPIRED] = new() { },
                    [PaymentTransaction.TransactionStatus.CANCELLED] = new() { }
                };

                if (!validTransitions.ContainsKey(paymentTransaction.Status) ||
                    !validTransitions[paymentTransaction.Status].Contains(status))
                {
                    return false;
                }

                paymentTransaction.Status = status;

                switch (status)
                {
                    case PaymentTransaction.TransactionStatus.AUTHORIZED:
                        paymentTransaction.AuthorizedAt = DateTime.UtcNow.AddHours(-1);
                        break;
                    case PaymentTransaction.TransactionStatus.CAPTURED:
                        paymentTransaction.CapturedAt = DateTime.UtcNow.AddHours(-1);
                        break;
                    case PaymentTransaction.TransactionStatus.FAILED:
                        paymentTransaction.FailedAt = DateTime.UtcNow.AddHours(-1);
                        break;
                }

                _context.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool SetGlobalTransactionId(long transactionId, string globalTransactionId)
        {
            var transaction = _context.PaymentTransactions.Find(transactionId);
            if (transaction == null)
                return false;

            transaction.GlobalTransactionId = globalTransactionId;
            transaction.AcquirerTimestamp = DateTime.UtcNow.AddHours(-1);

            _context.SaveChanges();
            return true;
        }
        public bool IsTransactionValid(long transactionId)
        {
            var transaction = _context.PaymentTransactions
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == transactionId);

            return transaction != null &&
                   transaction.ExpiresAt > DateTime.UtcNow.AddHours(-1) &&
                   transaction.Status == PaymentTransaction.TransactionStatus.PENDING;
        }

        public bool LinkCardAndAccountToTransaction(
        long transactionId,
        long cardId,
        long customerAccountId,
        string customerId)
            {
                var transaction = _context.PaymentTransactions.Find(transactionId);
                if (transaction == null)
                    return false;

                transaction.CardId = cardId;
                transaction.CustomerAccountId = customerAccountId;
                transaction.CustomerId = customerId;

                _context.SaveChanges();
                return true;
            }

        public int ExpireOldTransactions()
        {
            var expiredTransactions = _context.PaymentTransactions
                .Where(t => t.ExpiresAt < DateTime.UtcNow.AddHours(-1) &&
                           t.Status == PaymentTransaction.TransactionStatus.PENDING)
                .ToList();

            foreach (var transaction in expiredTransactions)
            {
                transaction.Status = PaymentTransaction.TransactionStatus.EXPIRED;
            }

            return _context.SaveChanges();
        }

        public bool HasDuplicateTransactionByStan(string stan)
        {
            return _context.PaymentTransactions
                .Any(t => t.Stan == stan &&
                         t.Status != PaymentTransaction.TransactionStatus.FAILED &&
                         t.Status != PaymentTransaction.TransactionStatus.EXPIRED &&
                         t.Status != PaymentTransaction.TransactionStatus.CANCELLED);
        }

        public bool UpdatePaymentId(long transactionId, string paymentId)
        {
            var transaction = _context.PaymentTransactions.Find(transactionId);
            if (transaction == null)
                return false;

            // Proveri da li je paymentId jedinstven
            if (_context.PaymentTransactions.Any(t => t.PaymentId == paymentId))
                return false;

            transaction.PaymentId = paymentId;
            _context.SaveChanges();
            return true;
        }

        // U PaymentTransactionRepository dodaj
        public IEnumerable<PaymentTransaction> GetAuthorizedTransactionsOlderThan(TimeSpan age)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-1) - age;
    
            return _context.PaymentTransactions
                .Where(t => t.Status == TransactionStatus.AUTHORIZED &&
                           t.AuthorizedAt < cutoffTime)
                .ToList();
        }
       
            }
        }
