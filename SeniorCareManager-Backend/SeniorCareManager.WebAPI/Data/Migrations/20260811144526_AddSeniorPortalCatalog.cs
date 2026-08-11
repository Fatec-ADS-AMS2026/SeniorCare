using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SeniorCareManager.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeniorPortalCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "moduledefinition",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    path = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    required_permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moduledefinition", x => x.id);
                    table.ForeignKey(
                        name: "FK_moduledefinition_permission_required_permission_id",
                        column: x => x.required_permission_id,
                        principalTable: "permission",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "institutionmodule",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_definition_id = table.Column<int>(type: "integer", nullable: false),
                    operational_state = table.Column<int>(type: "integer", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    operational_message = table.Column<string>(type: "character varying(280)", maxLength: 280, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_institutionmodule", x => x.id);
                    table.CheckConstraint("ck_institutionmodule_operational_state", "operational_state BETWEEN 0 AND 3");
                    table.ForeignKey(
                        name: "FK_institutionmodule_moduledefinition_module_definition_id",
                        column: x => x.module_definition_id,
                        principalTable: "moduledefinition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "permission",
                columns: new[] { "id", "action", "description", "feature", "is_system_operation", "resource" },
                values: new object[,]
                {
                    { new Guid("e08d83bf-9ff8-4575-bbf1-3e62f7054048"), "care", "Acessar o módulo de assistência", null, false, "Module" },
                    { new Guid("f628285f-a074-4053-a1c3-22526c55aa93"), "stock", "Acessar o módulo de estoque", null, false, "Module" }
                });

            migrationBuilder.InsertData(
                table: "moduledefinition",
                columns: new[] { "id", "description", "icon", "is_active", "key", "name", "path", "required_permission_id" },
                values: new object[,]
                {
                    { 1, "Cuidado e acompanhamento dos residentes", "HeartStraight", true, "care", "Assistência", "/care", new Guid("e08d83bf-9ff8-4575-bbf1-3e62f7054048") },
                    { 2, "Controle de insumos e produtos", "Package", true, "stock", "Estoque", "/stock", new Guid("f628285f-a074-4053-a1c3-22526c55aa93") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_institutionmodule_institution_id_module_definition_id",
                table: "institutionmodule",
                columns: new[] { "institution_id", "module_definition_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_institutionmodule_module_definition_id",
                table: "institutionmodule",
                column: "module_definition_id");

            migrationBuilder.CreateIndex(
                name: "IX_moduledefinition_key",
                table: "moduledefinition",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_moduledefinition_required_permission_id",
                table: "moduledefinition",
                column: "required_permission_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "institutionmodule");

            migrationBuilder.DropTable(
                name: "moduledefinition");

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("e08d83bf-9ff8-4575-bbf1-3e62f7054048"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("f628285f-a074-4053-a1c3-22526c55aa93"));
        }
    }
}
