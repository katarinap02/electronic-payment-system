using Microsoft.VisualBasic;

namespace Bank.API.Models
{
    public class Card
    {
        public long Id { get; set; }
        public string CardHash { get; set; }
        public string MaskedPan { get; set; }
        public string LastFourDigits { get; set; }
        public string CardholderName { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public CardType Type { get; set; }
        public CardStatus Status { get; set; } = CardStatus.ACTIVE;
        public string CustomerId { get; set; }
        public Customer Customer { get; set; }
        //public string CvvSalt { get; set; }
        //public int PinAttempts { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow.AddHours(-1);
        public ICollection<CardToken> Tokens { get; set; } = new List<CardToken>();

        public enum CardType
        {
            DEBIT_VISA,
            DEBIT_MASTERCARD,
            CREDIT_VISA,
            CREDIT_MASTERCARD
        }

        public enum CardStatus
        {
            ACTIVE,
            BLOCKED,
            EXPIRED
        }
    }
}
