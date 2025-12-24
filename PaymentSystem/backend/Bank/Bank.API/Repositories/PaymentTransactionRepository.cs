using Bank.API.Data;
using Bank.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.API.Repositories
{
    public class PaymentTransactionRepository
    {
        private readonly AppDbContext _context;

        public PaymentTransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public PaymentTransaction GetByPaymentId(string paymentId)
        {
            return _context.PaymentTransactions
                .Include(t => t.MerchantAccount)
                .FirstOrDefault(t => t.PaymentId == paymentId);
        }

        public PaymentTransaction GetByStan(string stan)
        {
            return _context.PaymentTransactions
                .FirstOrDefault(t => t.Stan == stan);
        }

        public PaymentTransaction Create(PaymentTransaction transaction)
        {
            _context.PaymentTransactions.Add(transaction);
            _context.SaveChanges();
            return transaction;
        }

        public void Update(PaymentTransaction transaction)
        {
            _context.PaymentTransactions.Update(transaction);
            _context.SaveChanges();
        }

        public bool PaymentIdExists(string paymentId)
        {
            return _context.PaymentTransactions.Any(t => t.PaymentId == paymentId);
        }

        public bool StanExists(string stan)
        {
            return _context.PaymentTransactions.Any(t => t.Stan == stan);
        }
    }
}
