using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data.Builders;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data
{
    // IdentityUserContext (não IdentityDbContext completo) dá Users/UserClaims/UserLogins/
    // UserTokens, mas não Roles/UserRoles — o RBAC próprio (Role/Permission/PermissionGroup,
    // §5) não deve colidir com IdentityRole.
    public class AppDbContext : IdentityUserContext<ApplicationUser, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Supplier> Supplier { get; set; }
        public DbSet<UnitOfMeasure> UnitOfMeasures  { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Carrier> Carriers { get; set; }
        public DbSet<HealthInsurancePlan> HealthInsurancePlans { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Religion> Religions { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<AccountToken> AccountTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ProductGroupBuilder.Build(modelBuilder);
            ProductTypeBuilder.Build(modelBuilder);
            SupplierBuilder.Build(modelBuilder);
            UnitOfMeasureBuilder.Build(modelBuilder);
            ManufacturerBuilder.Build(modelBuilder);
            CarrierBuilder.Build(modelBuilder);
            HealthInsurancePlanBuilder.Build(modelBuilder);
            PositionBuilder.Build(modelBuilder);
            ReligionBuilder.Build(modelBuilder);
            InstitutionBuilder.Build(modelBuilder);
            ApplicationUserBuilder.Build(modelBuilder);
            AccountTokenBuilder.Build(modelBuilder);
        }
    }
}