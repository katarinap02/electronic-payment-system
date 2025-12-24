using Bank.API.Models;
using Microsoft.EntityFrameworkCore;


namespace Bank.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) {}

        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<CardToken> CardTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracija za BankAccount
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AccountNumber)
                    .IsRequired()
                    .HasMaxLength(34);
                entity.Property(e => e.Balance)
                    .HasPrecision(18, 2); // Za decimal precision
                entity.HasIndex(e => e.MerchantId).IsUnique(); // Unique za merchante
            });

            // Konfiguracija za PaymentTransaction
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount)
                    .HasPrecision(18, 2);
                entity.HasIndex(e => e.PaymentId).IsUnique(); // Jedinstven payment_id
                entity.HasIndex(e => e.Stan).IsUnique(); // Jedinstven STAN

                // Veze ka BankAccount
                entity.HasOne(e => e.MerchantAccount)
                    .WithMany(a => a.MerchantTransactions)
                    .HasForeignKey(e => e.MerchantAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CustomerAccount)
                    .WithMany(a => a.CustomerTransactions)
                    .HasForeignKey(e => e.CustomerAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Veza ka CardToken (opciona)
                entity.HasOne(e => e.CardToken)
                    .WithOne(t => t.Transaction)
                    .HasForeignKey<CardToken>(t => t.TransactionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Konfiguracija za CardToken
            modelBuilder.Entity<CardToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(e => e.Token).IsUnique(); // Unique token
                entity.HasIndex(e => e.CardHash).IsUnique(); // Unique card hash
            });
        }
    }
}
