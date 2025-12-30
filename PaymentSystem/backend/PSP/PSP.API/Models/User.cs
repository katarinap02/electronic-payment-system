namespace PSP.API.Models
{
    public enum UserRole
    {
        SuperAdmin,
        Admin,
        User
    }

    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
