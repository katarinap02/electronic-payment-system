using System.Transactions;

namespace Bank.API.Models
{
    public class PaymentTransaction
    {
        public long Id { get; set; }
        public string PaymentId { get; set; } //stavka 3
        public string MerchantId { get; set; } //tabela 2
        public DateTime MerchantTimestamp { get; set; } // da vidimo da nije istekao, tabela 1
        public string Stan { get; set; } //pracenje izmedju PSP-a i banke
        public string? GlobalTransactionId { get; set; } //bankin interni id
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public TransactionStatus Status { get; set; } = TransactionStatus.PENDING;
        public DateTime PspTimestamp { get; set; } //iz tabele 2, timestamp transakcije kod PSP-a
        public DateTime ExpiresAt { get; set; } //ogranici vremenski, stavka 4a

        //POKRIVA: "ACQUIRER_TIMESTAMP" (stavka 5), praćenje celokupnog toka
        public DateTime AcquirerTimestamp { get; set; } = DateTime.UtcNow;
        public DateTime? AuthorizedAt { get; set; }  // Kada je kartica autorizovana
        public DateTime? CapturedAt { get; set; }    // Kada su sredstva rezervisana
        public DateTime? FailedAt { get; set; }
        public long MerchantAccountId { get; set; }
        public long? CardTokenId { get; set; }
        public long? CustomerAccountId { get; set; }
        public BankAccount MerchantAccount { get; set; }
        public BankAccount CustomerAccount { get; set; }
        public CardToken CardToken { get; set; }
        public string? CustomerId { get; set; }     // Koji CUSTOMER
        public Customer? Customer { get; set; }
        public long? CardId { get; set; }           // Koja FIZIČKA kartica
        public Card? Card { get; set; }
        public enum TransactionStatus
        {
            PENDING,      // Kreirana, čeka unos kartice
            AUTHORIZED,   // Kartica validna, sredstva rezervisana
            CAPTURED,     // Sredstva prebačena merchantu
            FAILED,       // Greška
            EXPIRED,      // Isteklo vreme
            CANCELLED     // Korisnik otkazao
        }

    }
}
