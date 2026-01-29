using Microsoft.EntityFrameworkCore;
using PayPal.API.Models;

namespace PayPal.API.Data
{
    public class PayPalDbContext : DbContext
    {
        public PayPalDbContext(DbContextOptions<PayPalDbContext> options) : base(options) { }

        public DbSet<PaypalTransaction> PaypalTransactions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PaypalTransaction>(entity =>
            {
                entity.ToTable("paypal_transactions");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.PspTransactionId)
                      .IsUnique()
                      .HasDatabaseName("ix_paypal_psp_id");

                entity.HasIndex(e => e.MerchantId)
                      .HasDatabaseName("ix_paypal_merchant");

                entity.Property(e => e.Amount)
                      .HasPrecision(18, 2);

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.Property(e => e.Currency)
                      .HasMaxLength(3)
                      .HasDefaultValue("EUR");

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.EncryptedPayPalOrderId).IsRequired();
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
                      .HasMaxLength(50); // "CREATE_ORDER", "VIEW", "UPDATE"

                entity.Property(e => e.IpAddress)
                      .IsRequired()
                      .HasMaxLength(45); 

                entity.Property(e => e.Result)
                      .HasMaxLength(100);
            });
        }
    }
}