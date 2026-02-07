using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PSP.Application.Interfaces.Services;
using PSP.Domain.Entities;
using PSP.Domain.Enums;
using PSP.Infrastructure.Persistence;

namespace PSP.Infrastructure.Seed;

public static class SeedData
{
    public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

        await SeedSuperAdminAsync(context, passwordService);
        await SeedAdminAsync(context, passwordService);
        await SeedPaymentMethodsAsync(context);
        await SeedWebShopsAsync(context, configuration);
        await SeedAdminWebshopsAsync(context);
    }

    private static async Task SeedSuperAdminAsync(AppDbContext context, IPasswordService passwordService)
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
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedAdminAsync(AppDbContext context, IPasswordService passwordService)
    {
        if (!context.Users.Any(u => u.Email == "admin@webshop.com"))
        {
            var admin = new User
            {
                Email = "admin@webshop.com",
                PasswordHash = passwordService.HashPassword("Admin123!"),
                Name = "WebShop",
                Surname = "Admin",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedPaymentMethodsAsync(AppDbContext context)
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
                },
            new PaymentMethod
            {
                Name = "PayPal",
                Code = "PAYPAL",  
                Type = PaymentMethodType.PayPal,  
                Description = "Plaćanje putem PayPal naloga",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
            };

            context.PaymentMethods.AddRange(paymentMethods);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedWebShopsAsync(AppDbContext context, IConfiguration configuration)
    {
        if (!context.WebShops.Any())
        {
            // Get WebShop URL from environment variable or fallback to localhost
            var webShopUrl = configuration["WebShopFrontendUrl"] ?? "https://localhost:5173";
            
            var webShop = new WebShop
            {
                Name = "Car Rental WebShop",
                Url = webShopUrl,
                ApiKey = "webshop-api-key-change-this",
                MerchantId = "WEBSHOP_001",
                Status = WebShopStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ErrorUrl = $"{webShopUrl}/payment-error",
                FailedUrl = $"{webShopUrl}/payment-failed",
                SuccessUrl = $"{webShopUrl}/payment-success"
            };

            context.WebShops.Add(webShop);
            await context.SaveChangesAsync();

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

            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedAdminWebshopsAsync(AppDbContext context)
    {
        var adminUser = context.Users.FirstOrDefault(u => u.Email == "admin@webshop.com");
        var webShop = context.WebShops.FirstOrDefault(w => w.MerchantId == "WEBSHOP_001");

        if (adminUser != null && webShop != null)
        {
            if (!context.WebShopAdmins.Any(wa => wa.UserId == adminUser.Id && wa.WebShopId == webShop.Id))
            {
                var webShopAdmin = new WebShopAdmin
                {
                    UserId = adminUser.Id,
                    WebShopId = webShop.Id,
                    AssignedAt = DateTime.UtcNow
                };

                context.WebShopAdmins.Add(webShopAdmin);
                await context.SaveChangesAsync();
            }
        }
    }
}
