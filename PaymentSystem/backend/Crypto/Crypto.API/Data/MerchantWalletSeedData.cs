using Crypto.API.Models;
using Crypto.API.Services;

namespace Crypto.API.Data
{
    public static class MerchantWalletSeedData
    {
        public static void SeedMerchantWallets(CryptoDbContext context, EncryptionService encryptionService)
        {
            if (context.MerchantWallets.Any())
                return; // Already seeded

            var plainWalletAddress = "0x02ee6C54748Ff5B968BeC71C2d1351C9dc5f8Aa0";
            var encryptedWalletAddress = encryptionService.Encrypt(plainWalletAddress);

            var wallets = new List<MerchantWallet>
            {
                new MerchantWallet
                {
                    MerchantId = "WEBSHOP_001",
                    MerchantName = "Car Rental WebShop",
                    EncryptedWalletAddress = encryptedWalletAddress,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.MerchantWallets.AddRange(wallets);
            context.SaveChanges();
        }
    }
}
