using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Controllers;

public sealed class AuthControllerMfaTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AuthControllerMfaTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_SystemAdminWithoutMfaEnrolled_ReturnsEnrollmentRequiredAndNoSession()
    {
        var (email, _) = await CreateActiveSystemAdminAsync(mfaAlreadyEnabled: false);
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.Status.Should().Be("mfa_enrollment_required");
        body.ChallengeToken.Should().NotBeNullOrWhiteSpace();
        body.Identity.Should().BeNull();
        response.Headers.Contains("Set-Cookie").Should().BeFalse();

        // Nenhuma sessão foi criada — /me continua 401 (§7.7).
        (await client.GetAsync("/api/v1/Auth/me")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MfaEnrollThenConfirm_CompletesLoginAndReturnsRecoveryCodesOnce()
    {
        var (email, userId) = await CreateActiveSystemAdminAsync(mfaAlreadyEnabled: false);
        var client = _factory.CreateAuthenticatedFlowClient();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var enrollResponse = await client.PostAsJsonAsync("/api/v1/Auth/mfa/enroll",
            new MfaEnrollRequest { ChallengeToken = loginBody!.ChallengeToken });
        enrollResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var enrollBody = await enrollResponse.Content.ReadFromJsonAsync<MfaEnrollResponse>();
        enrollBody!.AuthenticatorKey.Should().NotBeNullOrWhiteSpace();

        var code = await GenerateCurrentTotpCodeAsync(userId);

        var confirmResponse = await client.PostAsJsonAsync("/api/v1/Auth/mfa/confirm",
            new MfaConfirmRequest { ChallengeToken = loginBody.ChallengeToken, Code = code });

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var confirmBody = await confirmResponse.Content.ReadFromJsonAsync<MfaConfirmResponse>();
        confirmBody!.RecoveryCodes.Should().NotBeEmpty();
        confirmBody.Identity.Should().NotBeNull();

        // A sessão já foi estabelecida pela confirmação — /me funciona sem novo login.
        (await client.GetAsync("/api/v1/Auth/me")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_AccountWithMfaAlreadyEnabled_ReturnsMfaRequired_ThenLoginMfaSucceeds()
    {
        var (email, userId) = await CreateActiveSystemAdminAsync(mfaAlreadyEnabled: true);
        var client = _factory.CreateAuthenticatedFlowClient();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginBody!.Status.Should().Be("mfa_required");

        var code = await GenerateCurrentTotpCodeAsync(userId);
        var verifyResponse = await client.PostAsJsonAsync("/api/v1/Auth/login/mfa",
            new LoginMfaRequest { ChallengeToken = loginBody.ChallengeToken!, Code = code });

        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var verifyBody = await verifyResponse.Content.ReadFromJsonAsync<LoginResponse>();
        verifyBody!.Status.Should().Be("ok");
        (await client.GetAsync("/api/v1/Auth/me")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LoginMfa_WrongCode_DoesNotConsumeChallenge_RetryWithValidCodeSucceeds()
    {
        var (email, userId) = await CreateActiveSystemAdminAsync(mfaAlreadyEnabled: true);
        var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var wrongAttempt = await client.PostAsJsonAsync("/api/v1/Auth/login/mfa",
            new LoginMfaRequest { ChallengeToken = loginBody!.ChallengeToken!, Code = "000000" });
        wrongAttempt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var code = await GenerateCurrentTotpCodeAsync(userId);
        var retryResponse = await client.PostAsJsonAsync("/api/v1/Auth/login/mfa",
            new LoginMfaRequest { ChallengeToken = loginBody.ChallengeToken!, Code = code });

        retryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LoginMfa_RecoveryCode_Succeeds()
    {
        var (email, userId) = await CreateActiveSystemAdminAsync(mfaAlreadyEnabled: false);
        var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        await client.PostAsJsonAsync("/api/v1/Auth/mfa/enroll", new MfaEnrollRequest { ChallengeToken = loginBody!.ChallengeToken });
        var enrollCode = await GenerateCurrentTotpCodeAsync(userId);
        var confirmResponse = await client.PostAsJsonAsync("/api/v1/Auth/mfa/confirm",
            new MfaConfirmRequest { ChallengeToken = loginBody.ChallengeToken, Code = enrollCode });
        var confirmBody = await confirmResponse.Content.ReadFromJsonAsync<MfaConfirmResponse>();
        var recoveryCode = confirmBody!.RecoveryCodes.First();

        // Novo login (o anterior já criou sessão) para obter um desafio MFA_VERIFY novo.
        await client.PostAsync("/api/v1/Auth/logout", null);
        var secondLogin = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });
        var secondLoginBody = await secondLogin.Content.ReadFromJsonAsync<LoginResponse>();
        secondLoginBody!.Status.Should().Be("mfa_required");

        var response = await client.PostAsJsonAsync("/api/v1/Auth/login/mfa",
            new LoginMfaRequest { ChallengeToken = secondLoginBody.ChallengeToken!, Code = recoveryCode });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<(string Email, Guid UserId)> CreateActiveSystemAdminAsync(bool mfaAlreadyEnabled)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var institution = new Institution(Guid.NewGuid(), $"ILPI {Guid.NewGuid():N}");
        db.Institutions.Add(institution);
        var email = $"admin-{Guid.NewGuid():N}@example.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            InstitutionId = institution.Id,
            DisplayName = "Administrador de Sistema",
            IdentityOrigin = IdentityOrigin.LOCAL,
            AccountState = AccountState.ACTIVE,
            IsSystemAdmin = true,
            // UserManager.UpdateAsync exige SecurityStamp não-nulo, e ResetAuthenticatorKeyAsync/
            // SetTwoFactorEnabledAsync/GenerateNewTwoFactorRecoveryCodesAsync chamam UpdateAsync
            // internamente — sem isso o update falha silenciosamente (IdentityResult.Failed não
            // verificado) e o segredo TOTP nunca é persistido.
            SecurityStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = true,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        await userManager.AddPasswordAsync(user, TestIdentitySeeder.DefaultTestPassword);

        if (mfaAlreadyEnabled)
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            await userManager.SetTwoFactorEnabledAsync(user, true);
        }

        return (email, user.Id);
    }

    // AuthenticatorTokenProvider.GenerateAsync do próprio ASP.NET Core Identity sempre
    // retorna null — só ValidateAsync é implementado, porque na vida real o código vem de um
    // app autenticador externo, nunca do servidor. Pra gerar um código válido aqui, replica
    // RFC 6238 puro (HMAC-SHA1, passo de 30s, sem modificador) — o mesmo algoritmo que
    // ValidateAsync espera (confirmado batendo contra ele antes de usar em teste real).
    private async Task<string> GenerateCurrentTotpCodeAsync(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        var key = await userManager.GetAuthenticatorKeyAsync(user!);
        return ComputeTotpCode(key!);
    }

    private static string ComputeTotpCode(string base32Secret)
    {
        var key = Base32Decode(base32Secret);
        var timestep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        var timestepBytes = BitConverter.GetBytes(timestep);
        if (BitConverter.IsLittleEndian) Array.Reverse(timestepBytes);

        using var hmac = new System.Security.Cryptography.HMACSHA1(key);
        var hash = hmac.ComputeHash(timestepBytes);
        var offset = hash[^1] & 0xf;
        var binaryCode = ((hash[offset] & 0x7f) << 24)
            | ((hash[offset + 1] & 0xff) << 16)
            | ((hash[offset + 2] & 0xff) << 8)
            | (hash[offset + 3] & 0xff);
        return (binaryCode % 1_000_000).ToString("D6");
    }

    private static byte[] Base32Decode(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new List<byte>();
        int buffer = 0, bitsLeft = 0;
        foreach (var c in input.TrimEnd('=').ToUpperInvariant())
        {
            var value = alphabet.IndexOf(c);
            if (value < 0) continue;
            buffer = (buffer << 5) | value;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                output.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                bitsLeft -= 8;
            }
        }
        return output.ToArray();
    }
}
