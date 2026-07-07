using AuthService_SB.Application.Exceptions;
using AuthService_SB.Application.DTOs;
using AuthService_SB.Application.Interfaces;
using AuthService_SB.Application.Services;
using AuthService_SB.Domain.Entities;
using AuthService_SB.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AuthService_SB.Tests;

public class AuthServiceLoginTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRoleRepository> _roleRepository = new();
    private readonly Mock<IPasswordHashService> _passwordHashService = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<ICloudinaryService> _cloudinaryService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["JwtSettings:ExpiryInMinutes"] = "30" })
        .Build();

    private AuthService BuildSut() => new(
        _userRepository.Object,
        _roleRepository.Object,
        _passwordHashService.Object,
        _jwtTokenService.Object,
        _cloudinaryService.Object,
        _emailService.Object,
        _configuration,
        new Mock<ILogger<AuthService>>().Object);

    private static User BuildActiveUser(string password = "hashed-password") => new()
    {
        Id = "user-1",
        Name = "Ada",
        Surname = "Lovelace",
        Username = "ada",
        Email = "ada@example.com",
        Password = password,
        Status = true,
        UserProfile = new UserProfile { Id = "p1", UserId = "user-1" },
        UserRoles = []
    };

    [Fact]
    public async Task LoginAsync_ReturnsAToken_ForValidEmailAndPassword()
    {
        var user = BuildActiveUser();
        _userRepository.Setup(r => r.GetByEmailAsync("ada@example.com")).ReturnsAsync(user);
        _passwordHashService.Setup(p => p.VerifyPassword("correct-password", user.Password)).Returns(true);
        _jwtTokenService.Setup(j => j.GenerateToken(user)).Returns("signed.jwt.token");

        var sut = BuildSut();
        var result = await sut.LoginAsync(new LoginDto { EmailOrUsername = "ada@example.com", Password = "correct-password" });

        Assert.True(result.Success);
        Assert.Equal("signed.jwt.token", result.Token);
    }

    [Fact]
    public async Task LoginAsync_ReturnsAToken_ForValidUsernameAndPassword()
    {
        var user = BuildActiveUser();
        _userRepository.Setup(r => r.GetByUsernameAsync("ada")).ReturnsAsync(user);
        _passwordHashService.Setup(p => p.VerifyPassword("correct-password", user.Password)).Returns(true);
        _jwtTokenService.Setup(j => j.GenerateToken(user)).Returns("signed.jwt.token");

        var sut = BuildSut();
        var result = await sut.LoginAsync(new LoginDto { EmailOrUsername = "ada", Password = "correct-password" });

        Assert.True(result.Success);
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenUserDoesNotExist()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("missing@example.com")).ReturnsAsync((User?)null);
        var sut = BuildSut();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.LoginAsync(new LoginDto { EmailOrUsername = "missing@example.com", Password = "whatever" }));
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenPasswordIsWrong()
    {
        var user = BuildActiveUser();
        _userRepository.Setup(r => r.GetByEmailAsync("ada@example.com")).ReturnsAsync(user);
        _passwordHashService.Setup(p => p.VerifyPassword("wrong-password", user.Password)).Returns(false);
        var sut = BuildSut();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.LoginAsync(new LoginDto { EmailOrUsername = "ada@example.com", Password = "wrong-password" }));

        _jwtTokenService.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenUserHasNotVerifiedTheirEmail()
    {
        var user = BuildActiveUser();
        user.Status = false;
        _userRepository.Setup(r => r.GetByEmailAsync("ada@example.com")).ReturnsAsync(user);
        var sut = BuildSut();

        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            sut.LoginAsync(new LoginDto { EmailOrUsername = "ada@example.com", Password = "whatever" }));

        Assert.Equal("USER_NOT_VERIFIED", ex.ErrorCode);
        _passwordHashService.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
