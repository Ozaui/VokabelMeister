using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordLearner.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixHashColumnMaxLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PendingOtpCodeHash",
                table: "Users",
                type: "nvarchar(44)",
                maxLength: 44,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(88)",
                oldMaxLength: 88,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalEmailHash",
                table: "Users",
                type: "nvarchar(44)",
                maxLength: 44,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(88)",
                oldMaxLength: 88,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "RefreshTokens",
                type: "nvarchar(44)",
                maxLength: 44,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(88)",
                oldMaxLength: 88);

            migrationBuilder.AlterColumn<string>(
                name: "QrTokenHash",
                table: "QrLoginSessions",
                type: "nvarchar(44)",
                maxLength: 44,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(88)",
                oldMaxLength: 88);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PendingOtpCodeHash",
                table: "Users",
                type: "nvarchar(88)",
                maxLength: 88,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(44)",
                oldMaxLength: 44,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalEmailHash",
                table: "Users",
                type: "nvarchar(88)",
                maxLength: 88,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(44)",
                oldMaxLength: 44,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "RefreshTokens",
                type: "nvarchar(88)",
                maxLength: 88,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(44)",
                oldMaxLength: 44);

            migrationBuilder.AlterColumn<string>(
                name: "QrTokenHash",
                table: "QrLoginSessions",
                type: "nvarchar(88)",
                maxLength: 88,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(44)",
                oldMaxLength: 44);
        }
    }
}
