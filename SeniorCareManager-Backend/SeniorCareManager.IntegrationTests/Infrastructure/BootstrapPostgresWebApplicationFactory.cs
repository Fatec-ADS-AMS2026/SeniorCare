using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.WebAPI;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Services.Interfaces;
using Testcontainers.PostgreSql;

namespace SeniorCareManager.IntegrationTests.Infrastructure;

/// <summary>
/// Mesmo padrão de <see cref="PostgresWebApplicationFactory"/> (contêiner Postgres efêmero,
/// migração no boot), mas com as três variáveis de bootstrap pré-configuradas — necessário
/// para testar o fluxo idempotente de instituição/admin PROVISIONED sem tocar variáveis de
/// ambiente do processo (evitaria interferência entre classes de teste rodando em paralelo).
/// </summary>
public sealed class BootstrapPostgresWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string InstitutionName = "ILPI Teste";
    public const string AdminEmail = "admin@ilpi-teste.local";
    public const string AdminDisplayName = "Administrador de Teste";
    public TestNotificationSender NotificationSender { get; } = new() { Status = NotificationDeliveryStatus.Sent };

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("db_seniorcare_test_bootstrap")
        .WithUsername("postgres")
        .WithPassword("postgres_test")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Bootstrap:InstitutionName"] = InstitutionName,
                ["Bootstrap:AdminEmail"] = AdminEmail,
                ["Bootstrap:AdminDisplayName"] = AdminDisplayName,
                ["Frontend:ActivationBaseUrl"] = "http://localhost:3002/ativar-conta",
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(_postgres.GetConnectionString()));

            var notificationDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(INotificationSender));
            if (notificationDescriptor != null)
                services.Remove(notificationDescriptor);
            services.AddSingleton<INotificationSender>(NotificationSender);
        });
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
