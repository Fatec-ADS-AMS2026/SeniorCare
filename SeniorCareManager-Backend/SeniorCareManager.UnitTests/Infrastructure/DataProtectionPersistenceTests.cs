using Microsoft.AspNetCore.DataProtection;

namespace SeniorCareManager.UnitTests.Infrastructure;

public sealed class DataProtectionPersistenceTests
{
    [Fact]
    public void PersistedKeyRing_AllowsNewProviderToReadProtectedPayload()
    {
        var path = Path.Combine(Path.GetTempPath(), $"seniorcare-keyring-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        try
        {
            var first = DataProtectionProvider.Create(new DirectoryInfo(path), builder => builder.SetApplicationName("seniorcare-academico"));
            var protectedValue = first.CreateProtector("session").Protect("valid-session");

            var recreated = DataProtectionProvider.Create(new DirectoryInfo(path), builder => builder.SetApplicationName("seniorcare-academico"));

            Assert.Equal("valid-session", recreated.CreateProtector("session").Unprotect(protectedValue));
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }
}
