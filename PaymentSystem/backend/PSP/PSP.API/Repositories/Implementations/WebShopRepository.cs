using Microsoft.EntityFrameworkCore;
using PSP.API.Data;
using PSP.API.Models;
using PSP.API.Repositories.Interfaces;

namespace PSP.API.Repositories.Implementations;

public class WebShopRepository : IWebShopRepository
{
    private readonly AppDbContext _context;

    public WebShopRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WebShop?> GetByIdAsync(int id)
    {
        return await _context.WebShops
            .Include(w => w.WebShopPaymentMethods)
                .ThenInclude(wp => wp.PaymentMethod)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<WebShop?> GetByMerchantIdAsync(string merchantId)
    {
        return await _context.WebShops
            .Include(w => w.WebShopPaymentMethods)
                .ThenInclude(wp => wp.PaymentMethod)
            .FirstOrDefaultAsync(w => w.MerchantId == merchantId);
    }

    public async Task<IEnumerable<WebShop>> GetAllAsync()
    {
        return await _context.WebShops
            .Include(w => w.WebShopPaymentMethods)
                .ThenInclude(wp => wp.PaymentMethod)
            .ToListAsync();
    }

    public async Task<WebShop> CreateAsync(WebShop webShop)
    {
        _context.WebShops.Add(webShop);
        await _context.SaveChangesAsync();
        return webShop;
    }

    public async Task<WebShop> UpdateAsync(WebShop webShop)
    {
        webShop.UpdatedAt = DateTime.UtcNow;
        _context.WebShops.Update(webShop);
        await _context.SaveChangesAsync();
        return webShop;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var webShop = await _context.WebShops.FindAsync(id);
        if (webShop == null) return false;

        _context.WebShops.Remove(webShop);
        await _context.SaveChangesAsync();
        return true;
    }
}
