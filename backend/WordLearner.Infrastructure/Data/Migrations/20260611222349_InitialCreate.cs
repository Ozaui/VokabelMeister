using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WordLearner.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RewardXP = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Common"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                    table.CheckConstraint("CK_Achievements_Rarity", "[Rarity] IN ('Common','Rare','Epic','Legendary')");
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameDE = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameTR = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescriptionTR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MinLevel = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    MaxLevel = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                    PreferredLanguagePair = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "TR-DE"),
                    PreferredUILanguage = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false, defaultValue: "tr"),
                    DailyWordGoal = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    DailyNewWordLimit = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    CurrentLevel = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false, defaultValue: "A1"),
                    TotalXP = table.Column<int>(type: "int", nullable: false),
                    LifetimeXP = table.Column<int>(type: "int", nullable: false),
                    TotalLearningMinutes = table.Column<int>(type: "int", nullable: false),
                    StreakDays = table.Column<int>(type: "int", nullable: false),
                    LastStreakDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOnboardingCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    EmailVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    LoginCount = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "User"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.CheckConstraint("CK_Users_AuthProvider", "[AuthProvider] IN ('Local','Google','Apple')");
                    table.CheckConstraint("CK_Users_Level", "[CurrentLevel] IN ('A1','A2','B1','B2','C1','C2')");
                    table.CheckConstraint("CK_Users_Role", "[Role] IN ('User','Instructor','Admin')");
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecordId = table.Column<int>(type: "int", nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InviteCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesterId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => x.Id);
                    table.CheckConstraint("CK_Friendships_Self", "[RequesterId] <> [ReceiverId]");
                    table.CheckConstraint("CK_Friendships_Status", "[Status] IN ('Pending','Accepted','Rejected','Blocked')");
                    table.ForeignKey(
                        name: "FK_Friendships_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Friendships_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearningSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SessionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LevelFilter = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    CategoryIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserCategoryIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    TotalWords = table.Column<int>(type: "int", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "int", nullable: false),
                    IncorrectAnswers = table.Column<int>(type: "int", nullable: false),
                    SuccessRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    XPEarned = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningSessions", x => x.Id);
                    table.CheckConstraint("CK_LearningSessions_Status", "[Status] IN ('Active','Completed','Abandoned')");
                    table.ForeignKey(
                        name: "FK_LearningSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "SharedContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    ShareToken = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ContentId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedContents", x => x.Id);
                    table.CheckConstraint("CK_SharedContents_ContentType", "[ContentType] IN ('UserCard','UserCategory','Class')");
                    table.ForeignKey(
                        name: "FK_SharedContents_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AchievementId = table.Column<int>(type: "int", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    AudioUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "UserCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCategories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GermanWord = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TurkishTranslation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PartOfSpeech = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DifficultyLevel = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false, defaultValue: "A1"),
                    Definition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Id);
                    table.CheckConstraint("CK_Words_Level", "[DifficultyLevel] IN ('A1','A2','B1','B2','C1','C2')");
                    table.CheckConstraint("CK_Words_PartOfSpeech", "[PartOfSpeech] IN ('Noun','Verb','Adjective','Adverb','Conjunction','Preposition','Pronoun','Other')");
                    table.ForeignKey(
                        name: "FK_Words_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Words_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Words_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassCategories_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassMemberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Student"),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassMemberships", x => x.Id);
                    table.CheckConstraint("CK_ClassMemberships_Role", "[Role] IN ('Student','Teacher')");
                    table.ForeignKey(
                        name: "FK_ClassMemberships_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassMemberships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SharedContentImports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SharedContentId = table.Column<int>(type: "int", nullable: false),
                    ImportedByUserId = table.Column<int>(type: "int", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedContentImports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedContentImports_SharedContents_SharedContentId",
                        column: x => x.SharedContentId,
                        principalTable: "SharedContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedContentImports_Users_ImportedByUserId",
                        column: x => x.ImportedByUserId,
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCardCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCardCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "UserCardProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserCardId = table.Column<int>(type: "int", nullable: false),
                    CurrentLevel = table.Column<int>(type: "int", nullable: false),
                    Mastery = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TimesCorrect = table.Column<int>(type: "int", nullable: false),
                    TimesIncorrect = table.Column<int>(type: "int", nullable: false),
                    TotalAttempts = table.Column<int>(type: "int", nullable: false),
                    SuccessRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    LastReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextReviewAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IntervalDays = table.Column<int>(type: "int", nullable: false),
                    RepetitionNumber = table.Column<int>(type: "int", nullable: false),
                    EasinessFactor = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 2.5m),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCardProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCardProgress_UserCards_UserCardId",
                        column: x => x.UserCardId,
                        principalTable: "UserCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCardProgress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassUserCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    UserCategoryId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassUserCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassUserCategories_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassUserCategories_UserCategories_UserCategoryId",
                        column: x => x.UserCategoryId,
                        principalTable: "UserCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCardUserCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserCardId = table.Column<int>(type: "int", nullable: false),
                    UserCategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "LearningHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WordId = table.Column<int>(type: "int", nullable: true),
                    UserCardId = table.Column<int>(type: "int", nullable: true),
                    LearningSessionId = table.Column<int>(type: "int", nullable: true),
                    SessionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    ResponseTime = table.Column<int>(type: "int", nullable: true),
                    TimeSpentSeconds = table.Column<int>(type: "int", nullable: true),
                    UserResponse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CorrectResponse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SelfRating = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningHistory_LearningSessions_LearningSessionId",
                        column: x => x.LearningSessionId,
                        principalTable: "LearningSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningHistory_UserCards_UserCardId",
                        column: x => x.UserCardId,
                        principalTable: "UserCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningHistory_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WordId = table.Column<int>(type: "int", nullable: false),
                    CurrentLevel = table.Column<int>(type: "int", nullable: false),
                    Mastery = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TimesCorrect = table.Column<int>(type: "int", nullable: false),
                    TimesIncorrect = table.Column<int>(type: "int", nullable: false),
                    TotalAttempts = table.Column<int>(type: "int", nullable: false),
                    SuccessRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    LastReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextReviewAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IntervalDays = table.Column<int>(type: "int", nullable: false),
                    RepetitionNumber = table.Column<int>(type: "int", nullable: false),
                    EasinessFactor = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 2.5m),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProgress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProgress_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WordCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WordId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                        name: "FK_WordCategories_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WordDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WordId = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ArticleDefiniteNom = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ArticleDefiniteAcc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ArticleDefiniteDat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ArticleDefiniteGen = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ArticleIndefiniteNom = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ArticleIndefiniteAcc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ArticleIndefiniteDat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ArticleIndefiniteGen = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    FormNominative = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FormAccusative = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FormDative = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FormGenitive = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PluralForm = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PluralFormNominative = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PluralFormAccusative = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PluralFormDative = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PluralFormGenitive = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ConjugationData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSeparableVerb = table.Column<bool>(type: "bit", nullable: false),
                    SeparablePrefix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Pronunciation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AudioUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommonMistakes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WordDetails_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WordExamples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WordId = table.Column<int>(type: "int", nullable: false),
                    SentenceDE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentenceTR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false, defaultValue: "A1"),
                    ExampleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Normal"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordExamples", x => x.Id);
                    table.CheckConstraint("CK_WordExamples_ExampleType", "[ExampleType] IN ('Normal','Idiom','Formal','Colloquial')");
                    table.CheckConstraint("CK_WordExamples_Level", "[Level] IN ('A1','A2','B1','B2','C1','C2')");
                    table.ForeignKey(
                        name: "FK_WordExamples_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WordExamples_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WordExamples_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Color", "CreatedAt", "DeletedAt", "DescriptionTR", "DisplayOrder", "Icon", "IsActive", "IsDeleted", "MaxLevel", "MinLevel", "NameDE", "NameTR", "ParentCategoryId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "#FF6B6B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "people", true, false, null, "A1", "Menschen", "İnsanlar", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "#FF8C42", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, "family", true, false, null, "A1", "Familie", "Aile", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "#95E1D3", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, "food", true, false, null, "A1", "Essen", "Yemek", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "#4ECDC4", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, "house", true, false, null, "A1", "Haus", "Ev", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "#AA96DA", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 5, "school", true, false, null, "A1", "Schule", "Okul", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, "#FCBAD3", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 6, "numbers", true, false, null, "A1", "Zahlen", "Sayılar", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, "#A8EDEA", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 7, "colors", true, false, null, "A1", "Farben", "Renkler", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, "#FFD89B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 8, "time", true, false, null, "A1", "Zeit", "Zaman", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, "#FB7D5B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 9, "body", true, false, null, "A1", "Körperteile", "Vücut Bölümleri", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, "#84DCC6", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 10, "animal", true, false, null, "A1", "Tiere", "Hayvanlar", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, "#F38181", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 11, "work", true, false, null, "A2", "Arbeit", "İş", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, "#C7CEEA", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 12, "travel", true, false, null, "A2", "Reisen", "Seyahat", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Action",
                table: "AuditLog",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_CreatedAt",
                table: "AuditLog",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserId",
                table: "AuditLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsActive",
                table: "Categories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassCategories_CategoryId",
                table: "ClassCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassCategories_ClassId",
                table: "ClassCategories",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassCategories_ClassId_CategoryId",
                table: "ClassCategories",
                columns: new[] { "ClassId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_InviteCode",
                table: "Classes",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_OwnerId",
                table: "Classes",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassMemberships_ClassId",
                table: "ClassMemberships",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassMemberships_ClassId_UserId",
                table: "ClassMemberships",
                columns: new[] { "ClassId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassMemberships_UserId",
                table: "ClassMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassUserCategories_ClassId",
                table: "ClassUserCategories",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassUserCategories_ClassId_UserCategoryId",
                table: "ClassUserCategories",
                columns: new[] { "ClassId", "UserCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassUserCategories_UserCategoryId",
                table: "ClassUserCategories",
                column: "UserCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_ReceiverId",
                table: "Friendships",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequesterId",
                table: "Friendships",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequesterId_ReceiverId",
                table: "Friendships",
                columns: new[] { "RequesterId", "ReceiverId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_Status",
                table: "Friendships",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LearningHistory_CreatedAt",
                table: "LearningHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LearningHistory_LearningSessionId",
                table: "LearningHistory",
                column: "LearningSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningHistory_UserCardId",
                table: "LearningHistory",
                column: "UserCardId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningHistory_UserId",
                table: "LearningHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningHistory_WordId",
                table: "LearningHistory",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningSessions_StartedAt",
                table: "LearningSessions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LearningSessions_Status",
                table: "LearningSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LearningSessions_UserId",
                table: "LearningSessions",
                column: "UserId");

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
                name: "IX_SharedContentImports_ImportedByUserId",
                table: "SharedContentImports",
                column: "ImportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedContentImports_SharedContentId",
                table: "SharedContentImports",
                column: "SharedContentId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedContentImports_SharedContentId_ImportedByUserId",
                table: "SharedContentImports",
                columns: new[] { "SharedContentId", "ImportedByUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SharedContents_OwnerId",
                table: "SharedContents",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedContents_ShareToken",
                table: "SharedContents",
                column: "ShareToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId",
                table: "UserAchievements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId_AchievementId",
                table: "UserAchievements",
                columns: new[] { "UserId", "AchievementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCardCategories_CategoryId",
                table: "UserCardCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCardCategories_UserCardId",
                table: "UserCardCategories",
                column: "UserCardId");

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
                name: "IX_UserCardProgress_UserCardId",
                table: "UserCardProgress",
                column: "UserCardId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCardProgress_UserId",
                table: "UserCardProgress",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCardProgress_UserId_NextReviewAt",
                table: "UserCardProgress",
                columns: new[] { "UserId", "NextReviewAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserCardProgress_UserId_UserCardId",
                table: "UserCardProgress",
                columns: new[] { "UserId", "UserCardId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCards_IsDeleted",
                table: "UserCards",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserCards_UserId",
                table: "UserCards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCardUserCategories_UserCardId",
                table: "UserCardUserCategories",
                column: "UserCardId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCardUserCategories_UserCardId_UserCategoryId",
                table: "UserCardUserCategories",
                columns: new[] { "UserCardId", "UserCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCardUserCategories_UserCategoryId",
                table: "UserCardUserCategories",
                column: "UserCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategories_UserId",
                table: "UserCategories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgress_CurrentLevel",
                table: "UserProgress",
                column: "CurrentLevel");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgress_UserId",
                table: "UserProgress",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgress_UserId_NextReviewAt",
                table: "UserProgress",
                columns: new[] { "UserId", "NextReviewAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserProgress_UserId_WordId",
                table: "UserProgress",
                columns: new[] { "UserId", "WordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProgress_WordId",
                table: "UserProgress",
                column: "WordId");

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
                name: "IX_Users_IsDeleted",
                table: "Users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_WordCategories_CategoryId",
                table: "WordCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WordCategories_WordId",
                table: "WordCategories",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_WordCategories_WordId_CategoryId",
                table: "WordCategories",
                columns: new[] { "WordId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordDetails_Gender",
                table: "WordDetails",
                column: "Gender");

            migrationBuilder.CreateIndex(
                name: "IX_WordDetails_IsSeparableVerb",
                table: "WordDetails",
                column: "IsSeparableVerb");

            migrationBuilder.CreateIndex(
                name: "IX_WordDetails_WordId",
                table: "WordDetails",
                column: "WordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordExamples_CreatedBy",
                table: "WordExamples",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WordExamples_CreatorId",
                table: "WordExamples",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_WordExamples_Level",
                table: "WordExamples",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_WordExamples_WordId",
                table: "WordExamples",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_WordExamples_WordId_Level",
                table: "WordExamples",
                columns: new[] { "WordId", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_Words_ApprovedBy",
                table: "Words",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Words_CreatedBy",
                table: "Words",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Words_DifficultyLevel",
                table: "Words",
                column: "DifficultyLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Words_GermanWord",
                table: "Words",
                column: "GermanWord");

            migrationBuilder.CreateIndex(
                name: "IX_Words_IsActive",
                table: "Words",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Words_PartOfSpeech",
                table: "Words",
                column: "PartOfSpeech");

            migrationBuilder.CreateIndex(
                name: "IX_Words_UpdatedBy",
                table: "Words",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "ClassCategories");

            migrationBuilder.DropTable(
                name: "ClassMemberships");

            migrationBuilder.DropTable(
                name: "ClassUserCategories");

            migrationBuilder.DropTable(
                name: "Friendships");

            migrationBuilder.DropTable(
                name: "LearningHistory");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "SharedContentImports");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "UserCardCategories");

            migrationBuilder.DropTable(
                name: "UserCardExamples");

            migrationBuilder.DropTable(
                name: "UserCardProgress");

            migrationBuilder.DropTable(
                name: "UserCardUserCategories");

            migrationBuilder.DropTable(
                name: "UserProgress");

            migrationBuilder.DropTable(
                name: "WordCategories");

            migrationBuilder.DropTable(
                name: "WordDetails");

            migrationBuilder.DropTable(
                name: "WordExamples");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "LearningSessions");

            migrationBuilder.DropTable(
                name: "SharedContents");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "UserCards");

            migrationBuilder.DropTable(
                name: "UserCategories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Words");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
