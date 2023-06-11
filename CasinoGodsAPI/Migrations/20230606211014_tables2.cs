using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasinoGodsAPI.Migrations
{
    /// <inheritdoc />
    public partial class tables2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TablesList_GamesList_gameName",
                table: "TablesList");

            migrationBuilder.RenameColumn(
                name: "gameName",
                table: "TablesList",
                newName: "game222Name");

            migrationBuilder.RenameIndex(
                name: "IX_TablesList_gameName",
                table: "TablesList",
                newName: "IX_TablesList_game222Name");

            migrationBuilder.AddForeignKey(
                name: "FK_TablesList_GamesList_game222Name",
                table: "TablesList",
                column: "game222Name",
                principalTable: "GamesList",
                principalColumn: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TablesList_GamesList_game222Name",
                table: "TablesList");

            migrationBuilder.RenameColumn(
                name: "game222Name",
                table: "TablesList",
                newName: "gameName");

            migrationBuilder.RenameIndex(
                name: "IX_TablesList_game222Name",
                table: "TablesList",
                newName: "IX_TablesList_gameName");

            migrationBuilder.AddForeignKey(
                name: "FK_TablesList_GamesList_gameName",
                table: "TablesList",
                column: "gameName",
                principalTable: "GamesList",
                principalColumn: "Name");
        }
    }
}
