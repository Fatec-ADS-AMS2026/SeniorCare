using FluentAssertions;
using SeniorCareManager.WebAPI.Services.Entities;

namespace SeniorCareManager.UnitTests.Services;

public class CommonPasswordBlocklistTests
{
    private readonly CommonPasswordBlocklist _blocklist = new();

    [Theory]
    [InlineData("123456")]
    [InlineData("password")]
    [InlineData("PASSWORD")]
    [InlineData("senha123")]
    public void IsCommon_KnownCommonPassword_ReturnsTrue(string password)
    {
        _blocklist.IsCommon(password).Should().BeTrue();
    }

    [Fact]
    public void IsCommon_LongRandomPassphrase_ReturnsFalse()
    {
        _blocklist.IsCommon("Correto-Cavalo-Grampo-Bateria-2026-Unico").Should().BeFalse();
    }
}
