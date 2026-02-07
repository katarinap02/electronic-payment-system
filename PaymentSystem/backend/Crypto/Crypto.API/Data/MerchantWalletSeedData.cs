using Crypto.API.Models;

namespace Crypto.API.Data
{
    public static class MerchantWalletSeedData
    {
        public static void SeedMerchantWallets(CryptoDbContext context)
        {
            if (context.MerchantWallets.Any())
                return; // Already seeded

            var wallets = new List<MerchantWallet>
            {
                new MerchantWallet
                {
                    MerchantId = "WEBSHOP_001",
                    MerchantName = "Car Rental WebShop",
                    WalletAddress = "0x02ee6C54748Ff5B968BeC71C2d1351C9dc5f8Aa0",
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.MerchantWallets.AddRange(wallets);
            context.SaveChanges();
        }
    }
}
