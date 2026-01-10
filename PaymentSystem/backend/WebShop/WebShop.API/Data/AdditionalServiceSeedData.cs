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
                    IconUrl = "https://m.media-amazon.com/images/I/71DoQ1vZYKL._AC_UF894,1000_QL80_.jpg"
                },
                new AdditionalService
                {
                    Name = "Child Seat (0-4 years)",
                    Description = "Safety-certified rear-facing child seat for infants and toddlers. Complies with EU standards.",
                    PricePerDay = 8.00m,
                    IsAvailable = true,
                    IconUrl = "https://w7.pngwing.com/pngs/512/825/png-transparent-britax-romer-king-ii-ats-baby-toddler-car-seats-seat-belt-child-others-child-king-car-seat.png"
                },
                new AdditionalService
                {
                    Name = "Booster Seat (4-12 years)",
                    Description = "Forward-facing booster seat for children aged 4-12. Adjustable height.",
                    PricePerDay = 6.00m,
                    IsAvailable = true,
                    IconUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS1Y6eV0kO0dMSzJPZitM_iIvSn3nBeYKsjxA&s"
                },
                new AdditionalService
                {
                    Name = "Extra Driver",
                    Description = "Add an additional authorized driver to the rental agreement. Driver must present valid license.",
                    PricePerDay = 10.00m,
                    IsAvailable = true,
                    IconUrl = "https://bendovizasvadbu.com/wp-content/uploads/2020/08/darko-lazic.png"
                },
                new AdditionalService
                {
                    Name = "WiFi Hotspot",
                    Description = "Portable 4G WiFi router with unlimited data. Connect up to 5 devices simultaneously.",
                    PricePerDay = 10.00m,
                    IsAvailable = true,
                    IconUrl = "https://png.pngtree.com/png-vector/20251125/ourlarge/pngtree-compact-white-and-silver-mobile-wifi-router-with-4g-lte-connectivity-png-image_18024825.webp"
                },
                new AdditionalService
                {
                    Name = "Snow Chains",
                    Description = "Winter tire chains for safe driving on snowy and icy mountain roads. Required in some regions during winter.",
                    PricePerDay = 15.00m,
                    IsAvailable = true,
                    IconUrl = "https://p7.hiclipart.com/preview/339/319/631/tire-snow-chains-lawn-mowers-chain.jpg"
                },
                new AdditionalService
                {
                    Name = "Ski Rack",
                    Description = "Roof-mounted ski and snowboard carrier. Holds up to 4 pairs of skis or 2 snowboards.",
                    PricePerDay = 12.00m,
                    IsAvailable = true,
                    IconUrl = "https://e7.pngegg.com/pngimages/527/138/png-clipart-jeep-cherokee-xj-jeep-liberty-jeep-cj-jeep-wrangler-roof-rack-angle-car-thumbnail.png"
                },
                new AdditionalService
                {
                    Name = "Toll Pass",
                    Description = "Electronic toll payment device for highways. Automatically charges tolls, avoiding queues at toll booths.",
                    PricePerDay = 3.00m,
                    IsAvailable = true,
                    IconUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSJwZsMGH3VSvG9TCwrS_ZHLe821f10M79K2w&s"
                }
            };

            context.AdditionalServices.AddRange(services);
            context.SaveChanges();
        }
    }
}
