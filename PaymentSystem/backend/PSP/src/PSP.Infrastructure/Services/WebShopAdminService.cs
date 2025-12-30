using PSP.Application.Common;
using PSP.Application.DTOs.WebShops;
using PSP.Application.Extensions;
using PSP.Application.Interfaces.Repositories;
using PSP.Application.Interfaces.Services;
using PSP.Domain.Entities;
using PSP.Domain.Enums;

namespace PSP.Infrastructure.Services;

public class WebShopAdminService : IWebShopAdminService
{
    private readonly IWebShopRepository _webShopRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWebShopAdminRepository _webShopAdminRepository;

    public WebShopAdminService(
        IWebShopRepository webShopRepository,
        IUserRepository userRepository,
        IWebShopAdminRepository webShopAdminRepository)
    {
        _webShopRepository = webShopRepository;
        _userRepository = userRepository;
        _webShopAdminRepository = webShopAdminRepository;
    }

    public async Task<Result> AssignAdminToWebShopAsync(int userId, int webShopId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result.Failure("User not found");

        if (user.Role != UserRole.Admin)
            return Result.Failure("User must have Admin role");

        var webShop = await _webShopRepository.GetByIdAsync(webShopId);
        if (webShop == null)
            return Result.Failure("WebShop not found");

        var existing = await _webShopAdminRepository.GetByUserAndWebShopAsync(userId, webShopId);
        if (existing != null)
            return Result.Failure("User is already admin of this WebShop");

        var webShopAdmin = new WebShopAdmin
        {
            UserId = userId,
            WebShopId = webShopId,
            AssignedAt = DateTime.UtcNow
        };

        await _webShopAdminRepository.AddAsync(webShopAdmin);
        return Result.Success();
    }

    public async Task<Result> RemoveAdminFromWebShopAsync(int userId, int webShopId)
    {
        var webShopAdmin = await _webShopAdminRepository.GetByUserAndWebShopAsync(userId, webShopId);
        if (webShopAdmin == null)
            return Result.Failure("User is not admin of this WebShop");

        await _webShopAdminRepository.RemoveAsync(webShopAdmin);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<WebShopAdminResponse>>> GetWebShopAdminsAsync(int webShopId)
    {
        var admins = await _webShopAdminRepository.GetWebShopAdminsAsync(webShopId);
        var response = admins.Select(wa => wa.ToWebShopAdminResponse());
        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<WebShopResponse>>> GetUserManagedWebShopsAsync(int userId)
    {
        var webShopAdmins = await _webShopAdminRepository.GetUserManagedWebShopsAsync(userId);
        var response = webShopAdmins.Select(wa => wa.WebShop.ToWebShopResponse());
        return Result.Success(response);
    }

    public async Task<bool> IsUserAdminOfWebShopAsync(int userId, int webShopId)
    {
        return await _webShopAdminRepository.IsUserAdminOfWebShopAsync(userId, webShopId);
    }
}
