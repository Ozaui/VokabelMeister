using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordLearner.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAndRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    GoogleId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AppleId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AuthProvider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Local"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DailyWordGoal = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    DailyNewWordLimit = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    CurrentLevel = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false, defaultValue: "A1"),
                    TotalXP = table.Column<int>(type: "int", nullable: false),
                    LifetimeXP = table.Column<int>(type: "int", nullable: false),
                    StreakDays = table.Column<int>(type: "int", nullable: false),
                    LastStreakDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PendingOtpCodeHash = table.Column<string>(type: "nvarchar(88)", maxLength: 88, nullable: true),
                    PendingOtpCodeExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PendingOtpCodePurpose = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsOnboardingCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    EmailVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    LoginCount = table.Column<int>(type: "int", nullable: false),
                    ScheduledDeletionAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAnonymized = table.Column<bool>(type: "bit", nullable: false),
                    OriginalEmailHash = table.Column<string>(type: "nvarchar(88)", maxLength: 88, nullable: true),
                    OneSignalPlayerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "User"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.CheckConstraint("CK_Users_AuthProvider", "AuthProvider IN ('Local','Google','Apple')");
                    table.CheckConstraint("CK_Users_Level", "CurrentLevel IN ('A1','A2','B1','B2','C1','C2')");
                    table.CheckConstraint("CK_Users_Role", "Role IN ('User','Admin')");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(88)", maxLength: 88, nullable: false),
                    TokenFamily = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeviceInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenFamily",
                table: "RefreshTokens",
                column: "TokenFamily");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AppleId",
                table: "Users",
                column: "AppleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GoogleId",
                table: "Users",
                column: "GoogleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
