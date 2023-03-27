using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasinoGodsAPI.Migrations
{
    /// <inheritdoc />
    public partial class ActivePlayersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "jwtCreated",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "jwtExpires",
                table: "Players");

            migrationBuilder.CreateTable(
                name: "ActivePlayersTable",
                columns: table => new
                {
                    username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    bankroll = table.Column<int>(type: "int", nullable: false),
                    profit = table.Column<int>(type: "int", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    jwtExpires = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivePlayersTable", x => x.username);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivePlayersTable");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "jwtCreated",
                table: "Players",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "jwtExpires",
                table: "Players",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
