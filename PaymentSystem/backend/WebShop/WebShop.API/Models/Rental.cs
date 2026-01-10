using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShop.API.Models
{
    /// <summary>
    /// Model za iznajmljivanje vozila - kompletna evidencija kupovine
    /// </summary>
    public class Rental
    {
        [Key]
        public int Id { get; set; }

        // Informacije o korisniku
        [Required]
        public long UserId { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        // Informacije o vozilu
        [Required]
        public long VehicleId { get; set; }
        
        [ForeignKey(nameof(VehicleId))]
        public Vehicle Vehicle { get; set; } = null!;

        // Datumi iznajmljivanja
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public int RentalDays { get; set; } // Broj dana

        // Dodatne usluge
        public string? AdditionalServices { get; set; } // JSON array ili comma-separated
        public decimal AdditionalServicesPrice { get; set; }

        // Osiguranje
        public string? InsuranceType { get; set; } // Basic, Standard, Premium
        public decimal InsurancePrice { get; set; }

        // Cene
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal VehiclePricePerDay { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // Informacije o plaćanju
        [Required]
        [MaxLength(50)]
        public string PaymentId { get; set; } = string.Empty; // Bank PaymentId
        
        [MaxLength(100)]
        public string? GlobalTransactionId { get; set; } // Bank GlobalTransactionId
        
        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "EUR"; // RSD, EUR, USD
        
        [Required]
        [MaxLength(20)]
        public string PaymentMethod { get; set; } = string.Empty; // CreditCard, QR_CODE

        // Status
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Completed, Cancelled
        
        // Vremena
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Dodatne informacije
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

