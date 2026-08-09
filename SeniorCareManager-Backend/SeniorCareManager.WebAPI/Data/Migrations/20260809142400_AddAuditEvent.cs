using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeniorCareManager.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_event",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    feature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    target_scope_type = table.Column<int>(type: "integer", nullable: true),
                    target_scope_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    outcome = table.Column<int>(type: "integer", nullable: false),
                    determining_layer = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    before_value = table.Column<string>(type: "jsonb", nullable: true),
                    after_value = table.Column<string>(type: "jsonb", nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_event", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_event_correlation_id",
                table: "audit_event",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_event_institution_id_occurred_at_utc",
                table: "audit_event",
                columns: new[] { "institution_id", "occurred_at_utc" });

            // §8.6, segunda camada de imutabilidade: rejeita UPDATE/DELETE no próprio Postgres,
            // sobrevivendo a um acesso direto ao banco (fora da API/EF) ou a um bug futuro que
            // bypasse o AuditImmutabilityInterceptor. Primeira migração do projeto com SQL bruto
            // — todas as anteriores usam só a API fluente do EF.
            migrationBuilder.Sql(@"
                CREATE FUNCTION audit_event_block_mutation() RETURNS trigger AS $$
                BEGIN
                    RAISE EXCEPTION 'audit_event é append-only: % não é permitido', TG_OP;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER audit_event_immutable
                BEFORE UPDATE OR DELETE ON audit_event
                FOR EACH ROW EXECUTE FUNCTION audit_event_block_mutation();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS audit_event_immutable ON audit_event;
                DROP FUNCTION IF EXISTS audit_event_block_mutation();
            ");

            migrationBuilder.DropTable(
                name: "audit_event");
        }
    }
}
