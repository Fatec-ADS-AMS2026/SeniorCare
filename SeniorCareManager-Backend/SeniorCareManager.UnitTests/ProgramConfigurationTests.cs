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

    [Fact]
    public void GetMissingConfiguration_SmtpParcial_ListaChavesCondicionaisAusentes()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=db;Username=u;Password=p;",
            ["Smtp:Host"] = "smtp.example.test",
        }).Build();

        var missing = Program.GetMissingConfiguration(config);

        missing.Should().Contain(x => x.Contains("Smtp:Port"));
        missing.Should().Contain(x => x.Contains("Smtp:FromAddress"));
        missing.Should().Contain(x => x.Contains("Frontend:ActivationBaseUrl"));
    }

    [Fact]
    public void GetMissingConfiguration_SmtpCompletoSemAutenticacao_NaoRetornaNada()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=db;Username=u;Password=p;",
            ["Smtp:Host"] = "mailpit",
            ["Smtp:Port"] = "1025",
            ["Smtp:FromAddress"] = "noreply@example.test",
            ["Smtp:UseStartTls"] = "false",
            ["Frontend:ActivationBaseUrl"] = "http://localhost:3002/ativar-conta",
        }).Build();

        Program.GetMissingConfiguration(config).Should().BeEmpty();
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void ShouldRunBootstrapOnStartup_RespeitaConfiguracaoAcademica(string? configuredValue, bool expected)
    {
        var values = new Dictionary<string, string?>();
        if (configuredValue is not null)
            values["Bootstrap:RunOnStartup"] = configuredValue;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        Program.ShouldRunBootstrapOnStartup(configuration).Should().Be(expected);
    }

    [Fact]
    public void IsAcademicSeedCommand_AceitaSomenteFlagExplicita()
    {
        Program.IsAcademicSeedCommand(new[] { "--academic-seed" }).Should().BeTrue();
        Program.IsAcademicSeedCommand(Array.Empty<string>()).Should().BeFalse();
    }
}
