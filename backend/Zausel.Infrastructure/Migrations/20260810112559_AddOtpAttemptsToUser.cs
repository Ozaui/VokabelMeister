using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zausel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpAttemptsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PendingOtpCodeAttempts",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingOtpCodeAttempts",
                table: "Users");
        }
    }
}
