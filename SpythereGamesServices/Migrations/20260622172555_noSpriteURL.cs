using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpythereGamesServices.Migrations
{
    /// <inheritdoc />
    public partial class noSpriteURL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "Games");

            migrationBuilder.CreateIndex(
                name: "IX_Players_ExternalId",
                table: "Players",
                column: "ExternalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_ExternalId",
                table: "Players");

            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "Games",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Games",
                keyColumn: "Id",
                keyValue: 1,
                column: "IconUrl",
                value: "");
        }
    }
}
