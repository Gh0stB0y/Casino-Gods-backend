using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasinoGodsAPI.Migrations
{
    /// <inheritdoc />
    public partial class tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dealers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    profit = table.Column<float>(type: "real", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dealers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GamesList",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamesList", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bankroll = table.Column<int>(type: "int", nullable: false),
                    profit = table.Column<int>(type: "int", nullable: false),
                    birthdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    passSalt = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    passHash = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TablesList",
                columns: table => new
                {
                    name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    gameName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    minBet = table.Column<int>(type: "int", nullable: false),
                    maxBet = table.Column<int>(type: "int", nullable: false),
                    betTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TablesList", x => x.name);
                    table.ForeignKey(
                        name: "FK_TablesList_GamesList_gameName",
                        column: x => x.gameName,
                        principalTable: "GamesList",
                        principalColumn: "Name");
                });

            migrationBuilder.CreateTable(
                name: "GamePlusPlayersTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gameNameName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    playerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    gamesPlayed = table.Column<int>(type: "int", nullable: false),
                    wins = table.Column<int>(type: "int", nullable: false),
                    loses = table.Column<int>(type: "int", nullable: false),
                    draws = table.Column<int>(type: "int", nullable: false),
                    winratio = table.Column<float>(type: "real", nullable: false),
                    profit = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePlusPlayersTable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GamePlusPlayersTable_GamesList_gameNameName",
                        column: x => x.gameNameName,
                        principalTable: "GamesList",
                        principalColumn: "Name");
                    table.ForeignKey(
                        name: "FK_GamePlusPlayersTable_Players_playerId",
                        column: x => x.playerId,
                        principalTable: "Players",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MyBlackJackTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tablename = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    actionTime = table.Column<int>(type: "int", nullable: false),
                    sidebet1 = table.Column<bool>(type: "bit", nullable: false),
                    sidebet2 = table.Column<bool>(type: "bit", nullable: false),
                    decks = table.Column<int>(type: "int", nullable: false),
                    seatsCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyBlackJackTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MyBlackJackTables_TablesList_tablename",
                        column: x => x.tablename,
                        principalTable: "TablesList",
                        principalColumn: "name");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GamePlusPlayersTable_gameNameName",
                table: "GamePlusPlayersTable",
                column: "gameNameName");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlusPlayersTable_playerId",
                table: "GamePlusPlayersTable",
                column: "playerId");

            migrationBuilder.CreateIndex(
                name: "IX_MyBlackJackTables_tablename",
                table: "MyBlackJackTables",
                column: "tablename");

            migrationBuilder.CreateIndex(
                name: "IX_TablesList_gameName",
                table: "TablesList",
                column: "gameName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dealers");

            migrationBuilder.DropTable(
                name: "GamePlusPlayersTable");

            migrationBuilder.DropTable(
                name: "MyBlackJackTables");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "TablesList");

            migrationBuilder.DropTable(
                name: "GamesList");
        }
    }
}
