
using AuthService_SB.Domain.Entities;

namespace AuthService_SB.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}