using PSP.Domain.Entities;

namespace PSP.Application.Interfaces.Repositories;

public interface IWebShopAdminRepository
{
    Task<WebShopAdmin?> GetByUserAndWebShopAsync(int userId, int webShopId);
    Task<IEnumerable<WebShopAdmin>> GetWebShopAdminsAsync(int webShopId);
    Task<IEnumerable<WebShopAdmin>> GetUserManagedWebShopsAsync(int userId);
    Task<bool> IsUserAdminOfWebShopAsync(int userId, int webShopId);
    Task AddAsync(WebShopAdmin webShopAdmin);
    Task RemoveAsync(WebShopAdmin webShopAdmin);
}
