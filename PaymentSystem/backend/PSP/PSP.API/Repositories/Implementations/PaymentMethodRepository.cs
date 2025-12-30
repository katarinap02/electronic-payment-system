using Microsoft.EntityFrameworkCore;
using PSP.API.Data;
using PSP.API.Models;
using PSP.API.Repositories.Interfaces;

namespace PSP.API.Repositories.Implementations;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly AppDbContext _context;

    public PaymentMethodRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentMethod?> GetByIdAsync(int id)
    {
        return await _context.PaymentMethods.FindAsync(id);
    }

    public async Task<IEnumerable<PaymentMethod>> GetAllAsync()
    {
        return await _context.PaymentMethods.ToListAsync();
    }

    public async Task<IEnumerable<PaymentMethod>> GetActiveAsync()
    {
        return await _context.PaymentMethods
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<WebShopPaymentMethod?> GetWebShopPaymentMethodAsync(int webShopId, int paymentMethodId)
    {
        return await _context.WebShopPaymentMethods
            .FirstOrDefaultAsync(wp => wp.WebShopId == webShopId && wp.PaymentMethodId == paymentMethodId);
    }

    public async Task<WebShopPaymentMethod> AddWebShopPaymentMethodAsync(WebShopPaymentMethod webShopPaymentMethod)
    {
        _context.WebShopPaymentMethods.Add(webShopPaymentMethod);
        await _context.SaveChangesAsync();
        return webShopPaymentMethod;
    }

    public async Task<WebShopPaymentMethod> UpdateWebShopPaymentMethodAsync(WebShopPaymentMethod webShopPaymentMethod)
    {
        _context.WebShopPaymentMethods.Update(webShopPaymentMethod);
        await _context.SaveChangesAsync();
        return webShopPaymentMethod;
    }

    public async Task RemoveWebShopPaymentMethodAsync(WebShopPaymentMethod webShopPaymentMethod)
    {
        _context.WebShopPaymentMethods.Remove(webShopPaymentMethod);
        await _context.SaveChangesAsync();
    }
}
