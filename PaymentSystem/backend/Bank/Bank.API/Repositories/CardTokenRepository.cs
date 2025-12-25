using Bank.API.Data;
using Bank.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.API.Repositories
{
    public class CardTokenRepository
    {
        private readonly AppDbContext _context;

        public CardTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public CardToken CreateToken(long cardId, long transactionId)
        {
            var card = _context.Cards
                .FirstOrDefault(c => c.Id == cardId && c.Status == Card.CardStatus.ACTIVE);

            if (card == null)
                throw new ArgumentException("Card not found or inactive");

            var existingToken = _context.CardTokens
                .FirstOrDefault(t => t.TransactionId == transactionId &&
                                   t.DeletedAt == null &&
                                   t.ExpiresAt > DateTime.UtcNow);

            if (existingToken != null)
                return existingToken;

            var token = new CardToken
            {
                CardId = cardId,
                TransactionId = transactionId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            _context.CardTokens.Add(token);
            _context.SaveChanges();

            return token;
        }

        public CardToken? ValidateToken(string token, long transactionId)
        {
            var cardToken = _context.CardTokens
                .Include(t => t.Card)
                .FirstOrDefault(t => t.Token == token &&
                                   t.TransactionId == transactionId &&
                                   t.DeletedAt == null);

            if (cardToken == null)
                return null;

            if (cardToken.ExpiresAt < DateTime.UtcNow)
            {
                cardToken.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return null;
            }

            if (cardToken.IsUsed)
                return null;

            return cardToken;
        }

        public bool MarkTokenAsUsed(long tokenId)
        {
            var token = _context.CardTokens.Find(tokenId);
            if (token == null || token.IsUsed || token.DeletedAt != null)
                return false;

            token.IsUsed = true;
            token.ValidateAndDeleteCvv();

            _context.SaveChanges();
            return true;
        }

        public bool DeleteToken(long tokenId)
        {
            var token = _context.CardTokens.Find(tokenId);
            if (token == null)
                return false;

            token.DeletedAt = DateTime.UtcNow;
            token.ValidateAndDeleteCvv();

            _context.SaveChanges();
            return true;
        }

        public bool HasActiveTokenForTransaction(long transactionId)
        {
            return _context.CardTokens
                .Any(t => t.TransactionId == transactionId &&
                         t.DeletedAt == null &&
                         t.ExpiresAt > DateTime.UtcNow);
        }

        public int CleanupExpiredTokens()
        {
            var expiredTokens = _context.CardTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow && t.DeletedAt == null)
                .ToList();

            foreach (var token in expiredTokens)
            {
                token.DeletedAt = DateTime.UtcNow;
                token.ValidateAndDeleteCvv();
            }

            return _context.SaveChanges();
        }
    }
}
