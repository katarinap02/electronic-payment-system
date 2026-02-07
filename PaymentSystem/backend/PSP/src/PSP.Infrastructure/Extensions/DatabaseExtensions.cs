using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PSP.Infrastructure.Persistence;
using PSP.Infrastructure.Seed;

namespace PSP.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();
        var configuration = services.GetRequiredService<IConfiguration>();
        
        // Apply pending migrations
        await context.Database.MigrateAsync();
        
        // Seed data
        await SeedData.SeedDatabaseAsync(services, configuration);
    }
}
