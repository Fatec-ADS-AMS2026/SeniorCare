using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SeniorCareManager.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    generic_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    product_type_id = table.Column<int>(type: "integer", nullable: false),
                    unit_of_measure_id = table.Column<int>(type: "integer", nullable: false),
                    minimum_stock = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    current_stock = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    average_cost = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    last_purchase_price = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    stock_value = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    high_cost = table.Column<bool>(type: "boolean", nullable: false),
                    expiration_controlled = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_producttype_product_type_id",
                        column: x => x.product_type_id,
                        principalTable: "producttype",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_unitofmeasure_unit_of_measure_id",
                        column: x => x.unit_of_measure_id,
                        principalTable: "unitofmeasure",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_description",
                table: "product",
                column: "description");

            migrationBuilder.CreateIndex(
                name: "IX_product_product_type_id",
                table: "product",
                column: "product_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_unit_of_measure_id",
                table: "product",
                column: "unit_of_measure_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product");
        }
    }
}
