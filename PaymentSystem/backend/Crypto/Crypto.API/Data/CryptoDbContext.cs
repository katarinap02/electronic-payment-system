using Microsoft.EntityFrameworkCore;
using Crypto.API.Models;

namespace Crypto.API.Data
{
    public class CryptoDbContext : DbContext
    {
        public CryptoDbContext(DbContextOptions<CryptoDbContext> options) : base(options) { }

        public DbSet<CryptoTransaction> CryptoTransactions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<MerchantWallet> MerchantWallets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CryptoTransaction>(entity =>
            {
                entity.ToTable("crypto_transactions");
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.PspTransactionId)
                      .IsUnique()
                      .HasDatabaseName("ix_crypto_psp_id");

                entity.HasIndex(e => e.CryptoPaymentId)
                      .IsUnique()
                      .HasDatabaseName("ix_crypto_payment_id");

                entity.HasIndex(e => e.MerchantId)
                      .HasDatabaseName("ix_crypto_merchant");

                entity.HasIndex(e => e.Status)
                      .HasDatabaseName("ix_crypto_status");

                entity.Property(e => e.Amount)
                      .HasPrecision(18, 2);

                entity.Property(e => e.AmountInCrypto)
                      .HasPrecision(18, 8);

                entity.Property(e => e.ExchangeRate)
                      .HasPrecision(18, 2);

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.Property(e => e.Currency)
                      .HasMaxLength(10)
                      .HasDefaultValue("EUR");

                entity.Property(e => e.CryptoSymbol)
                      .HasMaxLength(10)
                      .HasDefaultValue("ETH");

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.CryptoPaymentId).IsRequired();
                entity.Property(e => e.PspTransactionId).IsRequired();
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_logs");
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Timestamp)
                      .HasDatabaseName("ix_audit_timestamp");

                entity.HasIndex(e => e.TransactionId)
                      .HasDatabaseName("ix_audit_transaction");

                entity.Property(e => e.Timestamp)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Action)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.IpAddress)
                      .IsRequired()
                      .HasMaxLength(45);

                entity.Property(e => e.Result)
                      .HasMaxLength(100);
            });

            modelBuilder.Entity<MerchantWallet>(entity =>
            {
                entity.ToTable("merchant_wallets");
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.MerchantId)
                      .IsUnique()
                      .HasDatabaseName("ix_merchant_id");

                entity.Property(e => e.MerchantId)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.MerchantName)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.EncryptedWalletAddress)
                    .HasColumnName("encryptedwalletaddress")
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
