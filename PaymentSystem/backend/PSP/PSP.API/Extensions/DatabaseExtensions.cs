using Microsoft.EntityFrameworkCore;
using PSP.API.Data;
using PSP.API.Services.Interfaces;

namespace PSP.API.Extensions;

public static class DatabaseExtensions
{
    public static WebApplication ApplyMigrationsAndSeed(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();
            
            try
            {
                var dbContext = services.GetRequiredService<AppDbContext>();
                
                logger.LogInformation("Applying database migrations...");
                dbContext.Database.Migrate();
                logger.LogInformation("Database migrations applied successfully.");
                
                logger.LogInformation("Seeding database...");
                SeedData.SeedDatabase(dbContext, services.GetRequiredService<IPasswordService>());
                logger.LogInformation("Database seeded successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                throw;
            }
        }

        return app;
    }
}
