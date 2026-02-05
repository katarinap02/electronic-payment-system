using Bank.API.Data;
using Bank.API.Models;
using Bank.API.Services;
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
        public bool ReserveFunds(long accountId, decimal amount, string currency)
        {
            using var transaction = _context.Database.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var account = _context.BankAccounts
                    .FirstOrDefault(a => a.Id == accountId);
                decimal amountInEur = amount;
                if (currency.ToUpper() == "USD")
                {
                    // Fiksni kurs
                    decimal usdToEurRate = 0.85m; // Možeš da izvučeš iz konfiguracije
                    amountInEur = amount * usdToEurRate;
                }
                // Dodaj i druge valute ako treba
                else if (currency.ToUpper() != "EUR")
                {
                    throw new InvalidOperationException($"Unsupported currency: {currency}");
                }

                if (account == null || account.AvailableBalance < amountInEur)
                    return false;

                account.AvailableBalance -= amountInEur;
                account.ReservedBalance += amountInEur;

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

        // U BankAccountRepository proveri FinalizeCapture
        public bool FinalizeCapture(long customerAccountId, long merchantAccountId,
            decimal amount, string currency = "EUR")
        {

            using var transaction = _context.Database.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var customer = _context.BankAccounts.Find(customerAccountId);
                var merchant = _context.BankAccounts.Find(merchantAccountId);

                if (customer == null || merchant == null)
                {
                  
                    return false;
                }

                decimal amountInEur = currency.ToUpper() == "USD" ? amount * 0.85m : amount;

                // Proveri da li kupac ima dovoljno rezervisanih sredstava
                if (customer.ReservedBalance < amountInEur)
                {
                   
                    return false;
                }

                // 👉 TRANSFER: Reserved → Merchant
                customer.ReservedBalance -= amountInEur;
                customer.Balance -= amountInEur;
                merchant.AvailableBalance += amountInEur;
                merchant.Balance += amountInEur;

                _context.SaveChanges();
                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }

        //ovo je za rollback
        public bool ReleaseReservedFunds(long accountId, decimal amount, string currency)
        {
            using var transaction = _context.Database.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var account = _context.BankAccounts
                    .FirstOrDefault(a => a.Id == accountId);

                if (account == null || account.ReservedBalance < amount)
                    return false;

                decimal amountInEur = amount;

                // Konvertuj USD u EUR ako je potrebno
                if (currency.ToUpper() == "USD")
                {
                    decimal usdToEurRate = 0.85m;
                    amountInEur = amount * usdToEurRate;
                }

                // Oslobodi rezervisana sredstva
                account.AvailableBalance += amountInEur;
                account.ReservedBalance -= amountInEur;

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

        public void UpdateAccount(BankAccount account)
        {
            _context.BankAccounts.Update(account);
            _context.SaveChanges();
        }
    }
}
