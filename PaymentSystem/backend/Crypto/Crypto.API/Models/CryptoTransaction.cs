namespace Crypto.API.Models
{
    public class CryptoTransaction
    {
        public long Id { get; set; }
        public string CryptoPaymentId { get; set; } = string.Empty;
        public string PspTransactionId { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";

        public string WalletAddress { get; set; } = string.Empty;
        public decimal AmountInCrypto { get; set; }
        public string CryptoSymbol { get; set; } = "ETH";
        public decimal ExchangeRate { get; set; }

        public string? EncryptedTransactionHash { get; set; }
        public int Confirmations { get; set; } = 0;

        public CryptoStatus Status { get; set; } = CryptoStatus.PENDING;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public string CreatedByIp { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;

        public enum CryptoStatus
        {
            PENDING,
            CONFIRMING,
            CAPTURED,
            FAILED,
            EXPIRED,
            CANCELLED
        }
    }
}

