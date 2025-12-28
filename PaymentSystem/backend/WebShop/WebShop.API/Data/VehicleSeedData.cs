using WebShop.API.Data;
using WebShop.API.Models;

namespace WebShop.API.Data
{
    public static class VehicleSeedData
    {
        public static void SeedVehicles(AppDbContext context)
        {
            if (context.Vehicles.Any())
                return; // Already seeded

            var vehicles = new List<Vehicle>
            {
                // Economy vehicles
                new Vehicle
                {
                    Brand = "Volkswagen",
                    Model = "Polo",
                    Year = 2022,
                    Category = VehicleCategory.Economy,
                    PricePerDay = 25.00m,
                    Transmission = TransmissionType.Manual,
                    FuelType = FuelType.Petrol,
                    Seats = 5,
                    LicensePlate = "BG-123-AB",
                    Status = VehicleStatus.Available,
                    Mileage = 15000,
                    Color = "White",
                    Description = "Economical and reliable city car",
                    ImageUrl = "https://example.com/images/polo.jpg"
                },
                new Vehicle
                {
                    Brand = "Fiat",
                    Model = "Punto",
                    Year = 2021,
                    Category = VehicleCategory.Economy,
                    PricePerDay = 22.00m,
                    Transmission = TransmissionType.Manual,
                    FuelType = FuelType.Petrol,
                    Seats = 5,
                    LicensePlate = "BG-456-CD",
                    Status = VehicleStatus.Available,
                    Mileage = 22000,
                    Color = "Red",
                    Description = "Compact and fuel-efficient",
                    ImageUrl = "https://example.com/images/punto.jpg"
                },

                // Comfort vehicles
                new Vehicle
                {
                    Brand = "Volkswagen",
                    Model = "Passat",
                    Year = 2023,
                    Category = VehicleCategory.Comfort,
                    PricePerDay = 45.00m,
                    Transmission = TransmissionType.Automatic,
                    FuelType = FuelType.Diesel,
                    Seats = 5,
                    LicensePlate = "BG-789-EF",
                    Status = VehicleStatus.Available,
                    Mileage = 8000,
                    Color = "Black",
                    Description = "Comfortable mid-size sedan with automatic transmission",
                    ImageUrl = "https://example.com/images/passat.jpg"
                },
                new Vehicle
                {
                    Brand = "Skoda",
                    Model = "Octavia",
                    Year = 2023,
                    Category = VehicleCategory.Comfort,
                    PricePerDay = 40.00m,
                    Transmission = TransmissionType.Automatic,
                    FuelType = FuelType.Diesel,
                    Seats = 5,
                    LicensePlate = "BG-321-GH",
                    Status = VehicleStatus.Available,
                    Mileage = 12000,
                    Color = "Gray",
                    Description = "Spacious and practical family car",
                    ImageUrl = "https://example.com/images/octavia.jpg"
                },

                // Luxury vehicles
                new Vehicle
                {
                    Brand = "Mercedes-Benz",
                    Model = "E-Class",
                    Year = 2024,
                    Category = VehicleCategory.Luxury,
                    PricePerDay = 80.00m,
                    Transmission = TransmissionType.Automatic,
                    FuelType = FuelType.Diesel,
                    Seats = 5,
                    LicensePlate = "BG-654-IJ",
                    Status = VehicleStatus.Available,
                    Mileage = 3000,
                    Color = "Silver",
                    Description = "Premium luxury sedan with advanced features",
                    ImageUrl = "https://example.com/images/e-class.jpg"
                },
                new Vehicle
                {
                    Brand = "BMW",
                    Model = "5 Series",
                    Year = 2024,
                    Category = VehicleCategory.Luxury,
                    PricePerDay = 85.00m,
                    Transmission = TransmissionType.Automatic,
                    FuelType = FuelType.Hybrid,
                    Seats = 5,
                    LicensePlate = "BG-987-KL",
                    Status = VehicleStatus.Available,
                    Mileage = 5000,
                    Color = "Blue",
                    Description = "Luxurious and powerful hybrid sedan",
                    ImageUrl = "https://example.com/images/bmw-5.jpg"
                },

                // SUV vehicles
                new Vehicle
                {
                    Brand = "Toyota",
                    Model = "RAV4",
                    Year = 2023,
                    Category = VehicleCategory.SUV,
                    PricePerDay = 55.00m,
                    Transmission = TransmissionType.Automatic,
                    FuelType = FuelType.Hybrid,
                    Seats = 5,
                    LicensePlate = "BG-135-MN",
                    Status = VehicleStatus.Available,
                    Mileage = 10000,
                    Color = "White",
                    Description = "Reliable hybrid SUV perfect for family trips",
                    ImageUrl = "https://example.com/images/rav4.jpg"
                },
                new Vehicle
                {
                    Brand = "Nissan",
                    Model = "X-Trail",
                    Year = 2022,
                    Category = VehicleCategory.SUV,
                    PricePerDay = 50.00m,
                    Transmission = TransmissionType.Automatic,
                    FuelType = FuelType.Diesel,
                    Seats = 7,
                    LicensePlate = "BG-246-OP",
                    Status = VehicleStatus.Available,
                    Mileage = 18000,
                    Color = "Black",
                    Description = "Spacious 7-seater SUV",
                    ImageUrl = "https://example.com/images/xtrail.jpg"
                },

                // Van vehicles
                new Vehicle
                {
                    Brand = "Mercedes-Benz",
                    Model = "Vito",
                    Year = 2023,
                    Category = VehicleCategory.Van,
                    PricePerDay = 60.00m,
                    Transmission = TransmissionType.Manual,
                    FuelType = FuelType.Diesel,
                    Seats = 9,
                    LicensePlate = "BG-357-QR",
                    Status = VehicleStatus.Available,
                    Mileage = 14000,
                    Color = "Silver",
                    Description = "9-seater van perfect for group travel",
                    ImageUrl = "https://example.com/images/vito.jpg"
                },
                new Vehicle
                {
                    Brand = "Volkswagen",
                    Model = "Transporter",
                    Year = 2022,
                    Category = VehicleCategory.Van,
                    PricePerDay = 55.00m,
                    Transmission = TransmissionType.Manual,
                    FuelType = FuelType.Diesel,
                    Seats = 9,
                    LicensePlate = "BG-468-ST",
                    Status = VehicleStatus.Available,
                    Mileage = 20000,
                    Color = "White",
                    Description = "Comfortable van for larger groups",
                    ImageUrl = "https://example.com/images/transporter.jpg"
                },

                // Sport vehicles
                new Vehicle
                {
                    Brand = "Audi",
                    Model = "A5 Sportback",
                    Year = 2024,
                    Category = VehicleCategory.Sport,
                    PricePerDay = 90.00m,
                    Transmission = TransmissionType.Automatic,
                    FuelType = FuelType.Petrol,
                    Seats = 4,
                    LicensePlate = "BG-579-UV",
                    Status = VehicleStatus.Available,
                    Mileage = 2000,
                    Color = "Red",
                    Description = "Sporty and stylish coupe",
                    ImageUrl = "https://example.com/images/a5.jpg"
                },

                // Electric vehicle
                new Vehicle
                {
                    Brand = "Tesla",
                    Model = "Model 3",
                    Year = 2024,
                    Category = VehicleCategory.Luxury,
                    PricePerDay = 95.00m,
                    Transmission = TransmissionType.Automatic,
                    FuelType = FuelType.Electric,
                    Seats = 5,
                    LicensePlate = "BG-680-WX",
                    Status = VehicleStatus.Available,
                    Mileage = 1000,
                    Color = "White",
                    Description = "Premium electric sedan with autopilot",
                    ImageUrl = "https://example.com/images/tesla-model3.jpg"
                }
            };

            context.Vehicles.AddRange(vehicles);
            context.SaveChanges();
        }
    }
}
