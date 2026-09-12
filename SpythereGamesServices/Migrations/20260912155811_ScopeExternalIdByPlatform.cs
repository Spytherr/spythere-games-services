using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpythereGamesServices.Migrations
{
    /// <inheritdoc />
    public partial class ScopeExternalIdByPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_ExternalId",
                table: "Players");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Platform_ExternalId",
                table: "Players",
                columns: new[] { "Platform", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_Platform_ExternalId",
                table: "Players");

            migrationBuilder.CreateIndex(
                name: "IX_Players_ExternalId",
                table: "Players",
                column: "ExternalId",
                unique: true);
        }
    }
}
