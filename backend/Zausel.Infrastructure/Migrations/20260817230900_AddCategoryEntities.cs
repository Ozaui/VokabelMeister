using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Zausel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MinLevel = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    MaxLevel = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.CheckConstraint("CK_Categories_MaxLevel", "MaxLevel IS NULL OR MaxLevel IN ('A1','A2','B1','B2','C1','C2')");
                    table.CheckConstraint("CK_Categories_MinLevel", "MinLevel IS NULL OR MinLevel IN ('A1','A2','B1','B2','C1','C2')");
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryTranslations_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryTranslations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WordCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WordConceptId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WordCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WordCategories_WordConcepts_WordConceptId",
                        column: x => x.WordConceptId,
                        principalTable: "WordConcepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Color", "CreatedAt", "CreatedByUserId", "DeletedAt", "DeletedByUserId", "DisplayOrder", "Icon", "IsActive", "IsDeleted", "MaxLevel", "MinLevel", "ParentCategoryId", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, "#FF6B6B", new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 1, "people", true, false, null, "A1", null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2, "#FF8C42", new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 2, "family", true, false, null, "A1", null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3, "#95E1D3", new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 3, "food", true, false, null, "A1", null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 4, "#4ECDC4", new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 4, "house", true, false, null, "A1", null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 5, "#AA96DA", new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 5, "school", true, false, null, "A1", null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 6, "#FCBAD3", new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 6, "numbers", true, false, null, "A1", null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 7, "#A8EDEA", new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 7, "colors", true, false, null, "A1", null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 8, "#FFD89B", new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 8, "time", true, false, null, "A1", null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 9, "#FB7D5B", new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 9, "body", true, false, null, "A1", null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 10, "#84DCC6", new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 10, "animal", true, false, null, "A1", null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 11, "#F38181", new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 11, "work", true, false, null, "A2", null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 12, "#C7CEEA", new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 12, "travel", true, false, null, "A2", null, new DateTime(2026, 8, 18, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "CategoryTranslations",
                columns: new[] { "Id", "CategoryId", "Description", "LanguageId", "Name" },
                values: new object[,]
                {
                    { 1, 1, null, 1, "Menschen" },
                    { 2, 1, null, 2, "İnsanlar" },
                    { 3, 2, null, 1, "Familie" },
                    { 4, 2, null, 2, "Aile" },
                    { 5, 3, null, 1, "Essen" },
                    { 6, 3, null, 2, "Yemek" },
                    { 7, 4, null, 1, "Haus" },
                    { 8, 4, null, 2, "Ev" },
                    { 9, 5, null, 1, "Schule" },
                    { 10, 5, null, 2, "Okul" },
                    { 11, 6, null, 1, "Zahlen" },
                    { 12, 6, null, 2, "Sayılar" },
                    { 13, 7, null, 1, "Farben" },
                    { 14, 7, null, 2, "Renkler" },
                    { 15, 8, null, 1, "Zeit" },
                    { 16, 8, null, 2, "Zaman" },
                    { 17, 9, null, 1, "Körperteile" },
                    { 18, 9, null, 2, "Vücut" },
                    { 19, 10, null, 1, "Tiere" },
                    { 20, 10, null, 2, "Hayvanlar" },
                    { 21, 11, null, 1, "Arbeit" },
                    { 22, 11, null, 2, "İş" },
                    { 23, 12, null, 1, "Reisen" },
                    { 24, 12, null, 2, "Seyahat" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTranslations_CategoryId_LanguageId",
                table: "CategoryTranslations",
                columns: new[] { "CategoryId", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTranslations_LanguageId",
                table: "CategoryTranslations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_WordCategories_CategoryId",
                table: "WordCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WordCategories_WordConceptId_CategoryId",
                table: "WordCategories",
                columns: new[] { "WordConceptId", "CategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryTranslations");

            migrationBuilder.DropTable(
                name: "WordCategories");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
