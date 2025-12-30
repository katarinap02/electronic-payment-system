using PSP.API.Models;
using PSP.API.Services.Interfaces;

namespace PSP.API.Data
{
    public static class SeedData
    {
        public static void SeedDatabase(AppDbContext context, IPasswordService passwordService)
        {
            SeedSuperAdmin(context, passwordService);
            SeedPaymentMethods(context);
            SeedWebShops(context);
        }

        private static void SeedSuperAdmin(AppDbContext context, IPasswordService passwordService)
        {
            if (!context.Users.Any(u => u.Email == "admin@psp.com"))
            {
                var superAdmin = new User
                {
                    Email = "admin@psp.com",
                    PasswordHash = passwordService.HashPassword("Admin123!"),
                    Name = "Super",
                    Surname = "Admin",
                    Role = UserRole.SuperAdmin,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(superAdmin);
                context.SaveChanges();
            }
        }

        private static void SeedPaymentMethods(AppDbContext context)
        {
            if (!context.PaymentMethods.Any())
            {
                var paymentMethods = new List<PaymentMethod>
                {
                    new PaymentMethod
                    {
                        Name = "Kreditna Kartica",
                        Code = "CREDIT_CARD",
                        Type = PaymentMethodType.CreditCard,
                        Description = "Plaćanje kreditnom ili debitnom karticom",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new PaymentMethod
                    {
                        Name = "IPS Skeniraj",
                        Code = "IPS_SCAN",
                        Type = PaymentMethodType.IPSScan,
                        Description = "Plaćanje skeniranjem IPS QR koda",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.PaymentMethods.AddRange(paymentMethods);
                context.SaveChanges();
            }
        }

        private static void SeedWebShops(AppDbContext context)
        {
            if (!context.WebShops.Any())
            {
                var webShop = new WebShop
                {
                    Name = "Car Rental WebShop",
                    Url = "http://localhost:5173",
                    ApiKey = Guid.NewGuid().ToString(),
                    MerchantId = "WEBSHOP_001",
                    Status = WebShopStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                context.WebShops.Add(webShop);
                context.SaveChanges();

                // Subscribe webshop to all payment methods by default
                var paymentMethods = context.PaymentMethods.ToList();
                foreach (var paymentMethod in paymentMethods)
                {
                    context.WebShopPaymentMethods.Add(new WebShopPaymentMethod
                    {
                        WebShopId = webShop.Id,
                        PaymentMethodId = paymentMethod.Id,
                        IsEnabled = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                context.SaveChanges();
            }
        }
    }
}
