using AuthService_SB.Application.Interfaces;
using AuthService_SB.Domain.Constants;
using AuthService_SB.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace AuthService_SB.Application.Services;

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public string GenerateToken(User user)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "AuthDotnet";
        var audience = jwtSettings["Audience"] ?? "AuthDotnet";
        var expiryInMinutes = int.Parse(jwtSettings["ExpiryInMinutes"] ?? "30");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Get user's role (assumes single role per user)
        var role = user.UserRoles?.FirstOrDefault()?.Role?.Name ?? RoleConstants.USER_ROLE;

        // Build full name
        var fullName = $"{user.Name} {user.Surname}".Trim();

        // Get phone from profile if exists
        var phone = user.UserProfile?.Phone ?? "";

        var claims = new[]
        {
            // Standard JWT claims
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            
            // Custom claims for application
            new Claim("role", role),
            new Claim("email", user.Email),
            new Claim("username", user.Username),
            new Claim("name", fullName),
            new Claim("phone", phone),
            new Claim("status", user.Status.ToString().ToLower()),
            
            // Datos específicos por sistema (banco)
            // TODO: Agregar accountNumber para CLIENT cuando sea necesario
            // new Claim("accountNumber", "account-number"),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
