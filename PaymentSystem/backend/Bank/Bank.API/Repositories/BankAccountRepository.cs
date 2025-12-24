using Bank.API.Data;
using Bank.API.Models;

namespace Bank.API.Repositories
{
    public class BankAccountRepository
    {
        private readonly AppDbContext _context;

        public BankAccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public BankAccount GetByMerchantId(string merchantId)
        {
            return _context.BankAccounts
                .FirstOrDefault(a => a.MerchantId == merchantId);
        }

        public BankAccount GetById(long id)
        {
            return _context.BankAccounts.Find(id);
        }

        public BankAccount Create(BankAccount account)
        {
            _context.BankAccounts.Add(account);
            _context.SaveChanges();
            return account;
        }

        public void Update(BankAccount account)
        {
            _context.BankAccounts.Update(account);
            _context.SaveChanges();
        }

        public bool MerchantIdExists(string merchantId)
        {
            return _context.BankAccounts.Any(a => a.MerchantId == merchantId);
        }
    
}
}
