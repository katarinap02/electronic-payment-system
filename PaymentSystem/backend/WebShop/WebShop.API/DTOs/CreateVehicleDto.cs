using System.ComponentModel.DataAnnotations;
using WebShop.API.Models;

namespace WebShop.API.DTOs
{
    public class CreateVehicleDto
    {
        [Required(ErrorMessage = "Brand is required")]
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required")]
        [MaxLength(100)]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required")]
        [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public VehicleCategory Category { get; set; }

        [Required(ErrorMessage = "Price per day is required")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000")]
        public decimal PricePerDay { get; set; }

        [Required(ErrorMessage = "Transmission type is required")]
        public TransmissionType Transmission { get; set; }

        [Required(ErrorMessage = "Fuel type is required")]
        public FuelType FuelType { get; set; }

        [Required(ErrorMessage = "Number of seats is required")]
        [Range(2, 50, ErrorMessage = "Seats must be between 2 and 50")]
        public int Seats { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "License plate is required")]
        [MaxLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mileage is required")]
        [Range(0, 1000000, ErrorMessage = "Mileage must be between 0 and 1,000,000")]
        public int Mileage { get; set; }

        [Required(ErrorMessage = "Color is required")]
        [MaxLength(50)]
        public string Color { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
