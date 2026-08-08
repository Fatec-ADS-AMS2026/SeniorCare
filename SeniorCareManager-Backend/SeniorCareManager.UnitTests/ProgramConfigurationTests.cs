using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SeniorCareManager.WebAPI;

namespace SeniorCareManager.UnitTests;

/// <summary>
/// Cenário "Configuração obrigatória ausente" da spec runtime-configuration:
/// startup falha com mensagem clara quando falta ConnectionStrings:DefaultConnection,
/// sem nunca expor o valor configurado.
/// </summary>
public class ProgramConfigurationTests
{
    private static IConfiguration BuildConfig(string? connectionString)
    {
        var dict = new Dictionary<string, string?>();
        if (connectionString is not null)
            dict["ConnectionStrings:DefaultConnection"] = connectionString;

        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public void GetMissingConfiguration_SemConnectionString_RetornaChaveAusente()
    {
        var config = BuildConfig(connectionString: null);

        var missing = Program.GetMissingConfiguration(config);

        missing.Should().ContainSingle()
            .Which.Should().Contain("ConnectionStrings:DefaultConnection");
    }

    [Fact]
    public void GetMissingConfiguration_ConnectionStringVazia_RetornaChaveAusente()
    {
        var config = BuildConfig(connectionString: "   ");

        var missing = Program.GetMissingConfiguration(config);

        missing.Should().ContainSingle();
    }

    [Fact]
    public void GetMissingConfiguration_ConnectionStringPresente_NaoRetornaNada()
    {
        var config = BuildConfig(connectionString: "Host=localhost;Database=db;Username=u;Password=p;");

        var missing = Program.GetMissingConfiguration(config);

        missing.Should().BeEmpty();
    }
}
