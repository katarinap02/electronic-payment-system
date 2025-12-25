using Bank.API.Data;
using Bank.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.API.Repositories
{
    public class CardRepository
    {
        private readonly AppDbContext _context;

        public CardRepository(AppDbContext context)
        {
            _context = context;
        }

        public Card? GetById(long cardId)
        {
            return _context.Cards
                .Include(c => c.Customer)
                .FirstOrDefault(c => c.Id == cardId);

        }
        public bool ValidateCardNumber(string pan)
        {
            if (string.IsNullOrWhiteSpace(pan) || pan.Length < 13 || pan.Length > 19)
                return false;

            int sum = 0;
            bool alternate = false;

            for (int i = pan.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(pan[i]))
                    return false;

                int digit = pan[i] - '0';

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

        public bool ValidateExpiryDate(string month, string year)
        {
            if (!int.TryParse(month, out int m) || !int.TryParse(year, out int y))
                return false;

            if (m < 1 || m > 12)
                return false;

            var currentDate = DateTime.UtcNow;
            var expiryDate = new DateTime(2000 + y, m, 1).AddMonths(1).AddDays(-1);

            return expiryDate >= currentDate;
        }

        public bool IsCardActive(long cardId)
        {
            var card = _context.Cards
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == cardId);

            return card != null &&
                   card.Status == Card.CardStatus.ACTIVE &&
                   card.PinAttempts < 3;
        }

        public string GenerateCvvHash(string cvv, string salt)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var cvvBytes = System.Text.Encoding.UTF8.GetBytes(cvv + salt);
            var hashBytes = sha256.ComputeHash(cvvBytes);
            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyCvv(string cvv, string salt, string storedHash)
        {
            var computedHash = GenerateCvvHash(cvv, salt);
            return computedHash == storedHash;

        }

        public string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return "****";

            return $"**** **** **** {cardNumber.Substring(cardNumber.Length - 4)}";
        }

        public Card? FindCardByHash(string cardHash)
        {
            return _context.Cards
                .Include(c => c.Customer)
                .FirstOrDefault(c => c.CardHash == cardHash &&
                                   c.Status == Card.CardStatus.ACTIVE);
        }
    }
}
