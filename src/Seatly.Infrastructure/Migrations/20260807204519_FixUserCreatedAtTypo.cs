using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seatly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserCreatedAtTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatetAd",
                table: "Users",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Users",
                newName: "CreatetAd");
        }
    }
}
