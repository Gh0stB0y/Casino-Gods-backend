using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasinoGodsAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangePlayerClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BJ",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "draws",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "gamesPlayed",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "loses",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "wins",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BJ",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "draws",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "gamesPlayed",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "loses",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "wins",
                table: "Players");
        }
    }
}
