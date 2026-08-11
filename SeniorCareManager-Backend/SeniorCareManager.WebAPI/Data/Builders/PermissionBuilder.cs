using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class PermissionBuilder
{
    // GUIDs fixos (gerados uma vez, congelados aqui) — task 5.5: seed só de permissões
    // sistêmicas, sem Role/PermissionGroup/atribuição/usuário/senha padrão.
    private static readonly (string Resource, Guid ReadId, Guid WriteId, Guid DeleteId)[] CatalogResources =
    {
        ("ProductGroup", Guid.Parse("4822D058-AB3C-4233-9D72-2E06515CF06A"), Guid.Parse("3B31D0C5-B52C-43A0-A638-959DF4E76E9B"), Guid.Parse("28913E62-E1C2-4E04-8559-202C895507DB")),
        ("ProductType", Guid.Parse("1D065485-C1F5-4E68-AF1B-97AAE10D837D"), Guid.Parse("3DC837DF-49D2-4E29-BC50-9B83C211569C"), Guid.Parse("111491E2-DFE4-44BA-A791-70EA17930A56")),
        ("Supplier", Guid.Parse("C479D380-4550-4C6F-ACDE-30BA2AD0123E"), Guid.Parse("80D2BC17-66D5-4F08-A7A0-7123B0556DB7"), Guid.Parse("D099399D-F909-4346-B649-1C45D22AE870")),
        ("UnitOfMeasure", Guid.Parse("BF087D0A-1912-4839-962F-784E232319AB"), Guid.Parse("D1E9B982-CC0F-4194-8D95-09DB6CF51BD6"), Guid.Parse("89EB36B4-F5FF-4DDF-9CC2-875F4FF80F9E")),
        ("Manufacturer", Guid.Parse("BDFFD7C8-9CE2-4FAA-86AC-939116AC0E87"), Guid.Parse("5D767431-4C88-4A96-A7AF-5B27338C4078"), Guid.Parse("325E6BF1-7D8D-4FAF-AB66-216B6FF53012")),
        ("Carrier", Guid.Parse("27725187-00D6-47AB-A39E-9BFDDCF7A039"), Guid.Parse("1B0EBB43-0AA5-449A-9246-D25290C80BC2"), Guid.Parse("E8E754A2-4CB9-461F-A773-3CE9ADBB4ECB")),
        ("HealthInsurancePlan", Guid.Parse("B9EFB981-E8E0-4D44-A45F-89EF7218FA34"), Guid.Parse("91EDE0CB-5492-4E1F-A5AA-67EDD6EEB231"), Guid.Parse("12512F07-E33C-4E91-BB92-5B2B22E389E6")),
        ("Position", Guid.Parse("8E6D9B0D-2ADB-48DA-A8BF-63C6FC775DDC"), Guid.Parse("AB0A7B57-9D6D-4110-85E7-C001F16072FC"), Guid.Parse("91CD749C-2D6E-453F-8D94-991F7F422300")),
        ("Religion", Guid.Parse("2BE2A833-9F65-4CDC-A721-C54AA6C1A606"), Guid.Parse("FC15CA1C-1A6C-4451-926D-D4E746862B93"), Guid.Parse("F7A4145C-E73E-4297-BA84-ACB35D392478")),
        // §9.4 — Produto como catálogo, mesmo padrão read/write/delete dos demais.
        ("Product", Guid.Parse("B3F21231-386C-49D6-92E6-96CD73147B73"), Guid.Parse("52373AAE-F601-456F-A726-DEBC551FEB9F"), Guid.Parse("4D774A1F-F44F-4ABD-A8A2-54D1255DF916")),
    };

    // §6 — vocabulário administrativo. GUIDs fixos, mesma convenção acima.
    private static readonly (Guid Id, string Resource, string Action, string Description, bool IsSystemOperation)[] AdminPermissions =
    {
        (Guid.Parse("188D0A43-405D-4C12-B520-8C4C8F770552"), "AdminUser", "read", "Consultar contas administradas", false),
        (Guid.Parse("F6E45A8B-B685-4115-AB83-50D7BD72C787"), "AdminUser", "write", "Criar contas e alterar seu estado", false),
        (Guid.Parse("5190A83C-A319-4B8D-BB49-D9A758F03C39"), "Role", "read", "Consultar papéis técnicos", false),
        (Guid.Parse("BDCB5194-B695-4ED9-B575-C5F5F3B1304E"), "Role", "write", "Criar ou editar papéis técnicos", false),
        (Guid.Parse("AEC7D56C-388A-40CC-8302-31E03B336773"), "Role", "delete", "Excluir papéis técnicos", false),
        (Guid.Parse("74CD4700-C774-4F08-B525-824397055D0E"), "PermissionGroup", "read", "Consultar grupos de permissão", false),
        (Guid.Parse("C95B0CA8-63E9-4363-B440-A7B8525BF6E4"), "PermissionGroup", "write", "Criar ou editar grupos de permissão", false),
        (Guid.Parse("A5D9432F-B6E2-44A1-8A4A-0736FEE1B740"), "PermissionGroup", "delete", "Excluir grupos de permissão", false),
        (Guid.Parse("16E1A13E-4FCE-4B23-ADFC-D3CBE098D3D2"), "Permission", "read", "Consultar o catálogo de permissões", false),
        (Guid.Parse("7CE48309-C3F1-4826-8FAC-890D425BA9F0"), "OrganizationalRole", "read", "Consultar responsabilidades organizacionais", false),
        (Guid.Parse("467DCC23-D406-4AF0-8110-E35054D2B0A7"), "OrganizationalRole", "write", "Criar ou editar responsabilidades organizacionais", false),
        (Guid.Parse("27B676E3-C59F-496A-ACCC-E2E6A85A5B37"), "OrganizationalRole", "delete", "Excluir responsabilidades organizacionais", false),
        (Guid.Parse("20EFF295-7744-4A5E-86E1-383185BD7DD8"), "OrganizationalRoleAssignment", "read", "Consultar atribuições de responsabilidade", false),
        (Guid.Parse("339FA802-D1B2-4B92-9737-78695F2DC0B9"), "OrganizationalRoleAssignment", "write", "Criar ou encerrar atribuições de responsabilidade", false),
        (Guid.Parse("E1BACFEB-75A4-48E1-BEED-66A6FA0A68A5"), "UserPermissionOverride", "read", "Consultar exceções individuais", false),
        (Guid.Parse("1A5BEB30-0976-46AE-8B8A-43B74C869675"), "UserPermissionOverride", "write", "Criar ou revogar exceções individuais", false),
        (Guid.Parse("16402BE8-D47D-4394-A6D5-1F0F48C6A1B5"), "AccessPolicy", "read", "Consultar políticas condicionais", false),
        (Guid.Parse("F4C0A499-DCB4-47FB-9928-7BE9E2DFE385"), "AccessPolicy", "write", "Criar novas versões de política condicional", false),
        (Guid.Parse("4ACD0F81-38B7-44A3-B082-1EABFC938272"), "InstitutionSecurityPolicy", "read", "Consultar parâmetros de segurança institucionais", false),
        (Guid.Parse("1F5C759B-2C24-41C5-AAAA-D011A45D6E26"), "InstitutionSecurityPolicy", "write", "Alterar parâmetros de segurança institucionais", false),
        (Guid.Parse("BA85FAD3-3CA2-4C97-9C6E-47473E6DD9F4"), "UserSession", "read", "Consultar sessões ativas", false),
        (Guid.Parse("93C69A5F-C382-40F8-8E94-C56C7C890721"), "UserSession", "write", "Revogar sessões ativas", false),
        (Guid.Parse("02CEFFD4-F764-4916-94A3-5A45DB960BF2"), "AccessDecisionExplanation", "read", "Explicar uma decisão de acesso para suporte/auditoria", false),
        // Marcador — não protege nenhum endpoint sozinho; só usado por AdminInvariantService
        // para identificar quem conta como "administrador institucional" (§6.7).
        (Guid.Parse("21344FA6-54A6-4DC9-9696-EEE38BC42680"), "AccessAdministration", "manage", "Marca a identidade como administradora de acesso institucional", false),
    };

    // introduce-senior-portal §2 — gate de visibilidade no catálogo do Senior Portal
    // (ModuleDefinition.RequiredPermissionId). GUIDs fixos, mesma convenção acima.
    private static readonly (Guid Id, string Resource, string Action, string Description, bool IsSystemOperation)[] ModulePermissions =
    {
        (Guid.Parse("E08D83BF-9FF8-4575-BBF1-3E62F7054048"), "Module", "care", "Acessar o módulo de assistência", false),
        (Guid.Parse("F628285F-A074-4053-A1C3-22526C55AA93"), "Module", "stock", "Acessar o módulo de estoque", false),
    };

    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>().HasKey(p => p.Id);
        modelBuilder.Entity<Permission>().Property(p => p.Resource).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Permission>().Property(p => p.Action).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<Permission>().Property(p => p.Feature).HasMaxLength(100);
        modelBuilder.Entity<Permission>()
            .HasIndex(p => new { p.Resource, p.Action, p.Feature })
            .IsUnique();

        modelBuilder.Entity<Permission>().HasData(SeedData());
    }

    private static IEnumerable<Permission> SeedData()
    {
        foreach (var (resource, readId, writeId, deleteId) in CatalogResources)
        {
            yield return new Permission(readId, resource, "read", $"Consultar {resource}");
            yield return new Permission(writeId, resource, "write", $"Criar ou editar {resource}");
            yield return new Permission(deleteId, resource, "delete", $"Excluir/inativar {resource}");
        }

        foreach (var (id, resource, action, description, isSystemOperation) in AdminPermissions)
            yield return new Permission(id, resource, action, description, isSystemOperation);

        foreach (var (id, resource, action, description, isSystemOperation) in ModulePermissions)
            yield return new Permission(id, resource, action, description, isSystemOperation);
    }
}
