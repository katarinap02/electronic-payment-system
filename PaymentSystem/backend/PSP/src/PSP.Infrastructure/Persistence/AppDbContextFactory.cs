using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PSP.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // Use a default connection string for migrations
        // This will be overridden at runtime by appsettings.json
        optionsBuilder.UseNpgsql("Host=localhost;Database=psp;Username=postgres;Password=super;Port=5436");

        return new AppDbContext(optionsBuilder.Options);
    }
}
