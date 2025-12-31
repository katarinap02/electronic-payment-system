using WebShop.API.Models;
using WebShop.API.Services;

namespace WebShop.API.Data
{
    public static class UserSeedData
    {
        public static void SeedAdminUser(AppDbContext context, PasswordService passwordService)
        {
            // Seed Admin user
            if (!context.Users.Any(u => u.Email == "admin@webshop.com"))
            {
                var adminUser = new User
                {
                    Email = "admin@webshop.com",
                    PasswordHash = passwordService.HashPassword("Admin123!"),
                    Name = "Admin",
                    Surname = "WebShop",
                    Role = UserRole.Admin
                };

                context.Users.Add(adminUser);
            }

            // Seed test customer user - Petar Petrović (matching Bank seed data)
            if (!context.Users.Any(u => u.Email == "petar.petrovic@example.com"))
            {
                var customerUser = new User
                {
                    Email = "petar.petrovic@example.com",
                    PasswordHash = passwordService.HashPassword("Petar123!"),
                    Name = "Petar",
                    Surname = "Petrović",
                    Role = UserRole.Customer
                };

                context.Users.Add(customerUser);
            }

            context.SaveChanges();
        }
    }
}
