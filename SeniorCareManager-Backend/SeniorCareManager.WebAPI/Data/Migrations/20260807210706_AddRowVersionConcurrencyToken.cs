using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeniorCareManager.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "unitofmeasure",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "supplier",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "religion",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "producttype",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "productgroup",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "position",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "manufacturer",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "healthinsuranceplan",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "carrier",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "unitofmeasure");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "supplier");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "religion");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "producttype");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "productgroup");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "position");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "manufacturer");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "healthinsuranceplan");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "carrier");
        }
    }
}
