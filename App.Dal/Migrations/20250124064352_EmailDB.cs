using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Dal.Migrations
{
    /// <inheritdoc />
    public partial class EmailDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailScheduled",
                table: "ProcessedBookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailSent",
                table: "ProcessedBookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PassClaimed",
                table: "ProcessedBookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailScheduled",
                table: "ProcessedBookings");

            migrationBuilder.DropColumn(
                name: "EmailSent",
                table: "ProcessedBookings");

            migrationBuilder.DropColumn(
                name: "PassClaimed",
                table: "ProcessedBookings");
        }
    }
}
