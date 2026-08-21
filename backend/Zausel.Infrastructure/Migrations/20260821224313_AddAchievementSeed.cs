using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Zausel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAchievementSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Achievements");

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "CreatedAt", "Icon", "RewardXP" },
                values: new object[] { 1, new DateTime(2026, 8, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, 10 });

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "CreatedAt", "Icon", "Rarity", "RewardXP" },
                values: new object[,]
                {
                    { 2, new DateTime(2026, 8, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, "Rare", 25 },
                    { 3, new DateTime(2026, 8, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, "Epic", 50 }
                });

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "CreatedAt", "Icon", "RewardXP" },
                values: new object[] { 4, new DateTime(2026, 8, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, 10 });

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "CreatedAt", "Icon", "Rarity", "RewardXP" },
                values: new object[,]
                {
                    { 5, new DateTime(2026, 8, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, "Rare", 25 },
                    { 6, new DateTime(2026, 8, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, "Epic", 50 },
                    { 7, new DateTime(2026, 8, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, "Rare", 30 },
                    { 8, new DateTime(2026, 8, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, "Epic", 40 },
                    { 9, new DateTime(2026, 8, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, "Rare", 20 }
                });

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "CreatedAt", "Icon", "RewardXP" },
                values: new object[] { 10, new DateTime(2026, 8, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, 15 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Achievements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Achievements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
