using System.ComponentModel.DataAnnotations;
using WebShop.API.Models;

namespace WebShop.API.DTOs
{
    public class UpdateVehicleDto
    {
        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(100)]
        public string? Model { get; set; }

        [Range(1900, 2100)]
        public int? Year { get; set; }

        public VehicleCategory? Category { get; set; }

        [Range(0.01, 10000)]
        public decimal? PricePerDay { get; set; }

        public TransmissionType? Transmission { get; set; }

        public FuelType? FuelType { get; set; }

        [Range(2, 50)]
        public int? Seats { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public VehicleStatus? Status { get; set; }

        [Range(0, 1000000)]
        public int? Mileage { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
