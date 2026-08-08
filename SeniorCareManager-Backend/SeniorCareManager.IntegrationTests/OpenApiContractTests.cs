using System.Net;
using System.Text.Json;
using FluentAssertions;
using SeniorCareManager.IntegrationTests.Infrastructure;

namespace SeniorCareManager.IntegrationTests;

/// <summary>
/// Tarefa 3.8: publica (via /swagger/v1/swagger.json, disponível em todo ambiente —
/// ver Startup.cs) e valida o contrato OpenAPI contra o que os dois front-ends
/// realmente chamam hoje.
///
/// A lista de entidades por front-end vem de uma leitura direta dos arquivos
/// *Service.ts (generateGenericMethods&lt;T&gt;('Nome')) em 2026-08-08:
///   care-web  (SeniorCareManagerFrontend): HealthInsurancePlan, Position, Religion
///   stock-web (SeniorStockManagerFrontend): Manufacturer, Product, ProductGroup,
///             ProductType, Supplier, Carrier, UnitOfMeasure
///
/// "Product" é uma lacuna CONHECIDA e já rastreada — o stock-web já tem código
/// pronto para chamar api/v1/Product, mas a entidade só é criada na tarefa 9.4.
/// Este teste documenta a lacuna explicitamente (falha se ela for corrigida sem
/// atualizar o teste, e falha se qualquer OUTRA rota regredir).
/// </summary>
public sealed class OpenApiContractTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OpenApiContractTests(PostgresWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static readonly string[] CareWebEntities = ["HealthInsurancePlan", "Position", "Religion"];

    private static readonly string[] StockWebEntities =
        ["Manufacturer", "ProductGroup", "ProductType", "Supplier", "Carrier", "UnitOfMeasure"];

    private const string KnownMissingEntity = "Product"; // tarefa 9.4

    [Fact]
    public async Task SwaggerJson_IsAvailableAndWellFormed()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(body);
        act.Should().NotThrow("o contrato publicado precisa ser JSON válido para qualquer ferramenta consumir");
    }

    [Theory]
    [MemberData(nameof(EntitiesCalledByCareWeb))]
    public async Task CareWeb_CalledEntity_ExistsInContract(string entity) => await AssertEntityInContract(entity);

    [Theory]
    [MemberData(nameof(EntitiesCalledByStockWeb))]
    public async Task StockWeb_CalledEntity_ExistsInContract(string entity) => await AssertEntityInContract(entity);

    [Fact]
    public async Task StockWeb_Product_IsAKnownGapNotYetInContract()
    {
        // Falha (nos avisando pra atualizar este teste) se alguém implementar Product
        // (tarefa 9.4) sem remover esta marcação de lacuna conhecida.
        var paths = await GetContractPaths();
        paths.Should().NotContainKey($"/api/v1/{KnownMissingEntity}",
            $"{KnownMissingEntity} ainda não existe no backend (tarefa 9.4) — se este teste falhar, ótimo: " +
            "significa que a lacuna foi fechada, então troque este teste por um CareWeb_CalledEntity_ExistsInContract normal");
    }

    public static IEnumerable<object[]> EntitiesCalledByCareWeb() => CareWebEntities.Select(e => new object[] { e });

    public static IEnumerable<object[]> EntitiesCalledByStockWeb() => StockWebEntities.Select(e => new object[] { e });

    private async Task AssertEntityInContract(string entity)
    {
        var paths = await GetContractPaths();
        var basePath = $"/api/v1/{entity}";

        paths.Should().ContainKey(basePath, $"o front-end chama {entity}/ — a rota precisa existir no contrato publicado");
        paths[basePath].TryGetProperty("get", out _).Should().BeTrue($"GET {basePath} (listagem) é chamado pelo front-end");
        paths[basePath].TryGetProperty("post", out _).Should().BeTrue($"POST {basePath} (criação) é chamado pelo front-end");

        var byIdPath = $"{basePath}/{{id}}";
        paths.Should().ContainKey(byIdPath, $"o front-end chama {entity}/{{id}} para leitura, atualização e remoção");
        paths[byIdPath].TryGetProperty("get", out _).Should().BeTrue();
        paths[byIdPath].TryGetProperty("put", out _).Should().BeTrue();
        paths[byIdPath].TryGetProperty("delete", out _).Should().BeTrue();
    }

    private async Task<Dictionary<string, JsonElement>> GetContractPaths()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("paths").EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.Clone());
    }
}
