using PSP.Application.Common;
using PSP.Application.DTOs.Auth;
using PSP.Application.Interfaces.Repositories;
using PSP.Application.Interfaces.Services;

namespace PSP.Infrastructure.Security;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        
        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponse>("Invalid email or password");
        }

        var token = _jwtService.GenerateToken(user);

        var response = new LoginResponse
        {
            Token = token,
            Email = user.Email,
            Name = $"{user.Name} {user.Surname}",
            Role = user.Role.ToString()
        };

        return Result.Success(response);
    }
}
