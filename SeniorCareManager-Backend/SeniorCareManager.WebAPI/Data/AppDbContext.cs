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
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<PermissionGroup> PermissionGroups { get; set; }
        public DbSet<PermissionGroupPermission> PermissionGroupPermissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermissionGroup> RolePermissionGroups { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<OrganizationalRole> OrganizationalRoles { get; set; }
        public DbSet<OrganizationalRolePermissionGroup> OrganizationalRolePermissionGroups { get; set; }
        public DbSet<OrganizationalRoleAssignment> OrganizationalRoleAssignments { get; set; }
        public DbSet<UserPermissionOverride> UserPermissionOverrides { get; set; }
        public DbSet<AccessPolicy> AccessPolicies { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<AuditEvent> AuditEvents { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ModuleDefinition> ModuleDefinitions { get; set; }
        public DbSet<InstitutionModule> InstitutionModules { get; set; }

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
            PermissionBuilder.Build(modelBuilder);
            PermissionGroupBuilder.Build(modelBuilder);
            RoleBuilder.Build(modelBuilder);
            OrganizationalRoleBuilder.Build(modelBuilder);
            OrganizationalRoleAssignmentBuilder.Build(modelBuilder);
            UserPermissionOverrideBuilder.Build(modelBuilder);
            AccessPolicyBuilder.Build(modelBuilder);
            UserSessionBuilder.Build(modelBuilder);
            AuditEventBuilder.Build(modelBuilder);
            ProductBuilder.Build(modelBuilder);
            ModuleDefinitionBuilder.Build(modelBuilder);
            InstitutionModuleBuilder.Build(modelBuilder);
        }
    }
}