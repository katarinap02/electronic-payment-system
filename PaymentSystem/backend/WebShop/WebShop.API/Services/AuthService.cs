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

        public AuthService(UserRepository userRepository, PasswordService passwordService, JwtService jwtService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        public AuthResponseDto Register(UserRegisterDto dto)
        {

            ValidateRegistration(dto);

            if (_userRepository.EmailExists(dto.Email))
                throw new Exception("Email već postoji.");

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

            return new AuthResponseDto
            {
                Token = token,
                User = MapToDto(createdUser)
            };
        }

        //lozinke pravim kao Zdravo123!
        public AuthResponseDto Login(UserLoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                throw new Exception("Email i lozinka su obavezni.");

            var user = _userRepository.GetByEmail(dto.Email.ToLower().Trim());
            if (user == null)
                throw new Exception("Pogrešan email.");

            if (!_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
                throw new Exception("Pogrešna lozinka.");

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToDto(user)
            };
        }

        public UserDto GetMyProfile(long userId)
        {
            var user = _userRepository.GetById(userId);
            if (user == null) throw new Exception("Korisnik nije pronađen.");
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
                errors.Add("Email je obavezan.");
            else
            {

                if (!IsValidEmail(dto.Email))
                    errors.Add("Nevalidan email format.");

            }

            
            if (string.IsNullOrWhiteSpace(dto.Password))
                errors.Add("Lozinka je obavezna.");
            else
            {
                
                if (dto.Password.Length < 8)
                    errors.Add("Lozinka mora imati najmanje 8 karaktera.");

                if (dto.Password.Length > 64)
                    errors.Add("Lozinka ne sme biti duža od 64 karaktera.");

                bool passwordErrors = _passwordService.IsPasswordStrong(dto.Password);
                if(!passwordErrors)
                { errors.Add("Lozinka nije dovoljno jaka"); }
                

            }

            if (string.IsNullOrWhiteSpace(dto.Name)) {
                errors.Add("Ime je obavezno.");
            }
                
            if (string.IsNullOrWhiteSpace(dto.Surname))
                errors.Add("Prezime je obavezno.");
            
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
            catch { return false; }
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


    }
}
