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
                    ImageUrl = "https://d62-a.sdn.cz/d_62/c_img_QQ_0/QsGi/volkswagen-polo.jpeg?fl=cro,46,203,1388,780%7Cres,1200,,1%7Cjpg,80,,1"
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
                    ImageUrl = "https://www.deusrentacar.rs/wp-content/uploads/2019/10/Fiat_Grande_Punto-e1590531719242.jpg"
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
                    ImageUrl = "https://dmotion.rs/wp-content/uploads/2019/02/vw-passat-facelift-2.jpg"
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
                    ImageUrl = "https://images.autohermes.rs/1583403927/desktop/news-large/2042OCTAVIA-RS-iV-01.4d92a57f08c56c3db884230feb379d16.fit-1450x760.jpg"
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
                    ImageUrl = "https://stimg.cardekho.com/images/carexteriorimages/930x620/Mercedes-Benz/E-Class/9790/1763471140336/front-left-side-47.jpg"
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
                    ImageUrl = "https://hips.hearstapps.com/hmg-prod/images/2024-bmw-530i-xdrive-118-65808a4c3d44a.jpg?crop=0.561xw:0.474xh;0.171xw,0.327xh&resize=1200:*"
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
                    ImageUrl = "https://www.automanijak.com/resources/images/variant/1580/rav4_5.jpg"
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
                    ImageUrl = "https://images.cdn.autocar.co.uk/sites/autocar.co.uk/files/styles/gallery_slide/public/nissan-x-trail-1.jpg?itok=c4I7u6rC"
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
                    ImageUrl = "https://www.automanijak.com/resources/images/variant/1195/v_1.jpg"
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
                    ImageUrl = "https://cdn.motor1.com/images/mgl/YAgXej/s1/vw-transporter-und-caravelle-sondermodell-edition-2025.jpg"
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
                    ImageUrl = "https://media.audiusa.com/assets/images/hero/11062-0K2A5308.jpg"
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
                    ImageUrl = "https://cdn.motor1.com/images/mgl/RqMxpA/s3/tesla-model-3-standard-2026.jpg"
                }
            };

            context.Vehicles.AddRange(vehicles);
            context.SaveChanges();
        }
    }
}
