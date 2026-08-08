using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.WebAPI;
using SeniorCareManager.WebAPI.Data;
using Testcontainers.PostgreSql;

namespace SeniorCareManager.IntegrationTests.Infrastructure;

/// <summary>
/// Sobe um contêiner PostgreSQL efêmero antes do primeiro teste e o descarta ao final.
/// Executa a migração desde banco vazio antes de qualquer caso de teste.
/// </summary>
public sealed class PostgresWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("db_seniorcare_test")
        .WithUsername("postgres")
        .WithPassword("postgres_test")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // A migração é executada aqui — depois que o host está pronto — usando o
        // service provider real da aplicação, evitando um segundo container de DI.
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Substitui o DbContext registrado pelo Startup por um apontando para o
            // contêiner efêmero — garante isolamento total do banco de produção/dev.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(_postgres.GetConnectionString()));

            // Esquema "Test" adicional (§5): requisição sem o header X-Test-UserId continua
            // anônima via o esquema Cookie normal (produção não muda); com o header, um
            // esquema de política encaminha para o TestAuthHandler — sem precisar de login
            // real, que só existe a partir da §7.
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "TestOrCookie";
                    options.DefaultChallengeScheme = "TestOrCookie";
                })
                .AddPolicyScheme("TestOrCookie", "Test ou Cookie", policyOptions =>
                {
                    policyOptions.ForwardDefaultSelector = context =>
                        context.Request.Headers.ContainsKey(TestAuthHandler.UserIdHeader)
                            ? TestAuthHandler.SchemeName
                            : CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
