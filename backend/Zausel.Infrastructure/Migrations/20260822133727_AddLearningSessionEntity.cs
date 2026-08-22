using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zausel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningSessionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TargetLanguageId = table.Column<int>(type: "int", nullable: false),
                    SessionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LevelFilter = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    CategoryIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserCategoryIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    TotalWords = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CorrectAnswers = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IncorrectAnswers = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SuccessRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    XPEarned = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningSessions", x => x.Id);
                    table.CheckConstraint("CK_LearningSessions_LevelFilter", "LevelFilter IS NULL OR LevelFilter IN ('A1','A2','B1','B2','C1','C2')");
                    table.CheckConstraint("CK_LearningSessions_SessionType", "SessionType IN ('Flashcard','MultipleChoice','ArticleQuiz','PluralQuiz','TranslationQuiz','TrueFalse')");
                    table.CheckConstraint("CK_LearningSessions_SourceType", "SourceType IN ('SystemWords','UserCards','Mixed')");
                    table.CheckConstraint("CK_LearningSessions_Status", "Status IN ('Active','Completed','Abandoned')");
                    table.ForeignKey(
                        name: "FK_LearningSessions_Languages_TargetLanguageId",
                        column: x => x.TargetLanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningHistory_LearningSessionId",
                table: "LearningHistory",
                column: "LearningSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningSessions_TargetLanguageId",
                table: "LearningSessions",
                column: "TargetLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningSessions_UserId",
                table: "LearningSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningHistory_LearningSessions_LearningSessionId",
                table: "LearningHistory",
                column: "LearningSessionId",
                principalTable: "LearningSessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningHistory_LearningSessions_LearningSessionId",
                table: "LearningHistory");

            migrationBuilder.DropTable(
                name: "LearningSessions");

            migrationBuilder.DropIndex(
                name: "IX_LearningHistory_LearningSessionId",
                table: "LearningHistory");
        }
    }
}
