using LifeLink.Infrastructure.Security;
using Xunit;

namespace LifeLink.Tests.Security;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_ShouldGenerateDifferentHashesForSamePassword()
    {
        // Arrange
        var hasher = new AspNetPasswordHasher();
        var password = "SecurePassword123!";

        // Act
        var hash1 = hasher.HashPassword(password);
        var hash2 = hasher.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
    {
        // Arrange
        var hasher = new AspNetPasswordHasher();
        var password = "SecurePassword123!";
        var hash = hasher.HashPassword(password);

        // Act
        var result = hasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
    {
        // Arrange
        var hasher = new AspNetPasswordHasher();
        var password = "SecurePassword123!";
        var hash = hasher.HashPassword(password);

        // Act
        var result = hasher.VerifyPassword("WrongPassword123!", hash);

        // Assert
        Assert.False(result);
    }
}