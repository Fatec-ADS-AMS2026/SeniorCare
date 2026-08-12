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
/// "Product" era uma lacuna conhecida (o stock-web já tinha código pronto pra chamar
/// api/v1/Product antes da entidade existir no backend) — fechada na §9.4; a partir daqui
/// é só mais uma entidade normal na lista abaixo.
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
        ["Manufacturer", "Product", "ProductGroup", "ProductType", "Supplier", "Carrier", "UnitOfMeasure"];

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

    public static IEnumerable<object[]> EntitiesCalledByCareWeb() => CareWebEntities.Select(e => new object[] { e });

    public static IEnumerable<object[]> EntitiesCalledByStockWeb() => StockWebEntities.Select(e => new object[] { e });

    // introduce-senior-portal §3.8 — os endpoints do catálogo não têm o formato CRUD
    // genérico de AssertEntityInContract (POST/DELETE não existem: InstitutionModule é
    // provisionado, nunca criado/excluído por API, §2.3/§3.3), então são verificados à
    // parte. Nenhum front-end consome estas rotas ainda (o Senior Portal é §4) — a
    // publicação automática via Swashbuckle é o próprio contrato aqui.
    [Fact]
    public async Task SeniorPortalCatalog_ContractExposesExpectedRoutesAndVerbs()
    {
        var paths = await GetContractPaths();

        paths.Should().ContainKey("/api/v1/me/modules");
        paths["/api/v1/me/modules"].TryGetProperty("get", out _).Should().BeTrue("GET /api/v1/me/modules é o catálogo mínimo do usuário");
        paths["/api/v1/me/modules"].TryGetProperty("post", out _).Should().BeFalse("o catálogo do usuário é só leitura");

        paths.Should().ContainKey("/api/v1/AdminInstitutionModule");
        paths["/api/v1/AdminInstitutionModule"].TryGetProperty("get", out _).Should().BeTrue();
        paths["/api/v1/AdminInstitutionModule"].TryGetProperty("post", out _).Should().BeFalse("InstitutionModule é provisionado, nunca criado por API");

        paths.Should().ContainKey("/api/v1/AdminInstitutionModule/{id}");
        paths["/api/v1/AdminInstitutionModule/{id}"].TryGetProperty("get", out _).Should().BeTrue();
        paths["/api/v1/AdminInstitutionModule/{id}"].TryGetProperty("put", out _).Should().BeTrue();
        paths["/api/v1/AdminInstitutionModule/{id}"].TryGetProperty("delete", out _).Should().BeFalse("InstitutionModule nunca é excluído — só desabilitado (IsEnabled=false)");
    }

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
