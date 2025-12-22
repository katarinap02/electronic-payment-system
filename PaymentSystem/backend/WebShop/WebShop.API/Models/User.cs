namespace WebShop.API.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public UserRole Role { get; set; } 

        public User() { }   

        public User(long id, string email, string passwordHash, string name, string surname)
        {
            Id = id;
            Email = email;
            PasswordHash = passwordHash;
            Name = name;
            Surname = surname;
            Role = UserRole.Customer;

        }
    }

    public enum UserRole
    {
        Admin = 1,
        Customer = 2
        
    }
}
