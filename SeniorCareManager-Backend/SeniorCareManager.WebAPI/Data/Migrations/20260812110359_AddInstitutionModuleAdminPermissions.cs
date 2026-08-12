using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SeniorCareManager.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionModuleAdminPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "permission",
                columns: new[] { "id", "action", "description", "feature", "is_system_operation", "resource" },
                values: new object[,]
                {
                    { new Guid("054251b0-6777-4954-ba9d-64f9d7130e0b"), "write", "Alterar habilitação, ordem, estado e mensagem dos módulos da instituição", null, false, "InstitutionModule" },
                    { new Guid("3bb5116e-6909-4932-9eff-f6d74b54a2f0"), "read", "Consultar a configuração do catálogo de módulos da instituição", null, false, "InstitutionModule" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("054251b0-6777-4954-ba9d-64f9d7130e0b"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("3bb5116e-6909-4932-9eff-f6d74b54a2f0"));
        }
    }
}
