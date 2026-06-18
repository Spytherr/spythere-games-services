using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpythereGamesServices.Migrations
{
    /// <inheritdoc />
    public partial class UniqueConstraintsAndUpsert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Scores_PlayerId",
                table: "Scores");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_PlayerId_GameId",
                table: "Scores",
                columns: new[] { "PlayerId", "GameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_Key",
                table: "Games",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Scores_PlayerId_GameId",
                table: "Scores");

            migrationBuilder.DropIndex(
                name: "IX_Games_Key",
                table: "Games");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_PlayerId",
                table: "Scores",
                column: "PlayerId");
        }
    }
}
