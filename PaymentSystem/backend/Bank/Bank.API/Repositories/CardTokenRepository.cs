using Bank.API.Data;
using Bank.API.Models;

namespace Bank.API.Repositories
{
    public class CardTokenRepository
    {
        private readonly AppDbContext _context;

        public CardTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public CardToken GetByToken(string token)
        {
            return _context.CardTokens
                .FirstOrDefault(t => t.Token == token);
        }

        public CardToken GetByTransactionId(long transactionId)
        {
            return _context.CardTokens
                .FirstOrDefault(t => t.TransactionId == transactionId);
        }

        public CardToken Create(CardToken token)
        {
            _context.CardTokens.Add(token);
            _context.SaveChanges();
            return token;
        }

        public void Delete(long id)
        {
            var token = _context.CardTokens.Find(id);
            if (token != null)
            {
                token.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
            }
        }

        public bool TokenExists(string token)
        {
            return _context.CardTokens.Any(t => t.Token == token);
        }
    }
}
