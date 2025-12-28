namespace WebShop.API.Models
{
    public class Vehicle
    {
        public long Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public VehicleCategory Category { get; set; }
        public decimal PricePerDay { get; set; }
        public TransmissionType Transmission { get; set; }
        public FuelType FuelType { get; set; }
        public int Seats { get; set; }
        public string? ImageUrl { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;
        public int Mileage { get; set; }
        public string Color { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (for future reservations)
        // public ICollection<Reservation>? Reservations { get; set; }
    }
}
