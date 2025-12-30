using Microsoft.EntityFrameworkCore;
using PSP.Application.Interfaces.Repositories;
using PSP.Domain.Entities;
using PSP.Infrastructure.Persistence;

namespace PSP.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.WebShop)
                .ThenInclude(w => w.WebShopPaymentMethods)
                    .ThenInclude(wpm => wpm.PaymentMethod)
            .Include(p => p.PaymentMethod)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetByMerchantOrderIdAsync(int webShopId, string merchantOrderId)
    {
        return await _context.Payments
            .Include(p => p.WebShop)
            .Include(p => p.PaymentMethod)
            .FirstOrDefaultAsync(p => p.WebShopId == webShopId && p.MerchantOrderId == merchantOrderId);
    }

    public async Task<IEnumerable<Payment>> GetByWebShopIdAsync(int webShopId)
    {
        return await _context.Payments
            .Include(p => p.PaymentMethod)
            .Where(p => p.WebShopId == webShopId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }
}
