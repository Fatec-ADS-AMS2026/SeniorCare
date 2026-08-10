using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeniorCareManager.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogActiveState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_producttype_productgroup_productgroupid",
                table: "producttype");

            migrationBuilder.DropIndex(
                name: "IX_producttype_productgroupid",
                table: "producttype");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "unitofmeasure",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "supplier",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "religion",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "producttype",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "productgroup",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "position",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "manufacturer",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "healthinsuranceplan",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "carrier",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "carrier",
                keyColumn: "id",
                keyValue: 1,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "carrier",
                keyColumn: "id",
                keyValue: 2,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "carrier",
                keyColumn: "id",
                keyValue: 3,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "healthinsuranceplan",
                keyColumn: "id",
                keyValue: 1,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "healthinsuranceplan",
                keyColumn: "id",
                keyValue: 2,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "healthinsuranceplan",
                keyColumn: "id",
                keyValue: 3,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "manufacturer",
                keyColumn: "id",
                keyValue: 1,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "manufacturer",
                keyColumn: "id",
                keyValue: 2,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "manufacturer",
                keyColumn: "id",
                keyValue: 3,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "position",
                keyColumn: "id",
                keyValue: 1,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "position",
                keyColumn: "id",
                keyValue: 2,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "position",
                keyColumn: "id",
                keyValue: 3,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "position",
                keyColumn: "id",
                keyValue: 4,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "position",
                keyColumn: "id",
                keyValue: 5,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "productgroup",
                keyColumn: "id",
                keyValue: 1,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "productgroup",
                keyColumn: "id",
                keyValue: 2,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "productgroup",
                keyColumn: "id",
                keyValue: 3,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "producttype",
                keyColumn: "id",
                keyValue: 1,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "producttype",
                keyColumn: "id",
                keyValue: 2,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "producttype",
                keyColumn: "id",
                keyValue: 3,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "religion",
                keyColumn: "id",
                keyValue: 1,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "religion",
                keyColumn: "id",
                keyValue: 2,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "religion",
                keyColumn: "id",
                keyValue: 3,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "unitofmeasure",
                keyColumn: "id",
                keyValue: 1,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "unitofmeasure",
                keyColumn: "id",
                keyValue: 2,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "unitofmeasure",
                keyColumn: "id",
                keyValue: 3,
                column: "is_active",
                value: true);

            migrationBuilder.CreateIndex(
                name: "IX_unitofmeasure_abbreviation",
                table: "unitofmeasure",
                column: "abbreviation",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplier_cpf_cnpj",
                table: "supplier",
                column: "cpf_cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_religion_name",
                table: "religion",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_producttype_productgroupid_name",
                table: "producttype",
                columns: new[] { "productgroupid", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_productgroup_name",
                table: "productgroup",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_position_name",
                table: "position",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_manufacturer_cpf_cnpj",
                table: "manufacturer",
                column: "cpf_cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_healthinsuranceplan_abbreviation",
                table: "healthinsuranceplan",
                column: "abbreviation",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_carrier_cpfcnpj",
                table: "carrier",
                column: "cpfcnpj",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_producttype_productgroup_productgroupid",
                table: "producttype",
                column: "productgroupid",
                principalTable: "productgroup",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_producttype_productgroup_productgroupid",
                table: "producttype");

            migrationBuilder.DropIndex(
                name: "IX_unitofmeasure_abbreviation",
                table: "unitofmeasure");

            migrationBuilder.DropIndex(
                name: "IX_supplier_cpf_cnpj",
                table: "supplier");

            migrationBuilder.DropIndex(
                name: "IX_religion_name",
                table: "religion");

            migrationBuilder.DropIndex(
                name: "IX_producttype_productgroupid_name",
                table: "producttype");

            migrationBuilder.DropIndex(
                name: "IX_productgroup_name",
                table: "productgroup");

            migrationBuilder.DropIndex(
                name: "IX_position_name",
                table: "position");

            migrationBuilder.DropIndex(
                name: "IX_manufacturer_cpf_cnpj",
                table: "manufacturer");

            migrationBuilder.DropIndex(
                name: "IX_healthinsuranceplan_abbreviation",
                table: "healthinsuranceplan");

            migrationBuilder.DropIndex(
                name: "IX_carrier_cpfcnpj",
                table: "carrier");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "unitofmeasure");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "supplier");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "religion");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "producttype");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "productgroup");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "position");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "manufacturer");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "healthinsuranceplan");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "carrier");

            migrationBuilder.CreateIndex(
                name: "IX_producttype_productgroupid",
                table: "producttype",
                column: "productgroupid");

            migrationBuilder.AddForeignKey(
                name: "FK_producttype_productgroup_productgroupid",
                table: "producttype",
                column: "productgroupid",
                principalTable: "productgroup",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
