using PSP.Application.Common;
using PSP.Application.DTOs.Auth;

namespace PSP.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
}
