using Bank.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<CardToken> CardTokens { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Card> Cards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasMaxLength(50);
                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.EmailHash)
                    .HasMaxLength(128);
                entity.Property(e => e.PhoneHash)
                    .HasMaxLength(128);
                entity.Property(e => e.WebShopUserIdHash)
                    .HasMaxLength(128);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasMany(e => e.Accounts)
                    .WithOne(a => a.Customer)
                    .HasForeignKey(a => a.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Cards)
                    .WithOne(c => c.Customer)
                    .HasForeignKey(c => c.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AccountNumber)
                    .IsRequired()
                    .HasMaxLength(34);
                entity.Property(e => e.SwiftCode)
                    .HasMaxLength(11)
                    .HasDefaultValue("BACXRSBG");
                entity.Property(e => e.Balance)
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0);
                entity.Property(e => e.AvailableBalance)
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0);
                entity.Property(e => e.ReservedBalance)
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0);
                entity.Property(e => e.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("EUR");

                entity.HasIndex(e => e.MerchantId)
                    .IsUnique()
                    .HasFilter("\"MerchantId\" IS NOT NULL");

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Accounts)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Transactions)
                    .WithOne(t => t.MerchantAccount)
                    .HasForeignKey(t => t.MerchantAccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CardHash)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(e => e.MaskedPan)
                    .HasMaxLength(19);
                entity.Property(e => e.LastFourDigits)
                    .HasMaxLength(4);
                entity.Property(e => e.CardholderName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.ExpiryMonth)
                    .HasMaxLength(2);
                entity.Property(e => e.ExpiryYear)
                    .HasMaxLength(2);
                entity.Property(e => e.CvvSalt)
                    .HasMaxLength(128);
                entity.Property(e => e.IssuedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");


                entity.HasIndex(e => e.CardHash)
                    .IsUnique();

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Cards)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Tokens)
                    .WithOne(t => t.Card)
                    .HasForeignKey(t => t.CardId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<CardToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.CvvHash)
                    .HasMaxLength(128);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.ExpiresAt)
                    .IsRequired();
                entity.Property(e => e.IsUsed)
                    .HasDefaultValue(false);

                entity.HasIndex(e => e.Token)
                    .IsUnique();

                entity.HasOne(e => e.Card)
                    .WithMany(c => c.Tokens)
                    .HasForeignKey(e => e.CardId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Transaction)
                    .WithOne(t => t.CardToken)
                    .HasForeignKey<CardToken>(e => e.TransactionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.ExpiresAt);
                entity.HasIndex(e => e.IsUsed);
            });

            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PaymentId)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.MerchantId)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Stan)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.GlobalTransactionId)
                    .HasMaxLength(100);
                entity.Property(e => e.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();
                entity.Property(e => e.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("EUR");
                entity.Property(e => e.ExpiresAt)
                    .IsRequired();
                entity.Property(e => e.MerchantTimestamp)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.PspTimestamp)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.AcquirerTimestamp)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(e => e.PaymentId)
                    .IsUnique();
                entity.HasIndex(e => e.Stan)
                    .IsUnique();

                entity.HasIndex(e => e.GlobalTransactionId)
                    .IsUnique()
                    .HasFilter("\"GlobalTransactionId\" IS NOT NULL");

                entity.HasOne(e => e.MerchantAccount)
                    .WithMany(a => a.Transactions)
                    .HasForeignKey(e => e.MerchantAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CustomerAccount)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Card)
                    .WithMany()
                    .HasForeignKey(e => e.CardId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CardToken)
                    .WithOne(t => t.Transaction)
                    .HasForeignKey<PaymentTransaction>(e => e.CardTokenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ExpiresAt);
                entity.HasIndex(e => e.MerchantTimestamp);
                entity.HasIndex(e => new { e.MerchantId, e.Status });
            });
        }
    }
}