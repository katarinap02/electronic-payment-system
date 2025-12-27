namespace Bank.API.Models
{
    public class Customer
    {
        public string Id { get; set; } = "CUST_" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        public string FullName { get; set; }
        public string EmailHash { get; set; }
        public string PhoneHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(-1);
        public CustomerStatus Status { get; set; } = CustomerStatus.ACTIVE;
        public string? WebShopUserIdHash { get; set; }
        public ICollection<BankAccount> Accounts { get; set; } = new List<BankAccount>();
        public ICollection<Card> Cards { get; set; } = new List<Card>();

        public enum CustomerStatus
        {
            ACTIVE = 1,             
            SUSPENDED = 2,           
            CLOSED = 3
        }



    }
}
