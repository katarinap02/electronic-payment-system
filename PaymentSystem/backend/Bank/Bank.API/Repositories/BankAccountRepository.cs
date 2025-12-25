using Bank.API.Data;
using Bank.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Bank.API.Repositories
{
    public class BankAccountRepository
    {
        private readonly AppDbContext _context;

        public BankAccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public BankAccount? GetByAccountNumber(string accountNumber)
        {
            return _context.BankAccounts
                .Include(a => a.Customer)
                .FirstOrDefault(a => a.AccountNumber == accountNumber);
        }
        public bool ReserveFunds(long accountId, decimal amount)
        {
            using var transaction = _context.Database.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var account = _context.BankAccounts
                    .FirstOrDefault(a => a.Id == accountId);

                if (account == null || account.AvailableBalance < amount)
                    return false;

                account.AvailableBalance -= amount;
                account.ReservedBalance += amount;

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

        public bool CanReserveFunds(long accountId, decimal amount)
        {
            var account = _context.BankAccounts
                .AsNoTracking()
                .FirstOrDefault(a => a.Id == accountId);

            return account != null && account.AvailableBalance >= amount;
        }

        public bool CaptureReservedFunds(long accountId, decimal amount)
        {
            using var transaction = _context.Database.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var account = _context.BankAccounts
                    .FirstOrDefault(a => a.Id == accountId);

                if (account == null || account.ReservedBalance < amount)
                    return false;

                account.ReservedBalance -= amount;
                account.PendingCaptureBalance += amount;

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

        public bool FinalizeCapture(long merchantAccountId, long customerAccountId, decimal amount)
        {
            using var transaction = _context.Database.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var merchantAccount = _context.BankAccounts
                    .FirstOrDefault(a => a.Id == merchantAccountId && a.IsMerchantAccount);

                var customerAccount = _context.BankAccounts
                    .FirstOrDefault(a => a.Id == customerAccountId);

                if (merchantAccount == null || customerAccount == null ||
                    customerAccount.PendingCaptureBalance < amount)
                    return false;

                customerAccount.PendingCaptureBalance -= amount;
                merchantAccount.Balance += amount;

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

        //ovo je za rollback
        public bool ReleaseReservedFunds(long accountId, decimal amount)
        {
            using var transaction = _context.Database.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var account = _context.BankAccounts
                    .FirstOrDefault(a => a.Id == accountId);

                if (account == null || account.ReservedBalance < amount)
                    return false;

                account.ReservedBalance -= amount;
                account.AvailableBalance += amount;

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

        public BankAccount? GetMerchantByMerchantId(string merchantId)
        {
            return _context.BankAccounts
                .Include(a => a.Customer)
                .FirstOrDefault(a => a.MerchantId == merchantId && a.IsMerchantAccount);
        }

        public BankAccount? FindCustomerAccount(string customerId)
        {
            return _context.BankAccounts
                .FirstOrDefault(a => a.CustomerId == customerId &&
                                   !a.IsMerchantAccount);
        }

    }
}
