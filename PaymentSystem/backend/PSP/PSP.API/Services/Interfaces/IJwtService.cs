using PSP.API.Models;

namespace PSP.API.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
