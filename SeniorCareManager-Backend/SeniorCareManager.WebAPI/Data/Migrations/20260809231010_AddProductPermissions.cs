using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SeniorCareManager.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "permission",
                columns: new[] { "id", "action", "description", "feature", "is_system_operation", "resource" },
                values: new object[,]
                {
                    { new Guid("4d774a1f-f44f-4abd-a8a2-54d1255df916"), "delete", "Excluir/inativar Product", null, false, "Product" },
                    { new Guid("52373aae-f601-456f-a726-debc551feb9f"), "write", "Criar ou editar Product", null, false, "Product" },
                    { new Guid("b3f21231-386c-49d6-92e6-96cd73147b73"), "read", "Consultar Product", null, false, "Product" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("4d774a1f-f44f-4abd-a8a2-54d1255df916"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("52373aae-f601-456f-a726-debc551feb9f"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("b3f21231-386c-49d6-92e6-96cd73147b73"));
        }
    }
}
