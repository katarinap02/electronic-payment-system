using System.Security.Cryptography;
using System.Text;

namespace WebShop.API.Services
{
    public class PasswordService
    {
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();

            // Salt za dodatnu sigurnost
            var salt = Encoding.UTF8.GetBytes("webshop-salt-2024");
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            // Kombinacija salt i password-a
            var combinedBytes = new byte[salt.Length + passwordBytes.Length];
            Buffer.BlockCopy(salt, 0, combinedBytes, 0, salt.Length);
            Buffer.BlockCopy(passwordBytes, 0, combinedBytes, salt.Length, passwordBytes.Length);

            var hash = sha256.ComputeHash(combinedBytes);
            return Convert.ToBase64String(hash);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var newHash = HashPassword(password);
            return newHash == hashedPassword;
        }

        public bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            if (password.Length < 8) return false;

            // Mora imati bar jedan broj
            if (!password.Any(char.IsDigit)) return false;

            // Mora imati bar jedno veliko slovo
            if (!password.Any(char.IsUpper)) return false;

            // Mora imati bar jedno malo slovo
            if (!password.Any(char.IsLower)) return false;

            // Mora imati bar jedan specijalni karakter
            var specialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            if (!password.Any(c => specialCharacters.Contains(c))) return false;

            return true;
        }
    }
}
