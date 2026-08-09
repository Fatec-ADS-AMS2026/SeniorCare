using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Controllers;

public sealed class AdminUserSessionControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AdminUserSessionControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Revoke_WrongCurrentPassword_Denies()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserWithPasswordAsync(db, userManager);
        var targetUserId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db, institutionId);

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = targetUserId,
            CreatedAtUtc = DateTime.UtcNow,
            LastSeenAtUtc = DateTime.UtcNow,
        };
        db.UserSessions.Add(session);
        await db.SaveChangesAsync();

        var client = _factory.CreateClient().AsUser(adminId);
        var response = await client.PutAsJsonAsync($"/api/v1/AdminUserSession/{session.Id}/revoke",
            new RevokeSessionRequest { CurrentPassword = "senha-errada" });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RevokeAll_RevokesEveryActiveSession()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserWithPasswordAsync(db, userManager);
        var targetUserId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db, institutionId);

        db.UserSessions.AddRange(
            new UserSession { Id = Guid.NewGuid(), UserId = targetUserId, CreatedAtUtc = DateTime.UtcNow, LastSeenAtUtc = DateTime.UtcNow },
            new UserSession { Id = Guid.NewGuid(), UserId = targetUserId, CreatedAtUtc = DateTime.UtcNow, LastSeenAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var client = _factory.CreateClient().AsUser(adminId);
        var response = await client.PutAsJsonAsync($"/api/v1/AdminUserSession/revoke-all?userId={targetUserId}",
            new RevokeSessionRequest { CurrentPassword = TestIdentitySeeder.DefaultTestPassword });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var remainingActive = await assertDb.UserSessions.CountAsync(s => s.UserId == targetUserId && s.RevokedAtUtc == null);
        remainingActive.Should().Be(0);
    }
}
