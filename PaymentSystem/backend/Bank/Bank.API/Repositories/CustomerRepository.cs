using Bank.API.Data;
using Bank.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.API.Repositories
{
    public class CustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public Customer CreateCustomer(string fullName, string email, string phone, string? webShopUserId = null)
        {
            var customer = new Customer
            {
                FullName = fullName,
                EmailHash = HashData(email),
                PhoneHash = HashData(phone),
                WebShopUserIdHash = webShopUserId != null ? HashData(webShopUserId) : null
            };

            _context.Customers.Add(customer);
            _context.SaveChanges();

            return customer;
        }

        private string HashData(string data)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(data + GetPepper());
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string GetPepper()
        {
            return Environment.GetEnvironmentVariable("HASH_PEPPER") ?? "default-pepper-change-in-production";
        }

        public Customer? FindByEmailHash(string email)
        {
            var emailHash = HashData(email);
            return _context.Customers
                .Include(c => c.Accounts)
                .Include(c => c.Cards)
                .FirstOrDefault(c => c.EmailHash == emailHash);
        }

        public Customer? FindByWebShopUserId(string webShopUserId)
        {
            var hash = HashData(webShopUserId);
            return _context.Customers
                .FirstOrDefault(c => c.WebShopUserIdHash == hash);
        }

        public bool CustomerExists(string email)
        {
            var emailHash = HashData(email);
            return _context.Customers
                .Any(c => c.EmailHash == emailHash && c.Status == Customer.CustomerStatus.ACTIVE);
        }
        public bool UpdateCustomerStatus(string customerId, Customer.CustomerStatus status)
        {
            var customer = _context.Customers.Find(customerId);
            if (customer == null)
                return false;

            customer.Status = status;
            _context.SaveChanges();
            return true;
        }

        public bool AddAccountToCustomer(string customerId, BankAccount account)
        {
            var customer = _context.Customers
                .Include(c => c.Accounts)
                .FirstOrDefault(c => c.Id == customerId);

            if (customer == null)
                return false;

            if (customer.Accounts.Any(a => a.AccountNumber == account.AccountNumber))
                return false;

            customer.Accounts.Add(account);
            _context.SaveChanges();
            return true;
        }

        public bool AddCardToCustomer(string customerId, Card card)
        {
            var customer = _context.Customers
                .Include(c => c.Cards)
                .FirstOrDefault(c => c.Id == customerId);

            if (customer == null)
                return false;

            customer.Cards.Add(card);
            _context.SaveChanges();
            return true;
        }
    }
}
