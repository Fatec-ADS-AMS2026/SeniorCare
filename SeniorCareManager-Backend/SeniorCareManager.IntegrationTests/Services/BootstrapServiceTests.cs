using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.IntegrationTests.Services;

public sealed class BootstrapServiceTests : IClassFixture<BootstrapPostgresWebApplicationFactory>
{
    private readonly BootstrapPostgresWebApplicationFactory _factory;

    public BootstrapServiceTests(BootstrapPostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RunAsync_CalledTwice_CreatesActiveAdminWithOneTimePasswordOnlyOnce()
    {
        using var firstScope = _factory.Services.CreateScope();
        var firstResult = await firstScope.ServiceProvider.GetRequiredService<IBootstrapService>().RunAsync();

        var login = await _factory.CreateClient().PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = BootstrapPostgresWebApplicationFactory.AdminEmail, Password = firstResult.InitialPassword! });

        using var secondScope = _factory.Services.CreateScope();
        var secondResult = await secondScope.ServiceProvider.GetRequiredService<IBootstrapService>().RunAsync();

        firstResult.Created.Should().BeTrue();
        firstResult.AdminEmail.Should().Be(BootstrapPostgresWebApplicationFactory.AdminEmail);
        firstResult.InitialPassword.Should().HaveLength(32);
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResult.Created.Should().BeFalse();
        secondResult.AdminEmail.Should().BeNull();


        using var assertScope = _factory.Services.CreateScope();
        var db = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Institutions.CountAsync()).Should().Be(1);
        (await db.Users.CountAsync()).Should().Be(1);

        // O admin recebe acesso total e já pode iniciar o fluxo autenticado de MFA.
        var admin = await db.Users.SingleAsync();
        admin.PasswordHash.Should().NotBeNull();
        admin.AccountState.Should().Be(AccountState.ACTIVE);
        var role = await db.Roles.SingleAsync(r => r.InstitutionId == admin.InstitutionId);
        role.Name.Should().Be("Administrador");
        (await db.UserRoles.AnyAsync(ur => ur.UserId == admin.Id && ur.RoleId == role.Id)).Should().BeTrue();

        var groupId = await db.RolePermissionGroups.Where(x => x.RoleId == role.Id).Select(x => x.PermissionGroupId).SingleAsync();
        var grantedPermissionCount = await db.PermissionGroupPermissions.CountAsync(x => x.PermissionGroupId == groupId);
        var totalPermissionCount = await db.Permissions.CountAsync();
        grantedPermissionCount.Should().Be(totalPermissionCount);

        var userManager = assertScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        (await userManager.CheckPasswordAsync(admin, firstResult.InitialPassword!)).Should().BeTrue();
    }
}
