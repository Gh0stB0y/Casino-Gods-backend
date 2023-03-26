using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasinoGodsAPI.Migrations
{
    /// <inheritdoc />
    public partial class table3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GamePlusPlayersTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gameNameName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    playerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                name: "GamePlusPlayersTable");
        }
    }
}
