using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.WebAPI;
using SeniorCareManager.WebAPI.Data;

namespace SeniorCareManager.IntegrationTests.Infrastructure;

/// <summary>
/// Aponta pra um host que nunca aceita conexão — simula o cenário "processo vivo,
/// banco indisponível" sem precisar derrubar um container no meio do teste.
/// Não sobe nenhum Postgres nem roda migração: a intenção aqui é justamente a
/// aplicação nunca conseguir falar com o banco.
/// </summary>
public sealed class UnavailableDatabaseWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Porta 1 não aceita conexão (não é um serviço TCP válido) — falha rápido,
            // sem depender de timeout de rede longo. Timeout curto reforça isso.
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql("Host=127.0.0.1;Port=1;Database=unreachable;Username=x;Password=x;Timeout=2;"));
        });
    }
}
