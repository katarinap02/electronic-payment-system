namespace Crypto.API.Models
{
    public class MerchantWallet
    {
        public long Id { get; set; }
        public string MerchantId { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public string EncryptedWalletAddress { get; set; } = string.Empty; // Encrypted wallet address
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
