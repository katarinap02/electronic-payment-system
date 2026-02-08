namespace Crypto.API.Models
{
    public class CryptoTransaction
    {
        public long Id { get; set; }
        public string CryptoPaymentId { get; set; } = string.Empty;
        public string PspTransactionId { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public string? CustomerId { get; set; } // Optional: User/Customer ID from WebShop

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";

        public string EncryptedWalletAddress { get; set; } = string.Empty; // Encrypted merchant wallet address
        public decimal AmountInCrypto { get; set; }
        public string CryptoSymbol { get; set; } = "ETH";
        public decimal ExchangeRate { get; set; }

        public string? EncryptedTransactionHash { get; set; } // Encrypted transaction hash from blockchain
        public CryptoStatus Status { get; set; } = CryptoStatus.PENDING;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public string CreatedByIp { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;

        public enum CryptoStatus
        {
            PENDING,
            CONFIRMING,
            CAPTURED,
            COMPLETED,
            FAILED,
            EXPIRED,
            CANCELLED
        }
    }
}

