using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Controllers;

public sealed class AdminInstitutionSecurityControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AdminInstitutionSecurityControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Put_OutOfRange_ReturnsUnprocessableEntityAndKeepsPrevious()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (_, adminId) = await TestIdentitySeeder.SeedFullAccessUserWithPasswordAsync(db, userManager);
        var client = _factory.CreateClient().AsUser(adminId);

        var response = await client.PutAsJsonAsync("/api/v1/AdminInstitutionSecurity", new InstitutionSecurityPolicyUpdateRequest
        {
            AccessTokenDurationMinutes = 9999,
            CurrentPassword = TestIdentitySeeder.DefaultTestPassword,
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var getResponse = await client.GetAsync("/api/v1/AdminInstitutionSecurity");
        var current = await getResponse.Content.ReadFromJsonAsync<InstitutionSecurityPolicyDTO>();
        current!.AccessTokenDurationMinutes.Should().BeNull();
    }

    [Fact]
    public async Task Put_WrongCurrentPassword_Denies()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (_, adminId) = await TestIdentitySeeder.SeedFullAccessUserWithPasswordAsync(db, userManager);
        var client = _factory.CreateClient().AsUser(adminId);

        var response = await client.PutAsJsonAsync("/api/v1/AdminInstitutionSecurity", new InstitutionSecurityPolicyUpdateRequest
        {
            MfaRequiredForAllUsers = true,
            CurrentPassword = "senha-errada",
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Put_ValidSettings_Persists()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (_, adminId) = await TestIdentitySeeder.SeedFullAccessUserWithPasswordAsync(db, userManager);
        var client = _factory.CreateClient().AsUser(adminId);

        var response = await client.PutAsJsonAsync("/api/v1/AdminInstitutionSecurity", new InstitutionSecurityPolicyUpdateRequest
        {
            LockoutDurationMinutes = 30,
            MaxFailedAttempts = 5,
            AccessTokenDurationMinutes = 15,
            RefreshTokenDurationDays = 7,
            MfaRequiredForAllUsers = true,
            CurrentPassword = TestIdentitySeeder.DefaultTestPassword,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<InstitutionSecurityPolicyDTO>();
        updated!.LockoutDurationMinutes.Should().Be(30);
        updated.MfaRequiredForAllUsers.Should().BeTrue();
    }
}
