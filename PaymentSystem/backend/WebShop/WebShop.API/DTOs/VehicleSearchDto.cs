using WebShop.API.Models;

namespace WebShop.API.DTOs
{
    public class VehicleSearchDto
    {
        public VehicleCategory? Category { get; set; }
        public TransmissionType? Transmission { get; set; }
        public FuelType? FuelType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinSeats { get; set; }
        public int? MaxSeats { get; set; }
        public VehicleStatus? Status { get; set; }
        public string? Brand { get; set; }
        public int? Year { get; set; }
    }
}
