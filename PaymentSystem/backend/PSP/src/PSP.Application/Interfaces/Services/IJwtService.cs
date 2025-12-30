using PSP.Domain.Entities;

namespace PSP.Application.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}
