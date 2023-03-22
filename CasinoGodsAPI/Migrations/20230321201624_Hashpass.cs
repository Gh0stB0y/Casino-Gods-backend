using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasinoGodsAPI.Migrations
{
    /// <inheritdoc />
    public partial class Hashpass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "passHash",
                table: "Players",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "passSalt",
                table: "Players",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "passHash",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "passSalt",
                table: "Players");
        }
    }
}
