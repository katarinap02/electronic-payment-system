using PSP.API.Common;
using PSP.API.DTOs;

namespace PSP.API.Services.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
}
