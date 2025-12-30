using WebShop.API.Models;
using WebShop.API.Services;

namespace WebShop.API.Data
{
    public static class UserSeedData
    {
        public static void SeedAdminUser(AppDbContext context, PasswordService passwordService)
        {
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
                context.SaveChanges();
            }
        }
    }
}
