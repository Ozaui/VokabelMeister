using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordLearner.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLoggingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ActorRole = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceContext = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RequestPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    EmailHash = table.Column<string>(type: "nvarchar(44)", maxLength: 44, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityLogs", x => x.Id);
                    table.CheckConstraint("CK_SecurityLog_EventType", "EventType IN ('LoginFailed','OtpFailed','RateLimitHit','UnauthorizedAccess','TokenReplay','PasswordReset','AccountDeletion','AdminAction','QrLoginConfirmed','QrLoginDenied')");
                    table.ForeignKey(
                        name: "FK_SecurityLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Action",
                table: "ActivityLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_CreatedAt",
                table: "ActivityLogs",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_EntityType",
                table: "ActivityLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_UserId",
                table: "ActivityLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Level",
                table: "ApplicationLogs",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_TimeStamp",
                table: "ApplicationLogs",
                column: "TimeStamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityLogs_CreatedAt",
                table: "SecurityLogs",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityLogs_EventType",
                table: "SecurityLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityLogs_IpAddress",
                table: "SecurityLogs",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityLogs_UserId",
                table: "SecurityLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "ApplicationLogs");

            migrationBuilder.DropTable(
                name: "SecurityLogs");
        }
    }
}
