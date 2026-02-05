using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PSP.Application.Common;
using PSP.Application.DTOs.Auth;
using PSP.Application.Interfaces.Repositories;
using PSP.Application.Interfaces.Services;
using Serilog.Core;

namespace PSP.Infrastructure.Security;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        ILogger<AuthService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var correlationId = GetCorrelationId();
        var ipAddress = GetClientIp();
        var startTime = DateTime.UtcNow;

        // Maskiraj email za logove (samo domen je vidljiv)
        var maskedEmail = MaskEmail(request.Email);
        var user = await _userRepository.GetByEmailAsync(request.Email);

        _logger.LogInformation("[PSP-AUTH] ATTEMPT | Desc: Login initiated | Email: {MaskedEmail} | CorrId: {CorrId} | IP: {IP}",
            maskedEmail, correlationId, ipAddress);

        if (user == null)
        {
            _logger.LogWarning("[PSP-AUTH] FAILED | Desc: User not found | Email: {MaskedEmail} | FailReason: USER_NOT_FOUND | CorrId: {CorrId} | IP: {IP}",
                maskedEmail, correlationId, ipAddress);

            return Result.Failure<LoginResponse>("Invalid email or password");
        }

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("[PSP-AUTH] FAILED | Desc: Invalid password | UserId: {UserId} | Email: {MaskedEmail} | FailReason: INVALID_PASSWORD | CorrId: {CorrId} | IP: {IP}",
                user.Id, maskedEmail, correlationId, ipAddress);

            return Result.Failure<LoginResponse>("Invalid email or password");
        }

        var token = _jwtService.GenerateToken(user);
        var duration = DateTime.UtcNow - startTime;

        _logger.LogInformation("[PSP-AUTH] SUCCESS | Desc: Login completed, token issued | UserId: {UserId} | Email: {MaskedEmail} | Role: {Role} | DurationMs: {DurationMs} | CorrId: {CorrId} | IP: {IP}",
            user.Id, maskedEmail, user.Role, duration.TotalMilliseconds, correlationId, ipAddress);


        var response = new LoginResponse
        {
            Token = token,
            Email = user.Email,
            Name = $"{user.Name} {user.Surname}",
            Role = user.Role.ToString()
        };

        return Result.Success(response);
    }
    private string GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N")[..12];
    }

    private string GetClientIp()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return "internal";

        var forwarded = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded)) return forwarded.Split(',')[0].Trim();

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@')) return "invalid";
        var parts = email.Split('@');
        var local = parts[0];
        var domain = parts[1];

        var maskedLocal = local.Length > 2
            ? $"{local[..2]}***"
            : "***";

        return $"{maskedLocal}@{domain}";
    }
}
