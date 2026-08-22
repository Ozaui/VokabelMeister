using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zausel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCardEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FrontText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BackText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_UserCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCards_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCardCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserCardId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCardCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCardCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCardCategories_UserCards_UserCardId",
                        column: x => x.UserCardId,
                        principalTable: "UserCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCardExamples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserCardId = table.Column<int>(type: "int", nullable: false),
                    SentenceFront = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentenceBack = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCardExamples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCardExamples_UserCards_UserCardId",
                        column: x => x.UserCardId,
                        principalTable: "UserCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCardUserCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserCardId = table.Column<int>(type: "int", nullable: false),
                    UserCategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCardUserCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCardUserCategories_UserCards_UserCardId",
                        column: x => x.UserCardId,
                        principalTable: "UserCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCardUserCategories_UserCategories_UserCategoryId",
                        column: x => x.UserCategoryId,
                        principalTable: "UserCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCardProgress_UserCardId",
                table: "UserCardProgress",
                column: "UserCardId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningHistory_UserCardId",
                table: "LearningHistory",
                column: "UserCardId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCardCategories_CategoryId",
                table: "UserCardCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCardCategories_UserCardId_CategoryId",
                table: "UserCardCategories",
                columns: new[] { "UserCardId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCardExamples_UserCardId",
                table: "UserCardExamples",
                column: "UserCardId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCards_UserId",
                table: "UserCards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCardUserCategories_UserCardId_UserCategoryId",
                table: "UserCardUserCategories",
                columns: new[] { "UserCardId", "UserCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCardUserCategories_UserCategoryId",
                table: "UserCardUserCategories",
                column: "UserCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningHistory_UserCards_UserCardId",
                table: "LearningHistory",
                column: "UserCardId",
                principalTable: "UserCards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCardProgress_UserCards_UserCardId",
                table: "UserCardProgress",
                column: "UserCardId",
                principalTable: "UserCards",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningHistory_UserCards_UserCardId",
                table: "LearningHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCardProgress_UserCards_UserCardId",
                table: "UserCardProgress");

            migrationBuilder.DropTable(
                name: "UserCardCategories");

            migrationBuilder.DropTable(
                name: "UserCardExamples");

            migrationBuilder.DropTable(
                name: "UserCardUserCategories");

            migrationBuilder.DropTable(
                name: "UserCards");

            migrationBuilder.DropIndex(
                name: "IX_UserCardProgress_UserCardId",
                table: "UserCardProgress");

            migrationBuilder.DropIndex(
                name: "IX_LearningHistory_UserCardId",
                table: "LearningHistory");
        }
    }
}
