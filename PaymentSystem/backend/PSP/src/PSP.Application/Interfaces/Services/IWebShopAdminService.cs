using PSP.Application.Common;
using PSP.Application.DTOs.WebShops;

namespace PSP.Application.Interfaces.Services;

public interface IWebShopAdminService
{
    Task<Result> AssignAdminToWebShopAsync(int userId, int webShopId);
    Task<Result> RemoveAdminFromWebShopAsync(int userId, int webShopId);
    Task<Result<IEnumerable<WebShopAdminResponse>>> GetWebShopAdminsAsync(int webShopId);
    Task<Result<IEnumerable<WebShopResponse>>> GetUserManagedWebShopsAsync(int userId);
    Task<bool> IsUserAdminOfWebShopAsync(int userId, int webShopId);
}
