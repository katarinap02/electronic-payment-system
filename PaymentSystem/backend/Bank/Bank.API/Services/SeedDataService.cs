using Bank.API.Data;
using Bank.API.Models;
using System.Security.Cryptography;
using System.Text;

namespace Bank.API.Services
{
    public class SeedDataService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(AppDbContext context, ILogger<SeedDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void InitializeTestData()
        {
            try
            {
                // Proveri da li vec postoje podaci
                if (_context.Customers.Any())
                {
                    _logger.LogInformation("Database already seeded.");
                    return;
                }

                _logger.LogInformation("Seeding test data...");

                // 1. Kreiraj Merchant-a (WebShop owner)
                var merchant = CreateMerchant();

                // 2. Kreiraj Customer-a (kupac)
                var customer = CreateCustomer();

                // 3. Kreiraj Merchant Account
                var merchantAccount = CreateMerchantAccount(merchant);

                // 4. Kreiraj Customer Account
                var customerAccount = CreateCustomerAccount(customer);

                // 5. Kreiraj test kartice za customer-a
                var visaCard = CreateVisaCard(customer);
                var mastercard = CreateMastercard(customer);

                // Dodaj sredstva na account-e za testiranje
                customerAccount.Balance = 5000.00m;
                customerAccount.AvailableBalance = 5000.00m;
                merchantAccount.Balance = 10000.00m;
                merchantAccount.AvailableBalance = 10000.00m;

                _context.SaveChanges();

                _logger.LogInformation($"Test data created: Merchant={merchant.FullName}, Customer={customer.FullName}");
                _logger.LogInformation($"Merchant Account: {merchantAccount.AccountNumber}, Balance: {merchantAccount.Balance}");
                _logger.LogInformation($"Customer Account: {customerAccount.AccountNumber}, Balance: {customerAccount.Balance}");
                _logger.LogInformation($"Cards: Visa={visaCard.MaskedPan}, Mastercard={mastercard.MaskedPan}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding test data");
                throw;
            }
        }

        private Customer CreateMerchant()
        {
            var merchant = new Customer
            {
                Id = "CUST_MERCHANT",
                FullName = "WebShop d.o.o.",
                EmailHash = HashData("webshop@example.com"),
                PhoneHash = HashData("+38111111111"),
                Status = Customer.CustomerStatus.ACTIVE,
                WebShopUserIdHash = HashData("webshop_owner_123")
            };

            _context.Customers.Add(merchant);
            return merchant;
        }

        private Customer CreateCustomer()
        {
            var customer = new Customer
            {
                Id = "CUST_BUYER001",
                FullName = "Petar Petrovic",
                EmailHash = HashData("petar.petrovic@example.com"),
                PhoneHash = HashData("+381641112233"),
                Status = Customer.CustomerStatus.ACTIVE,
                WebShopUserIdHash = HashData("user_789")
            };

            _context.Customers.Add(customer);
            return customer;
        }

        private BankAccount CreateMerchantAccount(Customer merchant)
        {
            var account = new BankAccount
            {
                AccountNumber = "RS35260005601001611379",
                SwiftCode = "BACXRSBG",
                Balance = 0,
                AvailableBalance = 0,
                ReservedBalance = 0,
                PendingCaptureBalance = 0,
                Currency = "EUR",
                IsMerchantAccount = true,
                MerchantId = "WEBSHOP_001", // Must match WebShop.MerchantId in PSP
                CustomerId = merchant.Id,
                Customer = merchant
            };

            _context.BankAccounts.Add(account);
            return account;
        }

        private BankAccount CreateCustomerAccount(Customer customer)
        {
            var account = new BankAccount
            {
                AccountNumber = "RS35265005601001234567",
                SwiftCode = "BACXRSBG",
                Balance = 0,
                AvailableBalance = 0,
                ReservedBalance = 0,
                PendingCaptureBalance = 0,
                Currency = "EUR",
                IsMerchantAccount = false,
                MerchantId = null,
                CustomerId = customer.Id,
                Customer = customer
            };

            _context.BankAccounts.Add(account);
            return account;
        }

        private Card CreateVisaCard(Customer customer)
        {
            var pan = "4111111111111111"; // Test Visa card
            var card = new Card
            {
                CardHash = GenerateCardHash(pan),
                MaskedPan = "4111********1111",
                LastFourDigits = "1111",
                CardholderName = "PETAR PETROVIC",
                ExpiryMonth = "12",
                ExpiryYear = "28", // 2028
                Type = Card.CardType.CREDIT_VISA,
                Status = Card.CardStatus.ACTIVE,
                CustomerId = customer.Id,
                Customer = customer,
                IssuedAt = DateTime.UtcNow.AddYears(-1)
            };

            _context.Cards.Add(card);
            return card;
        }

        private Card CreateMastercard(Customer customer)
        {
            var pan = "5555555555554444"; // Test Mastercard
            var card = new Card
            {
                CardHash = GenerateCardHash(pan),
                MaskedPan = "5555********4444",
                LastFourDigits = "4444",
                CardholderName = "PETAR PETROVIC",
                ExpiryMonth = "06",
                ExpiryYear = "27", // 2027
                Type = Card.CardType.CREDIT_MASTERCARD,
                Status = Card.CardStatus.ACTIVE,
                CustomerId = customer.Id,
                Customer = customer,
                IssuedAt = DateTime.UtcNow.AddYears(-1)
            };

            _context.Cards.Add(card);
            return card;
        }

        // Helper metode za hash
        private string HashData(string data)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(data);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string GenerateCardHash(string pan)
        {
            using var sha256 = SHA256.Create();
            var panBytes = Encoding.UTF8.GetBytes(pan + GetCardHashSalt());
            var hashBytes = sha256.ComputeHash(panBytes);
            return Convert.ToBase64String(hashBytes);
        }

        private string GetCardHashSalt()
        {
            return Environment.GetEnvironmentVariable("CARD_HASH_SALT") ??
                   "default-card-salt-change-in-production"; 
        }
    }
}
