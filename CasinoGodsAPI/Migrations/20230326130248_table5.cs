using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasinoGodsAPI.Migrations
{
    /// <inheritdoc />
    public partial class table5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "draws",
                table: "GamePlusPlayersTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "gamesPlayed",
                table: "GamePlusPlayersTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "loses",
                table: "GamePlusPlayersTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "profit",
                table: "GamePlusPlayersTable",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "winratio",
                table: "GamePlusPlayersTable",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "wins",
                table: "GamePlusPlayersTable",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "draws",
                table: "GamePlusPlayersTable");

            migrationBuilder.DropColumn(
                name: "gamesPlayed",
                table: "GamePlusPlayersTable");

            migrationBuilder.DropColumn(
                name: "loses",
                table: "GamePlusPlayersTable");

            migrationBuilder.DropColumn(
                name: "profit",
                table: "GamePlusPlayersTable");

            migrationBuilder.DropColumn(
                name: "winratio",
                table: "GamePlusPlayersTable");

            migrationBuilder.DropColumn(
                name: "wins",
                table: "GamePlusPlayersTable");
        }
    }
}
