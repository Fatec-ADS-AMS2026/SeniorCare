using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
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
    public async Task RunAsync_CalledTwice_CreatesInstitutionAndAdminOnlyOnce()
    {
        using var firstScope = _factory.Services.CreateScope();
        var firstResult = await firstScope.ServiceProvider.GetRequiredService<IBootstrapService>().RunAsync();

        using var secondScope = _factory.Services.CreateScope();
        var secondResult = await secondScope.ServiceProvider.GetRequiredService<IBootstrapService>().RunAsync();

        firstResult.Created.Should().BeTrue();
        firstResult.AdminEmail.Should().Be(BootstrapPostgresWebApplicationFactory.AdminEmail);
        firstResult.ActivationToken.Should().NotBeNullOrWhiteSpace();
        firstResult.NotificationStatus.Should().Be(NotificationDeliveryStatus.Sent);
        _factory.NotificationSender.Messages.Should().ContainSingle();

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
        admin.PasswordHash.Should().BeNull("o bootstrap cria conta PROVISIONED sem senha inicial");
        admin.AccountState.Should().Be(AccountState.PROVISIONED);
        var role = await db.Roles.SingleAsync(r => r.InstitutionId == admin.InstitutionId);
        role.Name.Should().Be("Administrador");
        (await db.UserRoles.AnyAsync(ur => ur.UserId == admin.Id && ur.RoleId == role.Id)).Should().BeTrue();

        var groupId = await db.RolePermissionGroups.Where(x => x.RoleId == role.Id).Select(x => x.PermissionGroupId).SingleAsync();
        var grantedPermissionCount = await db.PermissionGroupPermissions.CountAsync(x => x.PermissionGroupId == groupId);
        var totalPermissionCount = await db.Permissions.CountAsync();
        grantedPermissionCount.Should().Be(totalPermissionCount);

        var userManager = assertScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        const string chosenPassword = "Senha-Escolhida-Pela-Pessoa-2026"; // gitleaks:allow — teste
        (await userManager.AddPasswordAsync(admin, chosenPassword)).Succeeded.Should().BeTrue();
        (await userManager.CheckPasswordAsync(admin, chosenPassword)).Should().BeTrue();
    }
}
