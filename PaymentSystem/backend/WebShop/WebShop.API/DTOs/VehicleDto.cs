using WebShop.API.Models;

namespace WebShop.API.DTOs
{
    public class VehicleDto
    {
        public long Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public string Transmission { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public int Seats { get; set; }
        public string? ImageUrl { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Mileage { get; set; }
        public string Color { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
