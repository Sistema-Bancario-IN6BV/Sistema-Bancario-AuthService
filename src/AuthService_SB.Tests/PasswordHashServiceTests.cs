using AuthService_SB.Application.Services;
using Xunit;

namespace AuthService_SB.Tests;

public class PasswordHashServiceTests
{
    private readonly PasswordHashService _sut = new();

    [Fact]
    public void VerifyPassword_ReturnsTrue_ForTheCorrectPassword()
    {
        var hash = _sut.HashPassword("Correct-Horse-Battery-Staple1");

        Assert.True(_sut.VerifyPassword("Correct-Horse-Battery-Staple1", hash));
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForAnIncorrectPassword()
    {
        var hash = _sut.HashPassword("Correct-Horse-Battery-Staple1");

        Assert.False(_sut.VerifyPassword("wrong-password", hash));
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForACorruptedHash()
    {
        var hash = _sut.HashPassword("Correct-Horse-Battery-Staple1");
        var corrupted = hash[..^4] + "abcd";

        Assert.False(_sut.VerifyPassword("Correct-Horse-Battery-Staple1", corrupted));
    }

    [Fact]
    public void HashPassword_ProducesADifferentHash_EachTimeDueToRandomSalt()
    {
        var hash1 = _sut.HashPassword("same-password");
        var hash2 = _sut.HashPassword("same-password");

        Assert.NotEqual(hash1, hash2);
        Assert.True(_sut.VerifyPassword("same-password", hash1));
        Assert.True(_sut.VerifyPassword("same-password", hash2));
    }
}
