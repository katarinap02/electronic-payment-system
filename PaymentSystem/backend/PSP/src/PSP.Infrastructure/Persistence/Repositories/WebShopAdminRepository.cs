using Microsoft.EntityFrameworkCore;
using PSP.Application.Interfaces.Repositories;
using PSP.Domain.Entities;
using PSP.Infrastructure.Persistence;

namespace PSP.Infrastructure.Persistence.Repositories;

public class WebShopAdminRepository : IWebShopAdminRepository
{
    private readonly AppDbContext _context;

    public WebShopAdminRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WebShopAdmin?> GetByUserAndWebShopAsync(int userId, int webShopId)
    {
        return await _context.WebShopAdmins
            .FirstOrDefaultAsync(wa => wa.UserId == userId && wa.WebShopId == webShopId);
    }

    public async Task<IEnumerable<WebShopAdmin>> GetWebShopAdminsAsync(int webShopId)
    {
        return await _context.WebShopAdmins
            .Include(wa => wa.User)
            .Where(wa => wa.WebShopId == webShopId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WebShopAdmin>> GetUserManagedWebShopsAsync(int userId)
    {
        return await _context.WebShopAdmins
            .Include(wa => wa.WebShop)
                .ThenInclude(w => w.WebShopPaymentMethods)
                    .ThenInclude(wp => wp.PaymentMethod)
            .Where(wa => wa.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> IsUserAdminOfWebShopAsync(int userId, int webShopId)
    {
        return await _context.WebShopAdmins
            .AnyAsync(wa => wa.UserId == userId && wa.WebShopId == webShopId);
    }

    public async Task AddAsync(WebShopAdmin webShopAdmin)
    {
        _context.WebShopAdmins.Add(webShopAdmin);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(WebShopAdmin webShopAdmin)
    {
        _context.WebShopAdmins.Remove(webShopAdmin);
        await _context.SaveChangesAsync();
    }
}
