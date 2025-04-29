using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePostEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tokens",
                schema: "public");

            migrationBuilder.AddColumn<string>(
                name: "contact_number",
                schema: "public",
                table: "posts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<string>>(
                name: "image_urls",
                schema: "public",
                table: "posts",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "otp",
                schema: "public",
                table: "posts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "otp_expiration_time",
                schema: "public",
                table: "posts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "contact_number",
                schema: "public",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "image_urls",
                schema: "public",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "otp",
                schema: "public",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "otp_expiration_time",
                schema: "public",
                table: "posts");

            migrationBuilder.CreateTable(
                name: "tokens",
                schema: "public",
                columns: table => new
                {
                    token_id = table.Column<string>(type: "text", nullable: false),
                    expiration_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tokens", x => x.token_id);
                });
        }
    }
}
