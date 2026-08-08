using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SeniorCareManager.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessControlFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystemAdmin",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "accesspolicy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_key = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    feature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    scope_type = table.Column<int>(type: "integer", nullable: true),
                    scope_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    effect = table.Column<int>(type: "integer", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accesspolicy", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organizationalrole",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizationalrole", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permission",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    feature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_system_operation = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permission", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissiongroup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissiongroup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "userpermissionoverride",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    feature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    scope_type = table.Column<int>(type: "integer", nullable: true),
                    scope_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    effect = table.Column<int>(type: "integer", nullable: false),
                    justification = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    granted_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userpermissionoverride", x => x.id);
                    table.ForeignKey(
                        name: "FK_userpermissionoverride_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organizationalroleassignment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organizational_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_type = table.Column<int>(type: "integer", nullable: false),
                    scope_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizationalroleassignment", x => x.id);
                    table.ForeignKey(
                        name: "FK_organizationalroleassignment_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organizationalroleassignment_organizationalrole_organizatio~",
                        column: x => x.organizational_role_id,
                        principalTable: "organizationalrole",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organizationalrolepermissiongroup",
                columns: table => new
                {
                    organizational_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_group_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizationalrolepermissiongroup", x => new { x.organizational_role_id, x.permission_group_id });
                    table.ForeignKey(
                        name: "FK_organizationalrolepermissiongroup_organizationalrole_organi~",
                        column: x => x.organizational_role_id,
                        principalTable: "organizationalrole",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organizationalrolepermissiongroup_permissiongroup_permissio~",
                        column: x => x.permission_group_id,
                        principalTable: "permissiongroup",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "permissiongrouppermission",
                columns: table => new
                {
                    permission_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissiongrouppermission", x => new { x.permission_group_id, x.permission_id });
                    table.ForeignKey(
                        name: "FK_permissiongrouppermission_permission_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permission",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_permissiongrouppermission_permissiongroup_permission_group_~",
                        column: x => x.permission_group_id,
                        principalTable: "permissiongroup",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rolepermissiongroup",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_group_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rolepermissiongroup", x => new { x.role_id, x.permission_group_id });
                    table.ForeignKey(
                        name: "FK_rolepermissiongroup_permissiongroup_permission_group_id",
                        column: x => x.permission_group_id,
                        principalTable: "permissiongroup",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rolepermissiongroup_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userrole",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userrole", x => x.id);
                    table.ForeignKey(
                        name: "FK_userrole_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userrole_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "permission",
                columns: new[] { "id", "action", "description", "feature", "is_system_operation", "resource" },
                values: new object[,]
                {
                    { new Guid("111491e2-dfe4-44ba-a791-70ea17930a56"), "delete", "Excluir/inativar ProductType", null, false, "ProductType" },
                    { new Guid("12512f07-e33c-4e91-bb92-5b2b22e389e6"), "delete", "Excluir/inativar HealthInsurancePlan", null, false, "HealthInsurancePlan" },
                    { new Guid("1b0ebb43-0aa5-449a-9246-d25290c80bc2"), "write", "Criar ou editar Carrier", null, false, "Carrier" },
                    { new Guid("1d065485-c1f5-4e68-af1b-97aae10d837d"), "read", "Consultar ProductType", null, false, "ProductType" },
                    { new Guid("27725187-00d6-47ab-a39e-9bfddcf7a039"), "read", "Consultar Carrier", null, false, "Carrier" },
                    { new Guid("28913e62-e1c2-4e04-8559-202c895507db"), "delete", "Excluir/inativar ProductGroup", null, false, "ProductGroup" },
                    { new Guid("2be2a833-9f65-4cdc-a721-c54aa6c1a606"), "read", "Consultar Religion", null, false, "Religion" },
                    { new Guid("325e6bf1-7d8d-4faf-ab66-216b6ff53012"), "delete", "Excluir/inativar Manufacturer", null, false, "Manufacturer" },
                    { new Guid("3b31d0c5-b52c-43a0-a638-959df4e76e9b"), "write", "Criar ou editar ProductGroup", null, false, "ProductGroup" },
                    { new Guid("3dc837df-49d2-4e29-bc50-9b83c211569c"), "write", "Criar ou editar ProductType", null, false, "ProductType" },
                    { new Guid("4822d058-ab3c-4233-9d72-2e06515cf06a"), "read", "Consultar ProductGroup", null, false, "ProductGroup" },
                    { new Guid("5d767431-4c88-4a96-a7af-5b27338c4078"), "write", "Criar ou editar Manufacturer", null, false, "Manufacturer" },
                    { new Guid("80d2bc17-66d5-4f08-a7a0-7123b0556db7"), "write", "Criar ou editar Supplier", null, false, "Supplier" },
                    { new Guid("89eb36b4-f5ff-4ddf-9cc2-875f4ff80f9e"), "delete", "Excluir/inativar UnitOfMeasure", null, false, "UnitOfMeasure" },
                    { new Guid("8e6d9b0d-2adb-48da-a8bf-63c6fc775ddc"), "read", "Consultar Position", null, false, "Position" },
                    { new Guid("91cd749c-2d6e-453f-8d94-991f7f422300"), "delete", "Excluir/inativar Position", null, false, "Position" },
                    { new Guid("91ede0cb-5492-4e1f-a5aa-67edd6eeb231"), "write", "Criar ou editar HealthInsurancePlan", null, false, "HealthInsurancePlan" },
                    { new Guid("ab0a7b57-9d6d-4110-85e7-c001f16072fc"), "write", "Criar ou editar Position", null, false, "Position" },
                    { new Guid("b9efb981-e8e0-4d44-a45f-89ef7218fa34"), "read", "Consultar HealthInsurancePlan", null, false, "HealthInsurancePlan" },
                    { new Guid("bdffd7c8-9ce2-4faa-86ac-939116ac0e87"), "read", "Consultar Manufacturer", null, false, "Manufacturer" },
                    { new Guid("bf087d0a-1912-4839-962f-784e232319ab"), "read", "Consultar UnitOfMeasure", null, false, "UnitOfMeasure" },
                    { new Guid("c479d380-4550-4c6f-acde-30ba2ad0123e"), "read", "Consultar Supplier", null, false, "Supplier" },
                    { new Guid("d099399d-f909-4346-b649-1c45d22ae870"), "delete", "Excluir/inativar Supplier", null, false, "Supplier" },
                    { new Guid("d1e9b982-cc0f-4194-8d95-09db6cf51bd6"), "write", "Criar ou editar UnitOfMeasure", null, false, "UnitOfMeasure" },
                    { new Guid("e8e754a2-4cb9-461f-a773-3ce9adbb4ecb"), "delete", "Excluir/inativar Carrier", null, false, "Carrier" },
                    { new Guid("f7a4145c-e73e-4297-ba84-acb35d392478"), "delete", "Excluir/inativar Religion", null, false, "Religion" },
                    { new Guid("fc15ca1c-1a6c-4451-926d-d4e746862b93"), "write", "Criar ou editar Religion", null, false, "Religion" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_accesspolicy_institution_id_resource_action_feature_state",
                table: "accesspolicy",
                columns: new[] { "institution_id", "resource", "action", "feature", "state" });

            migrationBuilder.CreateIndex(
                name: "IX_accesspolicy_policy_key_version",
                table: "accesspolicy",
                columns: new[] { "policy_key", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organizationalrole_institution_id_name",
                table: "organizationalrole",
                columns: new[] { "institution_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organizationalroleassignment_organizational_role_id",
                table: "organizationalroleassignment",
                column: "organizational_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_organizationalroleassignment_user_id_organizational_role_id~",
                table: "organizationalroleassignment",
                columns: new[] { "user_id", "organizational_role_id", "valid_from" });

            migrationBuilder.CreateIndex(
                name: "IX_organizationalrolepermissiongroup_permission_group_id",
                table: "organizationalrolepermissiongroup",
                column: "permission_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_permission_resource_action_feature",
                table: "permission",
                columns: new[] { "resource", "action", "feature" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_permissiongrouppermission_permission_id",
                table: "permissiongrouppermission",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_institution_id_name",
                table: "role",
                columns: new[] { "institution_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rolepermissiongroup_permission_group_id",
                table: "rolepermissiongroup",
                column: "permission_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_userpermissionoverride_user_id_resource_action_feature",
                table: "userpermissionoverride",
                columns: new[] { "user_id", "resource", "action", "feature" });

            migrationBuilder.CreateIndex(
                name: "IX_userrole_role_id",
                table: "userrole",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_userrole_user_id_role_id",
                table: "userrole",
                columns: new[] { "user_id", "role_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accesspolicy");

            migrationBuilder.DropTable(
                name: "organizationalroleassignment");

            migrationBuilder.DropTable(
                name: "organizationalrolepermissiongroup");

            migrationBuilder.DropTable(
                name: "permissiongrouppermission");

            migrationBuilder.DropTable(
                name: "rolepermissiongroup");

            migrationBuilder.DropTable(
                name: "userpermissionoverride");

            migrationBuilder.DropTable(
                name: "userrole");

            migrationBuilder.DropTable(
                name: "organizationalrole");

            migrationBuilder.DropTable(
                name: "permission");

            migrationBuilder.DropTable(
                name: "permissiongroup");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropColumn(
                name: "IsSystemAdmin",
                table: "AspNetUsers");
        }
    }
}
