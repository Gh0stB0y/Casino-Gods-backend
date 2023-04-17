using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasinoGodsAPI.Migrations
{
    /// <inheritdoc />
    public partial class M1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "BlackjackTables",
                columns: table => new
                {
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    minBet = table.Column<int>(type: "int", nullable: false),
                    maxBet = table.Column<int>(type: "int", nullable: false),
                    betTime = table.Column<int>(type: "int", nullable: false),
                    actionTime = table.Column<int>(type: "int", nullable: false),
                    sidebet1 = table.Column<bool>(type: "bit", nullable: false),
                    sidebet2 = table.Column<bool>(type: "bit", nullable: false),
                    decks = table.Column<int>(type: "int", nullable: false),
                    seatsCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Dealers",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    profit = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dealers", x => x.Name);
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

            migrationBuilder.CreateIndex(
                name: "IX_GamePlusPlayersTable_gameNameName",
                table: "GamePlusPlayersTable",
                column: "gameNameName");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlusPlayersTable_playerId",
                table: "GamePlusPlayersTable",
                column: "playerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivePlayersTable");

            migrationBuilder.DropTable(
                name: "BlackjackTables");

            migrationBuilder.DropTable(
                name: "Dealers");

            migrationBuilder.DropTable(
                name: "GamePlusPlayersTable");

            migrationBuilder.DropTable(
                name: "GamesList");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
