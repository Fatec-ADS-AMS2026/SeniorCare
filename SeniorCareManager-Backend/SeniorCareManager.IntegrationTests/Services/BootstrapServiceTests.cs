using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
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
    public async Task RunAsync_CalledTwice_CreatesInstitutionAndAdminOnlyOnce()
    {
        using var firstScope = _factory.Services.CreateScope();
        var firstResult = await firstScope.ServiceProvider.GetRequiredService<IBootstrapService>().RunAsync();

        using var secondScope = _factory.Services.CreateScope();
        var secondResult = await secondScope.ServiceProvider.GetRequiredService<IBootstrapService>().RunAsync();

        firstResult.Created.Should().BeTrue();
        firstResult.AdminEmail.Should().Be(BootstrapPostgresWebApplicationFactory.AdminEmail);
        firstResult.ActivationToken.Should().NotBeNullOrWhiteSpace();

        // Idempotente: a segunda chamada não recria nem redefine nada.
        secondResult.Created.Should().BeFalse();
        secondResult.AdminEmail.Should().BeNull();

        using var assertScope = _factory.Services.CreateScope();
        var db = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Institutions.CountAsync()).Should().Be(1);
        (await db.Users.CountAsync()).Should().Be(1);

        // §6: sem isto, o admin ativado não conseguiria chamar nenhuma API administrativa —
        // mesma classe de teste porque ambas as asserções dependem do único bootstrap deste
        // container compartilhado (um segundo teste chamando RunAsync de novo entraria em
        // corrida com a checagem de idempotência acima).
        var admin = await db.Users.SingleAsync();
        var role = await db.Roles.SingleAsync(r => r.InstitutionId == admin.InstitutionId);
        role.Name.Should().Be("Administrador");
        (await db.UserRoles.AnyAsync(ur => ur.UserId == admin.Id && ur.RoleId == role.Id)).Should().BeTrue();

        var groupId = await db.RolePermissionGroups.Where(x => x.RoleId == role.Id).Select(x => x.PermissionGroupId).SingleAsync();
        var grantedPermissionCount = await db.PermissionGroupPermissions.CountAsync(x => x.PermissionGroupId == groupId);
        var totalPermissionCount = await db.Permissions.CountAsync();
        grantedPermissionCount.Should().Be(totalPermissionCount);
    }
}
