using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasinoGodsAPI.Migrations
{
    /// <inheritdoc />
    public partial class updatedTablesNames2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePlayersTable_Games_GameNameName",
                table: "GamePlayersTable");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePlayersTable_Players_PlayerId",
                table: "GamePlayersTable");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GamePlayersTable",
                table: "GamePlayersTable");

            migrationBuilder.RenameTable(
                name: "GamePlayersTable",
                newName: "GamePlayers");

            migrationBuilder.RenameIndex(
                name: "IX_GamePlayersTable_PlayerId",
                table: "GamePlayers",
                newName: "IX_GamePlayers_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePlayersTable_GameNameName",
                table: "GamePlayers",
                newName: "IX_GamePlayers_GameNameName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GamePlayers",
                table: "GamePlayers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlayers_Games_GameNameName",
                table: "GamePlayers",
                column: "GameNameName",
                principalTable: "Games",
                principalColumn: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlayers_Players_PlayerId",
                table: "GamePlayers",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePlayers_Games_GameNameName",
                table: "GamePlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePlayers_Players_PlayerId",
                table: "GamePlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GamePlayers",
                table: "GamePlayers");

            migrationBuilder.RenameTable(
                name: "GamePlayers",
                newName: "GamePlayersTable");

            migrationBuilder.RenameIndex(
                name: "IX_GamePlayers_PlayerId",
                table: "GamePlayersTable",
                newName: "IX_GamePlayersTable_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePlayers_GameNameName",
                table: "GamePlayersTable",
                newName: "IX_GamePlayersTable_GameNameName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GamePlayersTable",
                table: "GamePlayersTable",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlayersTable_Games_GameNameName",
                table: "GamePlayersTable",
                column: "GameNameName",
                principalTable: "Games",
                principalColumn: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlayersTable_Players_PlayerId",
                table: "GamePlayersTable",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id");
        }
    }
}
