using Crypto.API.DTOs;
using Crypto.API.Models;
using Crypto.API.Repositories;
using Crypto.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Crypto.API.Services
{
    public class CryptoPaymentService
    {
        private readonly CryptoTransactionRepository _transactionRepo;
        private readonly AuditLogRepository _auditRepo;
        private readonly Web3Service _web3Service;
        private readonly EncryptionService _encryption;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CryptoPaymentService> _logger;
        private readonly CryptoDbContext _context;

        public CryptoPaymentService(
            CryptoTransactionRepository transactionRepo,
            AuditLogRepository auditRepo,
            Web3Service web3Service,
            EncryptionService encryption,
            IConfiguration configuration,
            ILogger<CryptoPaymentService> logger,
            CryptoDbContext context)
        {
            _transactionRepo = transactionRepo;
            _auditRepo = auditRepo;
            _web3Service = web3Service;
            _encryption = encryption;
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        public async Task<CreatePaymentResponse> CreatePaymentAsync(
            CreatePaymentRequest request, 
            string ipAddress, 
            string userAgent)
        {
            try
            {
                // 1. Provera duplikata (idempotency)
                var existingTx = await _transactionRepo.GetByPspTransactionIdAsync(request.PspTransactionId);
                if (existingTx != null && existingTx.Status != CryptoTransaction.CryptoStatus.FAILED)
                {
                    _logger.LogWarning("Duplicate transaction detected: {PspId}", request.PspTransactionId);

                    var existingWalletAddress = existingTx.WalletAddress;
                    var existingPaymentUrl = GeneratePaymentUrl(existingTx.CryptoPaymentId);

                    return new CreatePaymentResponse(
                        existingTx.CryptoPaymentId,
                        existingPaymentUrl,
                        existingWalletAddress,
                        existingTx.AmountInCrypto,
                        existingTx.ExchangeRate,
                        existingTx.ExpiresAt,
                        existingTx.Status.ToString()
                    );
                }

                // 2. Dohvati exchange rate (EUR ? ETH)
                var exchangeRate = await _web3Service.GetEthToEurExchangeRateAsync();
                var amountInEth = request.Amount / exchangeRate;

                // 3. Generisi CryptoPaymentId
                var cryptoPaymentId = GenerateCryptoPaymentId();

                // 4. Merchant wallet adresa iz baze
                var merchant = await _context.MerchantWallets
                    .FirstOrDefaultAsync(m => m.MerchantId == request.MerchantId);
                
                if (merchant == null)
                {
                    _logger.LogError("Merchant not found: {MerchantId}", request.MerchantId);
                    throw new Exception($"Merchant {request.MerchantId} not found");
                }
                
                var merchantWallet = merchant.WalletAddress;

                // 5. Expiry time (15 minuta)
                var expiryMinutes = int.Parse(_configuration["CryptoSettings:PaymentExpiryMinutes"] ?? "15");
                var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

                // 6. Kreiraj transakciju
                var transaction = new CryptoTransaction
                {
                    CryptoPaymentId = cryptoPaymentId,
                    PspTransactionId = request.PspTransactionId,
                    MerchantId = request.MerchantId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    WalletAddress = merchantWallet,
                    AmountInCrypto = amountInEth,
                    CryptoSymbol = "ETH",
                    ExchangeRate = exchangeRate,
                    Status = CryptoTransaction.CryptoStatus.PENDING,
                    ExpiresAt = expiresAt,
                    CreatedByIp = ipAddress,
                    UserAgent = userAgent,
                    CreatedAt = DateTime.UtcNow
                };

                await _transactionRepo.CreateAsync(transaction);

                // 7. Audit log (PCI DSS 5.1)
                await _auditRepo.LogAsync(
                    action: "CREATE_PAYMENT",
                    transactionId: cryptoPaymentId,
                    ipAddress: ipAddress,
                    result: "SUCCESS",
                    details: $"Amount: {request.Amount} {request.Currency} ? {amountInEth:F8} ETH, Rate: {exchangeRate}"
                );

                _logger.LogInformation(
                    "Crypto payment created: {CryptoPaymentId}, PSP: {PspId}, Amount: {Amount} ETH",
                    cryptoPaymentId, request.PspTransactionId, amountInEth);

                // 8. Genersii payment URL
                var paymentUrl = GeneratePaymentUrl(cryptoPaymentId);

                return new CreatePaymentResponse(
                    cryptoPaymentId,
                    paymentUrl,
                    merchantWallet,
                    amountInEth,
                    exchangeRate,
                    expiresAt,
                    "PENDING"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating crypto payment for PSP: {PspId}", request.PspTransactionId);

                await _auditRepo.LogAsync(
                    action: "CREATE_PAYMENT",
                    transactionId: request.PspTransactionId,
                    ipAddress: ipAddress,
                    result: "FAILURE",
                    details: ex.Message
                );

                throw;
            }
        }

        public async Task<TransactionStatusResponse?> GetTransactionStatusAsync(string cryptoPaymentId)
        {
            var transaction = await _transactionRepo.GetByCryptoPaymentIdAsync(cryptoPaymentId);
            
            if (transaction == null)
                return null;

            string? txHash = null;
            if (!string.IsNullOrEmpty(transaction.EncryptedTransactionHash))
            {
                txHash = transaction.EncryptedTransactionHash; // No decryption needed
            }

            string walletAddress = transaction.WalletAddress;
            
            // Get merchant name from merchant_wallets table
            var merchant = await _context.MerchantWallets
                .FirstOrDefaultAsync(m => m.MerchantId == transaction.MerchantId);
            
            string merchantName = merchant?.MerchantName ?? "Unknown Merchant";

            return new TransactionStatusResponse(
                transaction.CryptoPaymentId,
                transaction.Status.ToString(),
                transaction.Amount,
                transaction.Currency,
                transaction.AmountInCrypto,
                txHash,
                transaction.Confirmations,
                transaction.CompletedAt,
                transaction.ExpiresAt,
                walletAddress,
                merchantName
            );
        }

        public async Task ConfirmTransactionAsync(string cryptoPaymentId, string txHash)
        {
            var transaction = await _transactionRepo.GetByCryptoPaymentIdAsync(cryptoPaymentId);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found for confirmation: {CryptoPaymentId}", cryptoPaymentId);
                return;
            }

            // Dozvoli potvrdu za PENDING i CANCELLED transakcije (ako je korisnik otkazao ali je uplatio)
            if (transaction.Status != CryptoTransaction.CryptoStatus.PENDING && 
                transaction.Status != CryptoTransaction.CryptoStatus.CANCELLED)
            {
                _logger.LogWarning("Transaction already processed: {CryptoPaymentId}, Status: {Status}",
                    cryptoPaymentId, transaction.Status);
                return;
            }

            // Validacija transakcije na blockchain-u
            var walletAddress = transaction.WalletAddress;
            var validation = await _web3Service.ValidateTransactionAsync(
                txHash,
                walletAddress,
                transaction.AmountInCrypto);

            if (!validation.IsValid)
            {
                _logger.LogWarning("Invalid blockchain transaction: {TxHash}, Error: {Error}",
                    txHash, validation.ErrorMessage);

                await _auditRepo.LogAsync(
                    action: "VALIDATE_TX",
                    transactionId: cryptoPaymentId,
                    ipAddress: "SYSTEM",
                    result: "FAILED",
                    details: validation.ErrorMessage
                );

                return;
            }


            // Azuriraj status na CONFIRMING
            transaction.EncryptedTransactionHash = txHash; // Plain text, not encrypted
            transaction.Status = CryptoTransaction.CryptoStatus.CONFIRMING;
            transaction.Confirmations = 0;
            await _transactionRepo.UpdateAsync(transaction);

            await _auditRepo.LogAsync(
                action: "TX_FOUND",
                transactionId: cryptoPaymentId,
                ipAddress: "SYSTEM",
                result: "SUCCESS",
                details: $"TxHash: {txHash}, Confirmations: 0"
            );

            _logger.LogInformation("Transaction found on blockchain: {CryptoPaymentId}, TxHash: {TxHash}",
                cryptoPaymentId, txHash);
        }

        public async Task CaptureTransactionAsync(string cryptoPaymentId)
        {
            var transaction = await _transactionRepo.GetByCryptoPaymentIdAsync(cryptoPaymentId);
            if (transaction.Status != CryptoTransaction.CryptoStatus.CONFIRMING)
            {
                _logger.LogWarning("Cannot capture transaction in status: {Status}", transaction.Status);
                return;
            }

            var txHash = transaction.EncryptedTransactionHash!; // No decryption needed
            var confirmations = await _web3Service.GetTransactionConfirmationsAsync(txHash);

            var requiredConfirmations = int.Parse(_configuration["CryptoSettings:ConfirmationsRequired"] ?? "1");

            if (confirmations >= requiredConfirmations)
            {
                transaction.Status = CryptoTransaction.CryptoStatus.CAPTURED;
                transaction.Confirmations = confirmations;
                transaction.CompletedAt = DateTime.UtcNow;
                await _transactionRepo.UpdateAsync(transaction);

                await _auditRepo.LogAsync(
                    action: "CAPTURE_PAYMENT",
                    transactionId: cryptoPaymentId,
                    ipAddress: "SYSTEM",
                    result: "SUCCESS",
                    details: $"TxHash: {txHash}, Confirmations: {confirmations}"
                );

                _logger.LogInformation("Payment captured: {CryptoPaymentId}, Confirmations: {Confirmations}", 
                    cryptoPaymentId, confirmations);
            }
            else
            {
                transaction.Confirmations = confirmations;
                await _transactionRepo.UpdateAsync(transaction);

                _logger.LogInformation("Waiting for more confirmations: {CryptoPaymentId}, Current: {Current}, Required: {Required}",
                    cryptoPaymentId, confirmations, requiredConfirmations);
            }
        }

        public async Task ExpireTransactionAsync(string cryptoPaymentId)
        {
            var transaction = await _transactionRepo.GetByCryptoPaymentIdAsync(cryptoPaymentId);
            if (transaction == null) return;

            if (transaction.Status != CryptoTransaction.CryptoStatus.PENDING)
                return;

            transaction.Status = CryptoTransaction.CryptoStatus.EXPIRED;
            await _transactionRepo.UpdateAsync(transaction);

            await _auditRepo.LogAsync(
                action: "EXPIRE_PAYMENT",
                transactionId: cryptoPaymentId,
                ipAddress: "SYSTEM",
                result: "SUCCESS",
                details: "Payment expired due to timeout"
            );

            _logger.LogInformation("Payment expired: {CryptoPaymentId}", cryptoPaymentId);
        }

        public async Task CancelTransactionAsync(string cryptoPaymentId)
        {
            var transaction = await _transactionRepo.GetByCryptoPaymentIdAsync(cryptoPaymentId);
            if (transaction == null)
                throw new InvalidOperationException("Transaction not found");

            if (transaction.Status != CryptoTransaction.CryptoStatus.PENDING)
                throw new InvalidOperationException("Can only cancel pending transactions");

            transaction.Status = CryptoTransaction.CryptoStatus.CANCELLED;
            await _transactionRepo.UpdateAsync(transaction);

            await _auditRepo.LogAsync(
                action: "CANCEL_PAYMENT",
                transactionId: cryptoPaymentId,
                ipAddress: "USER",
                result: "SUCCESS",
                details: "User cancelled payment"
            );

            _logger.LogInformation("Payment cancelled by user: {CryptoPaymentId}", cryptoPaymentId);
        }

        private string GenerateCryptoPaymentId()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[16];
            rng.GetBytes(bytes);
            return "crypto_" + Convert.ToBase64String(bytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .Substring(0, 16)
                .ToLower();
        }

        private string GeneratePaymentUrl(string cryptoPaymentId)
        {
            var baseUrl = _configuration["CryptoSettings:FrontendUrl"] ?? "http://localhost:5175";
            return $"{baseUrl}/crypto-payment/{cryptoPaymentId}";
        }
    }
}











