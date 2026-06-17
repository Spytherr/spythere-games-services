using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpythereGamesServices.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelsAndRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Score",
                table: "Scores",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Scores",
                newName: "SubmittedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Games",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Scores",
                newName: "Score");

            migrationBuilder.RenameColumn(
                name: "SubmittedAt",
                table: "Scores",
                newName: "CreatedAt");
        }
    }
}
