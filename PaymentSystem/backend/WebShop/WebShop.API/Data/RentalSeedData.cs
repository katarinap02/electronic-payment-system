using WebShop.API.Data;
using WebShop.API.Models;

namespace WebShop.API.Data
{
    public static class RentalSeedData
    {
        public static void SeedRentals(AppDbContext context)
        {
            if (context.Rentals.Any())
                return; // Already seeded

            // Proveri da li postoje korisnici i vozila
            var adminUser = context.Users.FirstOrDefault(u => u.Email == "admin@webshop.com");
            var vehicle = context.Vehicles.FirstOrDefault();

            if (adminUser == null || vehicle == null)
            {
                Console.WriteLine("Cannot seed rentals - admin user or vehicles not found.");
                return;
            }

            var rental = new Rental
            {
                UserId = adminUser.Id,
                VehicleId = vehicle.Id,
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow.AddDays(-4),
                RentalDays = 3,
                AdditionalServices = "GPS Navigation, Child Seat (0-4 years)",
                AdditionalServicesPrice = 18.00m,
                InsuranceType = "Full Coverage",
                InsurancePrice = 60.00m, // 20 EUR/day * 3 days
                VehiclePricePerDay = vehicle.PricePerDay,
                TotalPrice = (vehicle.PricePerDay * 3) + 18.00m + 60.00m,
                PaymentId = "PAY_SEED_001",
                GlobalTransactionId = "GTX_SEED_001",
                Currency = "EUR",
                PaymentMethod = "CreditCard",
                Status = "Completed",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                CompletedAt = DateTime.UtcNow.AddDays(-4),
                Notes = "Test rental - seeded data for demonstration"
            };

            context.Rentals.Add(rental);
            context.SaveChanges();

            Console.WriteLine($"Rental seeded successfully! UserId: {adminUser.Id}, VehicleId: {vehicle.Id}, Total: {rental.TotalPrice} EUR");
        }
    }
}
