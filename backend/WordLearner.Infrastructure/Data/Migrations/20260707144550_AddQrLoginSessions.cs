using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordLearner.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQrLoginSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QrLoginSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QrTokenHash = table.Column<string>(type: "nvarchar(88)", maxLength: 88, nullable: false),
                    PairingCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    RequesterIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    RequesterDeviceInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_QrLoginSessions", x => x.Id);
                    table.CheckConstraint("CK_QrLoginSessions_Status", "Status IN ('Pending','Scanned','Confirmed','Consumed','Denied','Expired')");
                    table.ForeignKey(
                        name: "FK_QrLoginSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QrLoginSessions_ExpiresAt",
                table: "QrLoginSessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_QrLoginSessions_QrTokenHash",
                table: "QrLoginSessions",
                column: "QrTokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_QrLoginSessions_UserId",
                table: "QrLoginSessions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QrLoginSessions");
        }
    }
}
