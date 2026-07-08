using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthService_SB.Application.Services;
using AuthService_SB.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace AuthService_SB.Tests;

public class JwtTokenServiceTests
{
    private static IConfiguration BuildConfiguration(string? secretKey = "unit-test-secret-key-needs-to-be-long-enough-256-bits")
    {
        var values = new Dictionary<string, string?>
        {
            ["JwtSettings:Issuer"] = "AuthServiceBancario",
            ["JwtSettings:Audience"] = "ApiBancaria",
            ["JwtSettings:ExpiryInMinutes"] = "30"
        };

        if (secretKey != null)
        {
            values["JwtSettings:SecretKey"] = secretKey;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static User BuildUser() => new()
    {
        Id = "user-1",
        Name = "Ada",
        Surname = "Lovelace",
        Username = "ada",
        Email = "ada@example.com",
        Password = "hashed",
        Status = true,
        UserProfile = new UserProfile { Id = "p1", UserId = "user-1", Phone = "12345678" },
        UserRoles =
        [
            new UserRole { Id = "ur1", UserId = "user-1", RoleId = "r1", Role = new Role { Id = "r1", Name = "ADMIN_ROLE" } }
        ]
    };

    [Fact]
    public void GenerateToken_ProducesATokenThatValidatesWithTheSameParametersProgramConfigures()
    {
        var configuration = BuildConfiguration();
        var sut = new JwtTokenService(configuration);
        var user = BuildUser();

        var token = sut.GenerateToken(user);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidAudience = configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]!)),
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

        // JwtSecurityTokenHandler remaps several short claim names ("sub",
        // "role", "email", ...) to long ClaimTypes URIs on the resulting
        // ClaimsPrincipal by default (MapInboundClaims) — this only affects
        // in-process claim lookups (and is actually required for
        // [Authorize(Roles = ...)] to work), it does NOT change what's
        // physically written in the JWT payload that other services (the
        // Node.js Admin API) decode independently. Read the raw, unmapped
        // claims straight off the token to assert what's actually encoded.
        var rawClaims = ((JwtSecurityToken)validatedToken).Claims.ToList();

        Assert.NotNull(validatedToken);
        Assert.Equal("user-1", rawClaims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("ADMIN_ROLE", rawClaims.First(c => c.Type == "role").Value);
        Assert.Equal("ada@example.com", rawClaims.First(c => c.Type == "email").Value);
    }

    [Fact]
    public void GenerateToken_ExpiresApproximatelyAfterConfiguredMinutes()
    {
        var sut = new JwtTokenService(BuildConfiguration());
        var token = sut.GenerateToken(BuildUser());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var expectedExpiry = DateTime.UtcNow.AddMinutes(30);
        Assert.True(Math.Abs((jwt.ValidTo - expectedExpiry).TotalSeconds) < 30);
    }

    [Fact]
    public void GenerateToken_Throws_WhenSecretKeyIsNotConfigured()
    {
        var sut = new JwtTokenService(BuildConfiguration(secretKey: null));

        Assert.Throws<InvalidOperationException>(() => sut.GenerateToken(BuildUser()));
    }
}
