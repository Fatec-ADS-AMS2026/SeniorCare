using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SeniorCareManager.WebAPI.Data;
using Testcontainers.PostgreSql;

namespace SeniorCareManager.IntegrationTests;

/// <summary>
/// Cenário "Atualização de versão" da spec automated-quality-gates: uma migração é
/// testada não só em banco vazio (já coberto por toda a suíte via
/// <see cref="Infrastructure.PostgresWebApplicationFactory"/>), mas também sobre um banco
/// da versão imediatamente anterior com dado real — os dados de catálogo precisam
/// permanecer íntegros e as novas restrições precisam ser satisfeitas, ou a atualização
/// falha antes de publicar a aplicação.
///
/// Alvo: AddCatalogActiveState (adiciona `is_active boolean NOT NULL DEFAULT true` a 6
/// tabelas de catálogo existentes, além de mexer em FK/índice do producttype) sobre o
/// estado imediatamente anterior (AddAuditEvent) — a migração mais recente com impacto
/// real em tabela já populada, não um caso trivial de "só cria tabela nova".
///
/// As linhas sintéticas são inseridas via SQL cru, não via `AppDbContext`/`DbSet`: o
/// DbContext em tempo de execução sempre reflete o modelo ATUAL (já com `is_active`) —
/// inserir pela ORM não simularia de verdade o estado histórico da tabela antes da
/// migração. SQL cru é a técnica correta para esse tipo de teste.
/// </summary>
public sealed class MigrationUpgradeTests : IAsyncLifetime
{
    private const string PreviousMigration = "20260809142400_AddAuditEvent";
    private const string TargetMigration = "20260809230019_AddCatalogActiveState";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("db_seniorcare_migration_test")
        .WithUsername("postgres")
        .WithPassword("postgres_test")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();

    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddCatalogActiveState_SobreDadoDaVersaoAnterior_MantemDadoEPopulaColunaNova()
    {
        await using var context = CreateContext();
        var migrator = context.GetInfrastructure().GetRequiredService<IMigrator>();

        // 1. Banco no estado da versão imediatamente anterior.
        await migrator.MigrateAsync(PreviousMigration);

        // 2. Dado sintético de catálogo pré-existente, como estaria em produção antes do
        //    upgrade — via SQL cru (ver motivo na doc da classe).
        await using (var connection = new NpgsqlConnection(_postgres.GetConnectionString()))
        {
            await connection.OpenAsync();
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                INSERT INTO religion (name) VALUES ('Religião Sintética de Teste');
                INSERT INTO position (name) VALUES ('Cargo Sintético de Teste');
                INSERT INTO unitofmeasure (description, abbreviation) VALUES ('Unidade Sintética', 'UST');
                INSERT INTO supplier (corporate_name, trade_name, cpf_cnpj, email, phone, postal_code, street, number, district, address_complement, city, state)
                    VALUES ('Fornecedor Sintético Ltda', 'Fornecedor Sintético', '00000000000000', 'fornecedor@example.com', '00000000000', '00000000', 'Rua Sintética', '1', 'Centro', '', 'Cidade Sintética', 'SP');
                INSERT INTO productgroup (name) VALUES ('Grupo Sintético de Teste');
                INSERT INTO producttype (name, productgroupid)
                    VALUES ('Tipo Sintético de Teste', (SELECT id FROM productgroup WHERE name = 'Grupo Sintético de Teste'));
                """;
            await cmd.ExecuteNonQueryAsync();
        }

        // 3. Aplica a migração alvo — é aqui que a nova coluna NOT NULL entra em cena
        //    sobre as tabelas já populadas acima.
        var act = () => migrator.MigrateAsync(TargetMigration);
        await act.Should().NotThrowAsync("dado existente de catálogo não pode quebrar a migração");

        // 4. Integridade: os dados sobrevivem, e a nova coluna foi populada com o
        //    default (true) nas linhas pré-existentes — não ficaram NULL nem foram
        //    apagadas.
        await using (var connection = new NpgsqlConnection(_postgres.GetConnectionString()))
        {
            await connection.OpenAsync();

            async Task<(string Name, bool IsActive)> ReadOneAsync(string table, string nameColumn)
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = $"SELECT {nameColumn}, is_active FROM {table} LIMIT 1";
                await using var reader = await cmd.ExecuteReaderAsync();
                await reader.ReadAsync();
                return (reader.GetString(0), reader.GetBoolean(1));
            }

            var religion = await ReadOneAsync("religion", "name");
            religion.Name.Should().Be("Religião Sintética de Teste");
            religion.IsActive.Should().BeTrue();

            var position = await ReadOneAsync("position", "name");
            position.Name.Should().Be("Cargo Sintético de Teste");
            position.IsActive.Should().BeTrue();

            var unitOfMeasure = await ReadOneAsync("unitofmeasure", "description");
            unitOfMeasure.Name.Should().Be("Unidade Sintética");
            unitOfMeasure.IsActive.Should().BeTrue();

            var supplier = await ReadOneAsync("supplier", "corporate_name");
            supplier.Name.Should().Be("Fornecedor Sintético Ltda");
            supplier.IsActive.Should().BeTrue();

            var productGroup = await ReadOneAsync("productgroup", "name");
            productGroup.Name.Should().Be("Grupo Sintético de Teste");
            productGroup.IsActive.Should().BeTrue();

            // producttype é o caso mais delicado: a migração alvo dropa e recria o
            // índice/FK dele antes de adicionar a coluna — confirma que o vínculo com
            // productgroup sobrevive à recriação, não só a linha em si.
            var productType = await ReadOneAsync("producttype", "name");
            productType.Name.Should().Be("Tipo Sintético de Teste");
            productType.IsActive.Should().BeTrue();

            await using var fkCmd = connection.CreateCommand();
            fkCmd.CommandText = """
                SELECT pt.name, pg.name FROM producttype pt
                JOIN productgroup pg ON pg.id = pt.productgroupid
                WHERE pt.name = 'Tipo Sintético de Teste'
                """;
            await using var fkReader = await fkCmd.ExecuteReaderAsync();
            (await fkReader.ReadAsync()).Should().BeTrue("o vínculo FK entre producttype e productgroup deve sobreviver à recriação do índice/FK pela migração");
            fkReader.GetString(1).Should().Be("Grupo Sintético de Teste");
        }
    }
}
