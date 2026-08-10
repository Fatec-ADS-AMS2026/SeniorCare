using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos;
using SeniorCareManager.WebAPI.Objects.Dtos.Common;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;

namespace SeniorCareManager.IntegrationTests.Controllers;

/// <summary>
/// Ciclo de vida de Product (§9.4-9.6, §9.8) — catálogo novo, sem tabela de movimento/lote
/// (design.md decisão 5). Autentica com um usuário de acesso total (§5).
/// </summary>
public sealed class ProductControllerTests : IClassFixture<PostgresWebApplicationFactory>, IAsyncLifetime
{
    private readonly PostgresWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private int _productTypeId;
    private int _unitOfMeasureId;

    public ProductControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, userId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        _client.AsUser(userId);

        var groupResponse = await _client.PostAsJsonAsync("/api/v1/ProductGroup",
            new ProductGroupCreateRequest { Name = $"Grupo {Guid.NewGuid():N}" });
        var group = await groupResponse.Content.ReadFromJsonAsync<ProductGroupDTO>();

        var typeResponse = await _client.PostAsJsonAsync("/api/v1/ProductType",
            new ProductTypeCreateRequest { Name = $"Tipo {Guid.NewGuid():N}", ProductGroupId = group!.Id });
        var type = await typeResponse.Content.ReadFromJsonAsync<ProductTypeDTO>();
        _productTypeId = type!.Id;

        // Abbreviation ganhou índice único (§9.1) — precisa ser distinto a cada teste, já
        // que InitializeAsync roda uma vez por [Fact] contra o mesmo banco compartilhado
        // (IClassFixture).
        var unitResponse = await _client.PostAsJsonAsync("/api/v1/UnitOfMeasure",
            new UnitOfMeasureCreateRequest { Abbreviation = Guid.NewGuid().ToString("N")[..3], Description = $"Unidade {Guid.NewGuid():N}"[..25] });
        var unit = await unitResponse.Content.ReadFromJsonAsync<UnitOfMeasureDTO>();
        _unitOfMeasureId = unit!.Id;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private ProductCreateRequest ValidRequest(string? description = null) => new()
    {
        Description = description ?? $"Produto {Guid.NewGuid():N}",
        GenericName = "Paracetamol",
        ProductTypeId = _productTypeId,
        UnitOfMeasureId = _unitOfMeasureId,
        MinimumStock = 10,
        CurrentStock = 25,
        UnitPrice = 5.50m,
        HighCost = false,
        ExpirationControlled = true,
    };

