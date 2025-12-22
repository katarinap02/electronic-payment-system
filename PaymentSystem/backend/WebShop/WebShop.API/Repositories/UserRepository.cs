using WebShop.API.Data;
using WebShop.API.Models;

namespace WebShop.API.Repositories
{
    public class UserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }

        public List<User> GetByRole(UserRole role)
        {
            return _context.Users
                .Where(u => u.Role == role)
                .OrderBy(u => u.Surname)
                .ThenBy(u => u.Name)
                .ToList();
        }

        public User? GetById(long id)
        {
            return _context.Users.Find(id);
        }
        public User? GetByEmail(string email)
        {
            return _context.Users
                .FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        }

        public bool EmailExists(string email)
        {
            return _context.Users
                .Any(u => u.Email.ToLower() == email.ToLower());
        }

        public User Create(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

    }
}
