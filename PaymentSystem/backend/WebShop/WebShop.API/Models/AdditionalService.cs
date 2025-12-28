namespace WebShop.API.Models
{
    public class AdditionalService
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? IconUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (Many-to-Many with Reservation)
        // public ICollection<ReservationService>? ReservationServices { get; set; }
    }
}
