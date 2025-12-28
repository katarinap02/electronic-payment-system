namespace WebShop.API.Models
{
    public class InsurancePackage
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public decimal CoverageLimit { get; set; }
        public decimal Deductible { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (for future reservations)
        // public ICollection<Reservation>? Reservations { get; set; }
    }
}
