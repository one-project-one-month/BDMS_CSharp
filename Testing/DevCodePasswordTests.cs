using BDMS.Shared;
using Xunit;

namespace Testing;

public class DevCodePasswordTests
{
    [Theory]
    [InlineData("Admin@123")]
    public void HashPassword_ReturnsHashThatVerifiesProvidedPassword(string password)
    {
        var hashedPassword = password.HashPassword();

        Assert.False(string.IsNullOrWhiteSpace(hashedPassword));
        Assert.NotEqual(password, hashedPassword);
        Assert.True(hashedPassword.VerifyPassword(password));
    }

    [Theory]
    [InlineData("Admin@123")]
    public void VerifyPassword_ReturnsFalseForDifferentPassword(string password)
    {
        var hashedPassword = password.HashPassword();

        Assert.False(hashedPassword.VerifyPassword($"{password}wrong"));
    }
}
