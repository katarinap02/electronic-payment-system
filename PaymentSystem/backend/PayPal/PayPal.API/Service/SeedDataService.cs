using PayPal.API.Data;
using PayPal.API.Models;

namespace PayPal.API.Service
{
    public class SeedDataService
    {/*
        private readonly PayPalDbContext _context;
        private readonly ILogger<SeedDataService> _logger;
        private readonly EncryptionService _encryption;

        public SeedDataService(
            PayPalDbContext context,
            ILogger<SeedDataService> logger,
            EncryptionService encryption)
        {
            _context = context;
            _logger = logger;
            _encryption = encryption;
        }

        public void InitializeTestData()
        {
            try
            {
                // Proveri da li već postoje podaci
                if (_context.PaypalTransactions.Any())
                {
                    _logger.LogInformation("PayPal database already seeded.");
                    return;
                }

                _logger.LogInformation("Seeding PayPal test data...");

                //  Kreiraj primer uspešne transakcije (za demonstraciju PCI DSS)
                CreateSampleTransaction(
                    pspId: "PSP-TEST-001",
                    merchantId: "WEBSHOP_001",
                    amount: 150.00m,
                    status: PaypalTransaction.PaypalStatus.CAPTURED,
                    createdAt: DateTime.UtcNow.AddHours(-2),
                    ip: "192.168.1.100"
                );

                //  Kreiraj primer pending transakcije
                CreateSampleTransaction(
                    pspId: "PSP-TEST-002",
                    merchantId: "WEBSHOP_001",
                    amount: 299.99m,
                    status: PaypalTransaction.PaypalStatus.PENDING,
                    createdAt: DateTime.UtcNow.AddMinutes(-5),
                    ip: "192.168.1.101"
                );

                // Kreiraj primer cancelled transakcije
                CreateSampleTransaction(
                    pspId: "PSP-TEST-003",
                    merchantId: "WEBSHOP_002",
                    amount: 50.00m,
                    status: PaypalTransaction.PaypalStatus.CANCELLED,
                    createdAt: DateTime.UtcNow.AddHours(-1),
                    ip: "192.168.1.102"
                );

                //  Kreiraj primer audit logova (za demonstraciju Zahteva 5.1)
                CreateSampleAuditLogs();

                _context.SaveChanges();

                _logger.LogInformation("PayPal test data created successfully:");
                _logger.LogInformation("- 3 sample transactions (CAPTURED, PENDING, CANCELLED)");
                _logger.LogInformation("- 4 audit log entries");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding PayPal test data");
                throw;
            }
        }

        private void CreateSampleTransaction(
            string pspId,
            string merchantId,
            decimal amount,
            PaypalTransaction.PaypalStatus status,
            DateTime createdAt,
            string ip)
        {
            // Enkriptuj senzitivne podatke (PCI DSS Zahtev 2)
            var orderId = $"ORDER-{Guid.NewGuid().ToString().Substring(0, 8)}";
            var payerId = status == PaypalTransaction.PaypalStatus.PENDING ? null : $"PAYER-{Guid.NewGuid().ToString().Substring(0, 8)}";
            var captureId = status == PaypalTransaction.PaypalStatus.CAPTURED ? $"CAPT-{Guid.NewGuid().ToString().Substring(0, 8)}" : null;

            var transaction = new PaypalTransaction
            {
                PspTransactionId = pspId,
                EncryptedPayPalOrderId = _encryption.Encrypt(orderId),
                MerchantId = merchantId,
                Amount = amount,
                Currency = "EUR",
                Status = status,
                EncryptedPayerId = payerId != null ? _encryption.Encrypt(payerId) : null,
                EncryptedCaptureId = captureId != null ? _encryption.Encrypt(captureId) : null,
                CreatedAt = createdAt,
                CompletedAt = status == PaypalTransaction.PaypalStatus.CAPTURED || status == PaypalTransaction.PaypalStatus.CANCELLED
                    ? createdAt.AddMinutes(5)
                    : null,
                CreatedByIp = ip,
                UserAgent = "Mozilla/5.0 (Test User Agent) Chrome/120.0"
            };

            _context.PaypalTransactions.Add(transaction);
            _logger.LogInformation($"Created {status} transaction: {pspId}, Amount: {amount} EUR");
        }

        private void CreateSampleAuditLogs()
        {
            var auditEntries = new[]
            {
            new AuditLog
            {
                Action = "CREATE_ORDER",
                TransactionId = "PSP-TEST-001",
                IpAddress = "192.168.1.100",
                Timestamp = DateTime.UtcNow.AddHours(-2),
                Result = "SUCCESS",
                Details = "Payment initiated successfully"
            },
            new AuditLog
            {
                Action = "VIEW_TRANSACTION",
                TransactionId = "PSP-TEST-001",
                IpAddress = "10.0.0.50",
                Timestamp = DateTime.UtcNow.AddHours(-1).AddMinutes(30),
                Result = "SUCCESS",
                Details = "Admin viewed transaction details"
            },
            new AuditLog
            {
                Action = "UPDATE_STATUS",
                TransactionId = "PSP-TEST-001",
                IpAddress = "192.168.1.100",
                Timestamp = DateTime.UtcNow.AddHours(-1).AddMinutes(55),
                Result = "SUCCESS",
                Details = "Status changed from PENDING to CAPTURED"
            },
            new AuditLog
            {
                Action = "CREATE_ORDER",
                TransactionId = "PSP-TEST-003",
                IpAddress = "192.168.1.102",
                Timestamp = DateTime.UtcNow.AddHours(-1),
                Result = "SUCCESS",
                Details = "Payment initiated"
            }
        };

            foreach (var entry in auditEntries)
            {
                _context.AuditLogs.Add(entry);
            }

            _logger.LogInformation($"Created {auditEntries.Length} audit log entries");
        }*/
    }
}
