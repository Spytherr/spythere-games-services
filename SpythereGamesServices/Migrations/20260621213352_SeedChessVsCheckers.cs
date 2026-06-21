using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpythereGamesServices.Migrations
{
    /// <inheritdoc />
    public partial class SeedChessVsCheckers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Games",
                columns: new[] { "Id", "CreatedAt", "Description", "IconUrl", "Key", "Name" },
                values: new object[] { 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A unique blend of Chess and Checkers mechanics.", "", "chess-vs-checkers", "Chess vs Checkers" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Games",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
