using Microsoft.EntityFrameworkCore;


namespace Bank.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) {}


    }
}
