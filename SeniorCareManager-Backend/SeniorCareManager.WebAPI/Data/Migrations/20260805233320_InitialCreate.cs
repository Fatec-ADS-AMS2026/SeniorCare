using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SeniorCareManager.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carrier",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    corporatename = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tradename = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cpfcnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    street = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    number = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    district = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    addresscomplement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    postalcode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    phone = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carrier", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "healthinsuranceplan",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<int>(type: "integer", maxLength: 1, nullable: false),
                    abbreviation = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_healthinsuranceplan", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "manufacturer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    corporate_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tradename = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cpf_cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    phone = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manufacturer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "position",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_position", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "productgroup",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productgroup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "religion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_religion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "supplier",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    corporate_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    trade_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cpf_cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    phone = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    street = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    number = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    district = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    address_complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "unitofmeasure",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    abbreviation = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unitofmeasure", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "producttype",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    productgroupid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producttype", x => x.id);
                    table.ForeignKey(
                        name: "FK_producttype_productgroup_productgroupid",
                        column: x => x.productgroupid,
                        principalTable: "productgroup",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "carrier",
                columns: new[] { "id", "addresscomplement", "city", "corporatename", "cpfcnpj", "district", "email", "number", "phone", "postalcode", "state", "street", "tradename" },
                values: new object[,]
                {
                    { 1, "Próximo ao banco", "São Paulo", "Transportes ABC LTDA", "12345678000190", "Centro", "contato@abctransportes.com", "123", "11987654321", "01001000", "SP", "Rua das Flores", "ABC Transportes" },
                    { 2, "Esquina com a Rua Augusta", "São Paulo", "Expresso XYZ S/A", "98765432000180", "Bela Vista", "expresso@xyz.com.br", "456", "11976543210", "01311000", "SP", "Avenida Paulista", "Expresso XYZ" },
                    { 3, "Próximo ao metrô", "Rio de Janeiro", "Translogística EFG ME", "22334455000122", "Centro", "contato@efgtrans.com.br", "789", "21987654321", "20040001", "RJ", "Avenida Rio Branco", "EFG Transportes" }
                });

            migrationBuilder.InsertData(
                table: "healthinsuranceplan",
                columns: new[] { "id", "abbreviation", "name", "type" },
                values: new object[,]
                {
                    { 1, "UNI", "Unimed", 2 },
                    { 2, "HAP", "Hapvida", 2 },
                    { 3, "SUS", "Sistema Único de Saúde", 1 }
                });

            migrationBuilder.InsertData(
                table: "manufacturer",
                columns: new[] { "id", "corporate_name", "cpf_cnpj", "email", "phone", "tradename" },
                values: new object[,]
                {
                    { 1, "Empresa A", "12345678000195", "contato@empresaa.com", "12345678901", "Trade A" },
                    { 2, "Empresa B", "12345678000196", "contato@empresab.com", "23456789012", "Trade B" },
                    { 3, "Empresa C", "12345678000197", "contato@empresac.com", "34567890123", "Trade C" }
                });

            migrationBuilder.InsertData(
                table: "position",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Enfermeiros" },
                    { 2, "Cuidadores" },
                    { 3, "Cozinheiro" },
                    { 4, "Administrador" },
                    { 5, "Nutricionista" }
                });

            migrationBuilder.InsertData(
                table: "productgroup",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Medicamentos" },
                    { 2, "Equipamentos Médicos" },
                    { 3, "Suplementos" }
                });

            migrationBuilder.InsertData(
                table: "religion",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Católico" },
                    { 2, "Evangelico" },
                    { 3, "Ateu" }
                });

            migrationBuilder.InsertData(
                table: "unitofmeasure",
                columns: new[] { "id", "abbreviation", "description" },
                values: new object[,]
                {
                    { 1, "kg", "Kilogram" },
                    { 2, "m", "Meter" },
                    { 3, "l", "Liter" }
                });

            migrationBuilder.InsertData(
                table: "producttype",
                columns: new[] { "id", "name", "productgroupid" },
                values: new object[,]
                {
                    { 1, "Legumes", 3 },
                    { 2, "Carnes", 3 },
                    { 3, "Frutas", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_producttype_productgroupid",
                table: "producttype",
                column: "productgroupid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "carrier");

            migrationBuilder.DropTable(
                name: "healthinsuranceplan");

            migrationBuilder.DropTable(
                name: "manufacturer");

            migrationBuilder.DropTable(
                name: "position");

            migrationBuilder.DropTable(
                name: "producttype");

            migrationBuilder.DropTable(
                name: "religion");

            migrationBuilder.DropTable(
                name: "supplier");

            migrationBuilder.DropTable(
                name: "unitofmeasure");

            migrationBuilder.DropTable(
                name: "productgroup");
        }
    }
}
