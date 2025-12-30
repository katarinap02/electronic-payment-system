using PSP.Domain.Entities;

namespace PSP.Application.Interfaces.Repositories;

public interface IPaymentMethodRepository
{
    Task<PaymentMethod?> GetByIdAsync(int id);
    Task<IEnumerable<PaymentMethod>> GetAllAsync();
    Task<IEnumerable<PaymentMethod>> GetActiveAsync();
    Task<WebShopPaymentMethod?> GetWebShopPaymentMethodAsync(int webShopId, int paymentMethodId);
    Task<WebShopPaymentMethod> AddWebShopPaymentMethodAsync(WebShopPaymentMethod webShopPaymentMethod);
    Task<WebShopPaymentMethod> UpdateWebShopPaymentMethodAsync(WebShopPaymentMethod webShopPaymentMethod);
    Task RemoveWebShopPaymentMethodAsync(WebShopPaymentMethod webShopPaymentMethod);
}
