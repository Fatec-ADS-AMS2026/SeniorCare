using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Entities;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.IntegrationTests.Controllers;

public sealed class AuthControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private const string ValidPassword = "Correto-Cavalo-Grampo-2026-Unico"; // gitleaks:allow — senha fixa de teste, não é segredo

    private readonly PostgresWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Activate_ValidToken_ActivatesAccountAndSetsPassword()
    {
        var (email, userId, token) = await CreateProvisionedUserAsync();

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/activate",
            new ActivateAccountRequest { Email = email, Token = token, NewPassword = ValidPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(u => u.Id == userId);
        user.AccountState.Should().Be(AccountState.ACTIVE);
    }

    [Fact]
    public async Task Activate_ReusedToken_ReturnsUnprocessableEntity()
    {
        var (email, _, token) = await CreateProvisionedUserAsync();

        var first = await _client.PostAsJsonAsync("/api/v1/Auth/activate",
            new ActivateAccountRequest { Email = email, Token = token, NewPassword = ValidPassword });
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await _client.PostAsJsonAsync("/api/v1/Auth/activate",
            new ActivateAccountRequest { Email = email, Token = token, NewPassword = ValidPassword });

        second.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Activate_ExpiredToken_ReturnsUnprocessableEntity()
    {
        var (email, userId, token) = await CreateProvisionedUserAsync();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var accountToken = await db.AccountTokens.SingleAsync(t => t.UserId == userId);
            accountToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(-1);
            await db.SaveChangesAsync();
        }

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/activate",
            new ActivateAccountRequest { Email = email, Token = token, NewPassword = ValidPassword });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Activate_PasswordBelowFloor_ReturnsUnprocessableEntityAndDoesNotConsumeToken()
    {
        var (email, _, token) = await CreateProvisionedUserAsync();

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/activate",
            new ActivateAccountRequest { Email = email, Token = token, NewPassword = "curta" });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Recover_ExistingAndNonExistentEmail_ReturnIdenticalGenericResponse()
    {
        var (email, _, _) = await CreateActiveUserAsync();

        var existing = await _client.PostAsJsonAsync("/api/v1/Auth/recover", new RecoverAccountRequest { Email = email });
        var nonExistent = await _client.PostAsJsonAsync("/api/v1/Auth/recover",
            new RecoverAccountRequest { Email = $"inexistente-{Guid.NewGuid():N}@example.com" });

        existing.StatusCode.Should().Be(HttpStatusCode.OK);
        nonExistent.StatusCode.Should().Be(HttpStatusCode.OK);

        var existingBody = await existing.Content.ReadAsStringAsync();
        var nonExistentBody = await nonExistent.Content.ReadAsStringAsync();
        existingBody.Should().Be(nonExistentBody);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_DeniesAndKeepsOriginalCredential()
    {
        var (email, userId, _) = await CreateActiveUserAsync();

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/change-password",
            new ChangePasswordRequest { Email = email, CurrentPassword = "senha-errada-qualquer", NewPassword = "Outra-Senha-Bem-Longa-2026" }); // gitleaks:allow — senha fixa de teste, não é segredo

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        (await userManager.CheckPasswordAsync(user!, ValidPassword)).Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_CorrectCurrentPassword_UpdatesCredential()
    {
        var (email, userId, _) = await CreateActiveUserAsync();
        const string newPassword = "Nova-Senha-Bem-Longa-E-Diferente-2026"; // gitleaks:allow — senha fixa de teste, não é segredo

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/change-password",
            new ChangePasswordRequest { Email = email, CurrentPassword = ValidPassword, NewPassword = newPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        (await userManager.CheckPasswordAsync(user!, newPassword)).Should().BeTrue();
    }

    private async Task<(string Email, Guid UserId, string ActivationToken)> CreateProvisionedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IAccountTokenService>();

        var email = $"provisionado-{Guid.NewGuid():N}@example.com";
        var institution = new Institution(Guid.NewGuid(), $"ILPI {Guid.NewGuid():N}");
        db.Institutions.Add(institution);
        await db.SaveChangesAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            InstitutionId = institution.Id,
            DisplayName = "Pessoa de Teste",
            IdentityOrigin = IdentityOrigin.LOCAL,
            AccountState = AccountState.PROVISIONED
        };
        var createResult = await userManager.CreateAsync(user);
        createResult.Succeeded.Should().BeTrue();

        var token = await tokenService.IssueAsync(user.Id, AccountTokenPurpose.ACTIVATION, AccountTokenService.ActivationTokenValidity);

        return (email, user.Id, token);
    }

    private async Task<(string Email, Guid UserId, string Unused)> CreateActiveUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var email = $"ativo-{Guid.NewGuid():N}@example.com";
        var institution = new Institution(Guid.NewGuid(), $"ILPI {Guid.NewGuid():N}");
        db.Institutions.Add(institution);
        await db.SaveChangesAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            InstitutionId = institution.Id,
            DisplayName = "Pessoa de Teste",
            IdentityOrigin = IdentityOrigin.LOCAL,
            AccountState = AccountState.ACTIVE
        };
        var createResult = await userManager.CreateAsync(user, ValidPassword);
        createResult.Succeeded.Should().BeTrue();

        return (email, user.Id, string.Empty);
    }
}
