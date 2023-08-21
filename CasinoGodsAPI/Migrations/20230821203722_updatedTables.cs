using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasinoGodsAPI.Migrations
{
    /// <inheritdoc />
    public partial class updatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePlusPlayersTable_GamesList_gameNameName",
                table: "GamePlusPlayersTable");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePlusPlayersTable_Players_playerId",
                table: "GamePlusPlayersTable");

            migrationBuilder.DropTable(
                name: "LobbyTableData");

            migrationBuilder.RenameColumn(
                name: "sidebet2",
                table: "TablesList",
                newName: "Sidebet2");

            migrationBuilder.RenameColumn(
                name: "sidebet1",
                table: "TablesList",
                newName: "Sidebet1");

            migrationBuilder.RenameColumn(
                name: "minBet",
                table: "TablesList",
                newName: "MinBet");

            migrationBuilder.RenameColumn(
                name: "maxseats",
                table: "TablesList",
                newName: "Maxseats");

            migrationBuilder.RenameColumn(
                name: "maxBet",
                table: "TablesList",
                newName: "MaxBet");

            migrationBuilder.RenameColumn(
                name: "decks",
                table: "TablesList",
                newName: "Decks");

            migrationBuilder.RenameColumn(
                name: "betTime",
                table: "TablesList",
                newName: "BetTime");

            migrationBuilder.RenameColumn(
                name: "actionTime",
                table: "TablesList",
                newName: "ActionTime");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Players",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "profit",
                table: "Players",
                newName: "Profit");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Players",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "passSalt",
                table: "Players",
                newName: "PassSalt");

            migrationBuilder.RenameColumn(
                name: "passHash",
                table: "Players",
                newName: "PassHash");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Players",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "birthdate",
                table: "Players",
                newName: "Birthdate");

            migrationBuilder.RenameColumn(
                name: "bankroll",
                table: "Players",
                newName: "Bankroll");

            migrationBuilder.RenameColumn(
                name: "wins",
                table: "GamePlusPlayersTable",
                newName: "Wins");

            migrationBuilder.RenameColumn(
                name: "winratio",
                table: "GamePlusPlayersTable",
                newName: "Winratio");

            migrationBuilder.RenameColumn(
                name: "profit",
                table: "GamePlusPlayersTable",
                newName: "Profit");

            migrationBuilder.RenameColumn(
                name: "playerId",
                table: "GamePlusPlayersTable",
                newName: "PlayerId");

            migrationBuilder.RenameColumn(
                name: "loses",
                table: "GamePlusPlayersTable",
                newName: "Loses");

            migrationBuilder.RenameColumn(
                name: "gamesPlayed",
                table: "GamePlusPlayersTable",
                newName: "GamesPlayed");

            migrationBuilder.RenameColumn(
                name: "gameNameName",
                table: "GamePlusPlayersTable",
                newName: "GameNameName");

            migrationBuilder.RenameColumn(
                name: "draws",
                table: "GamePlusPlayersTable",
                newName: "Draws");

            migrationBuilder.RenameIndex(
                name: "IX_GamePlusPlayersTable_playerId",
                table: "GamePlusPlayersTable",
                newName: "IX_GamePlusPlayersTable_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePlusPlayersTable_gameNameName",
                table: "GamePlusPlayersTable",
                newName: "IX_GamePlusPlayersTable_GameNameName");

            migrationBuilder.RenameColumn(
                name: "sidebet2",
                table: "ActiveTables",
                newName: "Sidebet2");

            migrationBuilder.RenameColumn(
                name: "sidebet1",
                table: "ActiveTables",
                newName: "Sidebet1");

            migrationBuilder.RenameColumn(
                name: "minBet",
                table: "ActiveTables",
                newName: "MinBet");

            migrationBuilder.RenameColumn(
                name: "maxseats",
                table: "ActiveTables",
                newName: "Maxseats");

            migrationBuilder.RenameColumn(
                name: "maxBet",
                table: "ActiveTables",
                newName: "MaxBet");

            migrationBuilder.RenameColumn(
                name: "decks",
                table: "ActiveTables",
                newName: "Decks");

            migrationBuilder.RenameColumn(
                name: "betTime",
                table: "ActiveTables",
                newName: "BetTime");

            migrationBuilder.RenameColumn(
                name: "actionTime",
                table: "ActiveTables",
                newName: "ActionTime");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePlusPlayersTable_GamesList_GameNameName",
                table: "GamePlusPlayersTable");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePlusPlayersTable_Players_PlayerId",
                table: "GamePlusPlayersTable");

            migrationBuilder.RenameColumn(
                name: "Sidebet2",
                table: "TablesList",
                newName: "sidebet2");

            migrationBuilder.RenameColumn(
                name: "Sidebet1",
                table: "TablesList",
                newName: "sidebet1");

            migrationBuilder.RenameColumn(
                name: "MinBet",
                table: "TablesList",
                newName: "minBet");

            migrationBuilder.RenameColumn(
                name: "Maxseats",
                table: "TablesList",
                newName: "maxseats");

            migrationBuilder.RenameColumn(
                name: "MaxBet",
                table: "TablesList",
                newName: "maxBet");

            migrationBuilder.RenameColumn(
                name: "Decks",
                table: "TablesList",
                newName: "decks");

            migrationBuilder.RenameColumn(
                name: "BetTime",
                table: "TablesList",
                newName: "betTime");

            migrationBuilder.RenameColumn(
                name: "ActionTime",
                table: "TablesList",
                newName: "actionTime");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Players",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Profit",
                table: "Players",
                newName: "profit");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Players",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "PassSalt",
                table: "Players",
                newName: "passSalt");

            migrationBuilder.RenameColumn(
                name: "PassHash",
                table: "Players",
                newName: "passHash");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Players",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Birthdate",
                table: "Players",
                newName: "birthdate");

            migrationBuilder.RenameColumn(
                name: "Bankroll",
                table: "Players",
                newName: "bankroll");

            migrationBuilder.RenameColumn(
                name: "Wins",
                table: "GamePlusPlayersTable",
                newName: "wins");

            migrationBuilder.RenameColumn(
                name: "Winratio",
                table: "GamePlusPlayersTable",
                newName: "winratio");

            migrationBuilder.RenameColumn(
                name: "Profit",
                table: "GamePlusPlayersTable",
                newName: "profit");

            migrationBuilder.RenameColumn(
                name: "PlayerId",
                table: "GamePlusPlayersTable",
                newName: "playerId");

            migrationBuilder.RenameColumn(
                name: "Loses",
                table: "GamePlusPlayersTable",
                newName: "loses");

            migrationBuilder.RenameColumn(
                name: "GamesPlayed",
                table: "GamePlusPlayersTable",
                newName: "gamesPlayed");

            migrationBuilder.RenameColumn(
                name: "GameNameName",
                table: "GamePlusPlayersTable",
                newName: "gameNameName");

            migrationBuilder.RenameColumn(
                name: "Draws",
                table: "GamePlusPlayersTable",
                newName: "draws");

            migrationBuilder.RenameIndex(
                name: "IX_GamePlusPlayersTable_PlayerId",
                table: "GamePlusPlayersTable",
                newName: "IX_GamePlusPlayersTable_playerId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePlusPlayersTable_GameNameName",
                table: "GamePlusPlayersTable",
                newName: "IX_GamePlusPlayersTable_gameNameName");

            migrationBuilder.RenameColumn(
                name: "Sidebet2",
                table: "ActiveTables",
                newName: "sidebet2");

            migrationBuilder.RenameColumn(
                name: "Sidebet1",
                table: "ActiveTables",
                newName: "sidebet1");

            migrationBuilder.RenameColumn(
                name: "MinBet",
                table: "ActiveTables",
                newName: "minBet");

            migrationBuilder.RenameColumn(
                name: "Maxseats",
                table: "ActiveTables",
                newName: "maxseats");

            migrationBuilder.RenameColumn(
                name: "MaxBet",
                table: "ActiveTables",
                newName: "maxBet");

            migrationBuilder.RenameColumn(
                name: "Decks",
                table: "ActiveTables",
                newName: "decks");

            migrationBuilder.RenameColumn(
                name: "BetTime",
                table: "ActiveTables",
                newName: "betTime");

            migrationBuilder.RenameColumn(
                name: "ActionTime",
                table: "ActiveTables",
                newName: "actionTime");

            migrationBuilder.CreateTable(
                name: "LobbyTableData",
                columns: table => new
                {
                    TableInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableTypeCKname = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TableTypeCKGame = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TablePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LobbyTableData", x => x.TableInstanceId);
                    table.ForeignKey(
                        name: "FK_LobbyTableData_TablesList_TableTypeCKname_TableTypeCKGame",
                        columns: x => new { x.TableTypeCKname, x.TableTypeCKGame },
                        principalTable: "TablesList",
                        principalColumns: new[] { "CKname", "CKGame" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_LobbyTableData_TableTypeCKname_TableTypeCKGame",
                table: "LobbyTableData",
                columns: new[] { "TableTypeCKname", "TableTypeCKGame" });

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlusPlayersTable_GamesList_gameNameName",
                table: "GamePlusPlayersTable",
                column: "gameNameName",
                principalTable: "GamesList",
                principalColumn: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlusPlayersTable_Players_playerId",
                table: "GamePlusPlayersTable",
                column: "playerId",
                principalTable: "Players",
                principalColumn: "Id");
        }
    }
}