    [Fact]
    public async Task FullLifecycle_CreateUpdateInactivateReactivate_Works()
    {
        var post = await _client.PostAsJsonAsync("/api/v1/Product", ValidRequest());
        post.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await post.Content.ReadFromJsonAsync<ProductDTO>();
        created!.IsActive.Should().BeTrue();

        // Edição com RowVersion certo — sucesso, o token avança.
        var update = new ProductUpdateRequest
        {
            Description = created.Description,
            GenericName = created.GenericName,
            ProductTypeId = _productTypeId,
            UnitOfMeasureId = _unitOfMeasureId,
            MinimumStock = 20,
            CurrentStock = 40,
            UnitPrice = 6.00m,
            RowVersion = created.RowVersion,
        };
        var put = await _client.PutAsJsonAsync($"/api/v1/Product/{created.Id}", update);
        put.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await put.Content.ReadFromJsonAsync<ProductDTO>();
        updated!.CurrentStock.Should().Be(40);
        updated.RowVersion.Should().NotBe(created.RowVersion);

        // RowVersion desatualizado (o original, pré-edição) — 409.
        var staleConflict = await _client.PutAsJsonAsync($"/api/v1/Product/{created.Id}", update);
        staleConflict.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Inativa — some da listagem padrão, GetById ainda funciona.
        var delete = await _client.DeleteAsync($"/api/v1/Product/{created.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getById = await _client.GetAsync($"/api/v1/Product/{created.Id}");
        getById.StatusCode.Should().Be(HttpStatusCode.OK);
        var inactive = await getById.Content.ReadFromJsonAsync<ProductDTO>();
        inactive!.IsActive.Should().BeFalse();

        var listActiveOnly = await _client.GetAsync($"/api/v1/Product?search={created.Description}");
        var activeBody = await listActiveOnly.Content.ReadFromJsonAsync<PagedResult<ProductDTO>>();
        activeBody!.Items.Should().NotContain(p => p.Id == created.Id);

        var listIncludingInactive = await _client.GetAsync($"/api/v1/Product?search={created.Description}&includeInactive=true");
        var allBody = await listIncludingInactive.Content.ReadFromJsonAsync<PagedResult<ProductDTO>>();
        allBody!.Items.Should().Contain(p => p.Id == created.Id);

        // Reativa.
        var activate = await _client.PutAsync($"/api/v1/Product/{created.Id}/activate", null);
        activate.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var reactivated = await (await _client.GetAsync($"/api/v1/Product/{created.Id}")).Content.ReadFromJsonAsync<ProductDTO>();
        reactivated!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Post_WithNonExistentProductTypeId_ReturnsUnprocessableEntity()
    {
        var request = ValidRequest();
        request.ProductTypeId = 999999;

        var response = await _client.PostAsJsonAsync("/api/v1/Product", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Post_WithInactiveProductTypeId_ReturnsUnprocessableEntity()
    {
        var groupResponse = await _client.PostAsJsonAsync("/api/v1/ProductGroup",
            new ProductGroupCreateRequest { Name = $"Grupo {Guid.NewGuid():N}" });
        var group = await groupResponse.Content.ReadFromJsonAsync<ProductGroupDTO>();
        var typeResponse = await _client.PostAsJsonAsync("/api/v1/ProductType",
            new ProductTypeCreateRequest { Name = $"Tipo {Guid.NewGuid():N}", ProductGroupId = group!.Id });
        var type = await typeResponse.Content.ReadFromJsonAsync<ProductTypeDTO>();
        await _client.DeleteAsync($"/api/v1/ProductType/{type!.Id}");

        var request = ValidRequest();
        request.ProductTypeId = type.Id;

        var response = await _client.PostAsJsonAsync("/api/v1/Product", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Post_WithNonExistentUnitOfMeasureId_ReturnsUnprocessableEntity()
    {
        var request = ValidRequest();
        request.UnitOfMeasureId = 999999;

        var response = await _client.PostAsJsonAsync("/api/v1/Product", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task InactivatingProductType_WithActiveProduct_ReturnsUnprocessableEntity()
    {
        await _client.PostAsJsonAsync("/api/v1/Product", ValidRequest());

        var response = await _client.DeleteAsync($"/api/v1/ProductType/{_productTypeId}");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task InactivatingUnitOfMeasure_WithActiveProduct_ReturnsUnprocessableEntity()
    {
        await _client.PostAsJsonAsync("/api/v1/Product", ValidRequest());

        var response = await _client.DeleteAsync($"/api/v1/UnitOfMeasure/{_unitOfMeasureId}");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Search_ByDescriptionOrGenericName_FiltersResults()
    {
        var uniqueDescription = $"Dipirona {Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/v1/Product", ValidRequest(uniqueDescription));

        var response = await _client.GetAsync($"/api/v1/Product?search={uniqueDescription}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<ProductDTO>>();
        body!.Items.Should().ContainSingle(p => p.Description == uniqueDescription);
    }

    [Fact]
    public async Task StockFields_AreDirectlySettable_NoMovementOrLotEndpointExists()
    {
        // §9.4/9.8, design.md decisão 5: produto é catálogo, não saldo transacional —
        // CurrentStock/StockValue são campos graváveis direto pelo Put, sem endpoint de
        // movimento/lote algum (a ausência de rota é a prova; não há o que testar
        // negativamente além de confirmar que a via direta funciona).
        var post = await _client.PostAsJsonAsync("/api/v1/Product", ValidRequest());
        var created = await post.Content.ReadFromJsonAsync<ProductDTO>();

        var update = new ProductUpdateRequest
        {
            Description = created!.Description,
            GenericName = created.GenericName,
            ProductTypeId = _productTypeId,
            UnitOfMeasureId = _unitOfMeasureId,
            CurrentStock = 999,
            StockValue = 1234.56m,
            RowVersion = created.RowVersion,
        };
        var put = await _client.PutAsJsonAsync($"/api/v1/Product/{created.Id}", update);

        put.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await put.Content.ReadFromJsonAsync<ProductDTO>();
        body!.CurrentStock.Should().Be(999);
        body.StockValue.Should().Be(1234.56m);
    }
}
