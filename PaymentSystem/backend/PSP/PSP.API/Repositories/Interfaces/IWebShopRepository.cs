using PSP.API.Models;

namespace PSP.API.Repositories.Interfaces;

public interface IWebShopRepository
{
    Task<WebShop?> GetByIdAsync(int id);
    Task<WebShop?> GetByMerchantIdAsync(string merchantId);
    Task<IEnumerable<WebShop>> GetAllAsync();
    Task<WebShop> CreateAsync(WebShop webShop);
    Task<WebShop> UpdateAsync(WebShop webShop);
    Task<bool> DeleteAsync(int id);
}
