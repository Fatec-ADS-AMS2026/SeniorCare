using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeniorCareManager.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionAndMfa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MfaEnabled",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "current_key_hash",
                table: "usersession",
                type: "character varying(88)",
                maxLength: 88,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "expires_at_utc",
                table: "usersession",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "last_rotated_at_utc",
                table: "usersession",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "previous_key_hash",
                table: "usersession",
                type: "character varying(88)",
                maxLength: 88,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "current_key_hash",
                table: "usersession");

            migrationBuilder.DropColumn(
                name: "expires_at_utc",
                table: "usersession");

            migrationBuilder.DropColumn(
                name: "last_rotated_at_utc",
                table: "usersession");

            migrationBuilder.DropColumn(
                name: "previous_key_hash",
                table: "usersession");

            migrationBuilder.AddColumn<bool>(
                name: "MfaEnabled",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
