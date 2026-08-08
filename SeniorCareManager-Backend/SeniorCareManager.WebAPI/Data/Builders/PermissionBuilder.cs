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
    }
}
