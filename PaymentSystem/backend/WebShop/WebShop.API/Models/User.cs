namespace WebShop.API.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public User() { }   

        public User(long id, string email, string passwordHash, string name, string surname)
        {
            Id = id;
            Email = email;
            PasswordHash = passwordHash;
            Name = name;
            Surname = surname;

        }
    }
}
