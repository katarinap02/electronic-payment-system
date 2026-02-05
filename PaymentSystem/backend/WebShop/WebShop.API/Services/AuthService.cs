using System.Collections.Generic;
using WebShop.API.DTOs;
using WebShop.API.Models;
using WebShop.API.Repositories;

namespace WebShop.API.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly PasswordService _passwordService;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(UserRepository userRepository, PasswordService passwordService, JwtService jwtService, ILogger<AuthService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public AuthResponseDto Register(UserRegisterDto dto)
        {
            var correlationId = GetCorrelationId();
            var ipAddress = GetClientIp();
            var startTime = DateTime.UtcNow;
            var maskedEmail = MaskEmail(dto.Email);
            _logger.LogInformation("[WEBSHOP-AUTH] REGISTER_ATTEMPT | Desc: User registration started | Email: {MaskedEmail} | Name: {Name} | CorrId: {CorrId} | IP: {IP}",
                maskedEmail, dto.Name, correlationId, ipAddress);
            ValidateRegistration(dto);

            if (_userRepository.EmailExists(dto.Email))
            {
                _logger.LogWarning("[WEBSHOP-AUTH] REGISTER_FAILED | Desc: Duplicate email | Email: {MaskedEmail} | FailReason: EMAIL_EXISTS | CorrId: {CorrId} | IP: {IP}",
                    maskedEmail, correlationId, ipAddress);
                throw new Exception("Email already exists.");
            }

            var user = new User
            {
                Email = dto.Email.ToLower().Trim(),
                PasswordHash = _passwordService.HashPassword(dto.Password),
                Name = dto.Name?.Trim() ?? "",
                Surname = dto.Surname?.Trim() ?? "",
                Role = UserRole.Customer
            };

            var createdUser = _userRepository.Create(user);
            var token = _jwtService.GenerateToken(createdUser);
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("[WEBSHOP-AUTH] REGISTER_SUCCESS | Desc: User registered successfully | UserId: {UserId} | Email: {MaskedEmail} | Role: {Role} | DurationMs: {DurationMs} | CorrId: {CorrId} | IP: {IP}",
                createdUser.Id, maskedEmail, createdUser.Role, duration.TotalMilliseconds, correlationId, ipAddress);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToDto(createdUser)
            };
        }

        public AuthResponseDto Login(UserLoginDto dto)
        {
            var correlationId = GetCorrelationId();
            var ipAddress = GetClientIp();
            var startTime = DateTime.UtcNow;
            var maskedEmail = MaskEmail(dto.Email);

            _logger.LogInformation("[WEBSHOP-AUTH] LOGIN_ATTEMPT | Desc: User login started | Email: {MaskedEmail} | CorrId: {CorrId} | IP: {IP}",
                maskedEmail, correlationId, ipAddress);
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("[WEBSHOP-AUTH] LOGIN_FAILED | Desc: Missing credentials | Email: {MaskedEmail} | FailReason: EMPTY_CREDENTIALS | CorrId: {CorrId} | IP: {IP}",
                    maskedEmail, correlationId, ipAddress);
                throw new Exception("Email and password are mandatory.");
            }

            var user = _userRepository.GetByEmail(dto.Email.ToLower().Trim());
            if (user == null)
            {
                _logger.LogWarning("[WEBSHOP-AUTH] LOGIN_FAILED | Desc: User not found | Email: {MaskedEmail} | FailReason: USER_NOT_FOUND | CorrId: {CorrId} | IP: {IP}",
                    maskedEmail, correlationId, ipAddress);
                throw new Exception("Invalid email or password.");
            }

            if (!_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("[WEBSHOP-AUTH] LOGIN_FAILED | Desc: Invalid password | UserId: {UserId} | Email: {MaskedEmail} | FailReason: INVALID_PASSWORD | CorrId: {CorrId} | IP: {IP}",
                    user.Id, maskedEmail, correlationId, ipAddress);
                throw new Exception("Invalid email or password.");
            }

            var token = _jwtService.GenerateToken(user);
            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation("[WEBSHOP-AUTH] LOGIN_SUCCESS | Desc: User authenticated | UserId: {UserId} | Email: {MaskedEmail} | Role: {Role} | DurationMs: {DurationMs} | CorrId: {CorrId} | IP: {IP}",
                user.Id, maskedEmail, user.Role, duration.TotalMilliseconds, correlationId, ipAddress);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToDto(user)
            };
        }

        public UserDto GetMyProfile(long userId)
        {
            var user = _userRepository.GetById(userId);
            if (user == null)
                throw new Exception("User not found.");

            return MapToDto(user);
        }

        public List<UserDto> GetAllUsers()
        {
            var users = _userRepository.GetAll();
            return users.Select(MapToDto).ToList();
        }

        private void ValidateRegistration(UserRegisterDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Email))
                errors.Add("Email is required.");
            else if (!IsValidEmail(dto.Email))
                errors.Add("Invalid email format.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                errors.Add("Password is required.");
            else
            {
                if (dto.Password.Length < 8)
                    errors.Add("Password must be at least 8 characters long.");

                if (dto.Password.Length > 64)
                    errors.Add("Password cannot be longer than 64 characters.");

                if (!_passwordService.IsPasswordStrong(dto.Password))
                    errors.Add("Password is not strong enough. Use uppercase, lowercase, numbers and special characters.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
                errors.Add("First name is required.");

            if (string.IsNullOrWhiteSpace(dto.Surname))
                errors.Add("Last name is required.");

            if (errors.Any())
                throw new Exception(string.Join(" ", errors));
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                Role = user.Role.ToString()
            };
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

}