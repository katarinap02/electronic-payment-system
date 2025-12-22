using Microsoft.EntityFrameworkCore;
using WebShop.API.Models;

namespace WebShop.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) {}

        public DbSet<User> Users { get; set; }
    }
}
