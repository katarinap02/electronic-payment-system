using WebShop.API.Models;

namespace WebShop.API.Data
{
    public static class AdditionalServiceSeedData
    {
        public static void SeedAdditionalServices(AppDbContext context)
        {
            if (context.AdditionalServices.Any())
                return; // Already seeded

            var services = new List<AdditionalService>
            {
                new AdditionalService
                {
                    Name = "GPS Navigation",
                    Description = "Modern Garmin GPS device with updated European maps, speed camera alerts, and traffic updates.",
                    PricePerDay = 5.00m,
                    IsAvailable = true,
                    IconUrl = "https://example.com/icons/gps.svg"
                },
                new AdditionalService
                {
                    Name = "Child Seat (0-4 years)",
                    Description = "Safety-certified rear-facing child seat for infants and toddlers. Complies with EU standards.",
                    PricePerDay = 8.00m,
                    IsAvailable = true,
                    IconUrl = "https://example.com/icons/child-seat.svg"
                },
                new AdditionalService
                {
                    Name = "Booster Seat (4-12 years)",
                    Description = "Forward-facing booster seat for children aged 4-12. Adjustable height.",
                    PricePerDay = 6.00m,
                    IsAvailable = true,
                    IconUrl = "https://example.com/icons/booster-seat.svg"
                },
                new AdditionalService
                {
                    Name = "Extra Driver",
                    Description = "Add an additional authorized driver to the rental agreement. Driver must present valid license.",
                    PricePerDay = 10.00m,
                    IsAvailable = true,
                    IconUrl = "https://example.com/icons/driver.svg"
                },
                new AdditionalService
                {
                    Name = "WiFi Hotspot",
                    Description = "Portable 4G WiFi router with unlimited data. Connect up to 5 devices simultaneously.",
                    PricePerDay = 10.00m,
                    IsAvailable = true,
                    IconUrl = "https://example.com/icons/wifi.svg"
                },
                new AdditionalService
                {
                    Name = "Snow Chains",
                    Description = "Winter tire chains for safe driving on snowy and icy mountain roads. Required in some regions during winter.",
                    PricePerDay = 15.00m,
                    IsAvailable = true,
                    IconUrl = "https://example.com/icons/snow-chains.svg"
                },
                new AdditionalService
                {
                    Name = "Ski Rack",
                    Description = "Roof-mounted ski and snowboard carrier. Holds up to 4 pairs of skis or 2 snowboards.",
                    PricePerDay = 12.00m,
                    IsAvailable = true,
                    IconUrl = "https://example.com/icons/ski-rack.svg"
                },
                new AdditionalService
                {
                    Name = "Toll Pass",
                    Description = "Electronic toll payment device for highways. Automatically charges tolls, avoiding queues at toll booths.",
                    PricePerDay = 3.00m,
                    IsAvailable = true,
                    IconUrl = "https://example.com/icons/toll.svg"
                }
            };

            context.AdditionalServices.AddRange(services);
            context.SaveChanges();
        }
    }
}
