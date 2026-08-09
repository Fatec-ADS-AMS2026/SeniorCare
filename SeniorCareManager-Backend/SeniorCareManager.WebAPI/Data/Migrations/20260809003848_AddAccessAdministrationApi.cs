using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SeniorCareManager.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessAdministrationApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "role",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "permissiongroup",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "organizationalrole",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<int>(
                name: "access_token_duration_minutes",
                table: "institution",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "lockout_duration_minutes",
                table: "institution",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_failed_attempts",
                table: "institution",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "mfa_required_for_all_users",
                table: "institution",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "refresh_token_duration_days",
                table: "institution",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "usersession",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_seen_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usersession", x => x.id);
                    table.ForeignKey(
                        name: "FK_usersession_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "permission",
                columns: new[] { "id", "action", "description", "feature", "is_system_operation", "resource" },
                values: new object[,]
                {
                    { new Guid("02ceffd4-f764-4916-94a3-5a45db960bf2"), "read", "Explicar uma decisão de acesso para suporte/auditoria", null, false, "AccessDecisionExplanation" },
                    { new Guid("16402be8-d47d-4394-a6d5-1f0f48c6a1b5"), "read", "Consultar políticas condicionais", null, false, "AccessPolicy" },
                    { new Guid("16e1a13e-4fce-4b23-adfc-d3cbe098d3d2"), "read", "Consultar o catálogo de permissões", null, false, "Permission" },
                    { new Guid("188d0a43-405d-4c12-b520-8c4c8f770552"), "read", "Consultar contas administradas", null, false, "AdminUser" },
                    { new Guid("1a5beb30-0976-46ae-8b8a-43b74c869675"), "write", "Criar ou revogar exceções individuais", null, false, "UserPermissionOverride" },
                    { new Guid("1f5c759b-2c24-41c5-aaaa-d011a45d6e26"), "write", "Alterar parâmetros de segurança institucionais", null, false, "InstitutionSecurityPolicy" },
                    { new Guid("20eff295-7744-4a5e-86e1-383185bd7dd8"), "read", "Consultar atribuições de responsabilidade", null, false, "OrganizationalRoleAssignment" },
                    { new Guid("21344fa6-54a6-4dc9-9696-eee38bc42680"), "manage", "Marca a identidade como administradora de acesso institucional", null, false, "AccessAdministration" },
                    { new Guid("27b676e3-c59f-496a-accc-e2e6a85a5b37"), "delete", "Excluir responsabilidades organizacionais", null, false, "OrganizationalRole" },
                    { new Guid("339fa802-d1b2-4b92-9737-78695f2dc0b9"), "write", "Criar ou encerrar atribuições de responsabilidade", null, false, "OrganizationalRoleAssignment" },
                    { new Guid("467dcc23-d406-4af0-8110-e35054d2b0a7"), "write", "Criar ou editar responsabilidades organizacionais", null, false, "OrganizationalRole" },
                    { new Guid("4acd0f81-38b7-44a3-b082-1eabfc938272"), "read", "Consultar parâmetros de segurança institucionais", null, false, "InstitutionSecurityPolicy" },
                    { new Guid("5190a83c-a319-4b8d-bb49-d9a758f03c39"), "read", "Consultar papéis técnicos", null, false, "Role" },
                    { new Guid("74cd4700-c774-4f08-b525-824397055d0e"), "read", "Consultar grupos de permissão", null, false, "PermissionGroup" },
                    { new Guid("7ce48309-c3f1-4826-8fac-890d425ba9f0"), "read", "Consultar responsabilidades organizacionais", null, false, "OrganizationalRole" },
                    { new Guid("93c69a5f-c382-40f8-8e94-c56c7c890721"), "write", "Revogar sessões ativas", null, false, "UserSession" },
                    { new Guid("a5d9432f-b6e2-44a1-8a4a-0736fee1b740"), "delete", "Excluir grupos de permissão", null, false, "PermissionGroup" },
                    { new Guid("aec7d56c-388a-40cc-8302-31e03b336773"), "delete", "Excluir papéis técnicos", null, false, "Role" },
                    { new Guid("ba85fad3-3ca2-4c97-9c6e-47473e6dd9f4"), "read", "Consultar sessões ativas", null, false, "UserSession" },
                    { new Guid("bdcb5194-b695-4ed9-b575-c5f5f3b1304e"), "write", "Criar ou editar papéis técnicos", null, false, "Role" },
                    { new Guid("c95b0ca8-63e9-4363-b440-a7b8525bf6e4"), "write", "Criar ou editar grupos de permissão", null, false, "PermissionGroup" },
                    { new Guid("e1bacfeb-75a4-48e1-beed-66a6fa0a68a5"), "read", "Consultar exceções individuais", null, false, "UserPermissionOverride" },
                    { new Guid("f4c0a499-dcb4-47fb-9928-7be9e2dfe385"), "write", "Criar novas versões de política condicional", null, false, "AccessPolicy" },
                    { new Guid("f6e45a8b-b685-4115-ab83-50d7bd72c787"), "write", "Criar contas e alterar seu estado", null, false, "AdminUser" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_usersession_user_id_revoked_at_utc",
                table: "usersession",
                columns: new[] { "user_id", "revoked_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usersession");

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("02ceffd4-f764-4916-94a3-5a45db960bf2"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("16402be8-d47d-4394-a6d5-1f0f48c6a1b5"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("16e1a13e-4fce-4b23-adfc-d3cbe098d3d2"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("188d0a43-405d-4c12-b520-8c4c8f770552"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("1a5beb30-0976-46ae-8b8a-43b74c869675"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("1f5c759b-2c24-41c5-aaaa-d011a45d6e26"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("20eff295-7744-4a5e-86e1-383185bd7dd8"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("21344fa6-54a6-4dc9-9696-eee38bc42680"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("27b676e3-c59f-496a-accc-e2e6a85a5b37"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("339fa802-d1b2-4b92-9737-78695f2dc0b9"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("467dcc23-d406-4af0-8110-e35054d2b0a7"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("4acd0f81-38b7-44a3-b082-1eabfc938272"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("5190a83c-a319-4b8d-bb49-d9a758f03c39"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("74cd4700-c774-4f08-b525-824397055d0e"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("7ce48309-c3f1-4826-8fac-890d425ba9f0"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("93c69a5f-c382-40f8-8e94-c56c7c890721"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("a5d9432f-b6e2-44a1-8a4a-0736fee1b740"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("aec7d56c-388a-40cc-8302-31e03b336773"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("ba85fad3-3ca2-4c97-9c6e-47473e6dd9f4"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("bdcb5194-b695-4ed9-b575-c5f5f3b1304e"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("c95b0ca8-63e9-4363-b440-a7b8525bf6e4"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("e1bacfeb-75a4-48e1-beed-66a6fa0a68a5"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("f4c0a499-dcb4-47fb-9928-7be9e2dfe385"));

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: new Guid("f6e45a8b-b685-4115-ab83-50d7bd72c787"));

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "role");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "permissiongroup");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "organizationalrole");

            migrationBuilder.DropColumn(
                name: "access_token_duration_minutes",
                table: "institution");

            migrationBuilder.DropColumn(
                name: "lockout_duration_minutes",
                table: "institution");

            migrationBuilder.DropColumn(
                name: "max_failed_attempts",
                table: "institution");

            migrationBuilder.DropColumn(
                name: "mfa_required_for_all_users",
                table: "institution");

            migrationBuilder.DropColumn(
                name: "refresh_token_duration_days",
                table: "institution");
        }
    }
}
