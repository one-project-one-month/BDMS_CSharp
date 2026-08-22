using BDMS.Shared;
using Xunit;

namespace Testing;

public class DevCodePasswordTests
{
    [Theory]
    [InlineData("Admin@123")]
    public void HashPassword_ReturnsSameHashThatVerifiesProvidedPassword(string password)
    {
        var hashedPassword = password.HashPassword();
        var repeatedHash = password.HashPassword();

        Assert.False(string.IsNullOrWhiteSpace(hashedPassword));
        Assert.NotEqual(password, hashedPassword);
        Assert.Equal("QkRNU19TVEFUSUNfU0FMVKEKdAh0/Ej3q/crHyhiXLcS2+aHQW9IuzBO3lRFrkLY", hashedPassword);
        Assert.Equal(repeatedHash, hashedPassword);
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
