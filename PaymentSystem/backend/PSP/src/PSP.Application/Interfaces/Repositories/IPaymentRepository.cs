using PSP.Domain.Entities;

namespace PSP.Application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);
    Task<Payment?> GetByMerchantOrderIdAsync(int webShopId, string merchantOrderId);
    Task<IEnumerable<Payment>> GetByWebShopIdAsync(int webShopId);
    Task<Payment> CreateAsync(Payment payment);
    Task UpdateAsync(Payment payment);
}
