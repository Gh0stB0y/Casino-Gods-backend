using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasinoGodsAPI.Migrations
{
    /// <inheritdoc />
    public partial class updatedTablesNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePlusPlayersTable_GamesList_GameNameName",
                table: "GamePlusPlayersTable");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePlusPlayersTable_Players_PlayerId",
                table: "GamePlusPlayersTable");

            migrationBuilder.DropForeignKey(
                name: "FK_TablesList_GamesList_CKGame",
                table: "TablesList");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TablesList",
                table: "TablesList");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GamesList",
                table: "GamesList");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GamePlusPlayersTable",
                table: "GamePlusPlayersTable");

            migrationBuilder.RenameTable(
                name: "TablesList",
                newName: "Tables");

            migrationBuilder.RenameTable(
                name: "GamesList",
                newName: "Games");

            migrationBuilder.RenameTable(
                name: "GamePlusPlayersTable",
                newName: "GamePlayersTable");

            migrationBuilder.RenameIndex(
                name: "IX_TablesList_CKGame",
                table: "Tables",
                newName: "IX_Tables_CKGame");

            migrationBuilder.RenameIndex(
                name: "IX_GamePlusPlayersTable_PlayerId",
                table: "GamePlayersTable",
                newName: "IX_GamePlayersTable_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePlusPlayersTable_GameNameName",
                table: "GamePlayersTable",
                newName: "IX_GamePlayersTable_GameNameName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tables",
                table: "Tables",
                columns: new[] { "CKname", "CKGame" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Games",
                table: "Games",
                column: "Name");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_Games_CKGame",
                table: "Tables",
                column: "CKGame",
                principalTable: "Games",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePlayersTable_Games_GameNameName",
                table: "GamePlayersTable");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePlayersTable_Players_PlayerId",
                table: "GamePlayersTable");

            migrationBuilder.DropForeignKey(
                name: "FK_Tables_Games_CKGame",
                table: "Tables");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tables",
                table: "Tables");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Games",
                table: "Games");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GamePlayersTable",
                table: "GamePlayersTable");

            migrationBuilder.RenameTable(
                name: "Tables",
                newName: "TablesList");

            migrationBuilder.RenameTable(
                name: "Games",
                newName: "GamesList");

            migrationBuilder.RenameTable(
                name: "GamePlayersTable",
                newName: "GamePlusPlayersTable");

            migrationBuilder.RenameIndex(
                name: "IX_Tables_CKGame",
                table: "TablesList",
                newName: "IX_TablesList_CKGame");

            migrationBuilder.RenameIndex(
                name: "IX_GamePlayersTable_PlayerId",
                table: "GamePlusPlayersTable",
                newName: "IX_GamePlusPlayersTable_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePlayersTable_GameNameName",
                table: "GamePlusPlayersTable",
                newName: "IX_GamePlusPlayersTable_GameNameName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TablesList",
                table: "TablesList",
                columns: new[] { "CKname", "CKGame" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GamesList",
                table: "GamesList",
                column: "Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GamePlusPlayersTable",
                table: "GamePlusPlayersTable",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlusPlayersTable_GamesList_GameNameName",
                table: "GamePlusPlayersTable",
                column: "GameNameName",
                principalTable: "GamesList",
                principalColumn: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlusPlayersTable_Players_PlayerId",
                table: "GamePlusPlayersTable",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TablesList_GamesList_CKGame",
                table: "TablesList",
                column: "CKGame",
                principalTable: "GamesList",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
