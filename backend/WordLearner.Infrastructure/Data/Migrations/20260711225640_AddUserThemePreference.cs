using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordLearner.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserThemePreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThemePreference",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_ThemePreference",
                table: "Users",
                sql: "ThemePreference IN ('Light','Dark','System')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_ThemePreference",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ThemePreference",
                table: "Users");
        }
    }
}
