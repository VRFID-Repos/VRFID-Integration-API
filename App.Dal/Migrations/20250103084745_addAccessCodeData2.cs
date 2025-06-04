using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Dal.Migrations
{
    /// <inheritdoc />
    public partial class addAccessCodeData2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardholderGuid",
                table: "ProcessedAccessCodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LicenseCredGuid",
                table: "ProcessedAccessCodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PinCredGuid",
                table: "ProcessedAccessCodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardholderGuid",
                table: "ProcessedAccessCodes");

            migrationBuilder.DropColumn(
                name: "LicenseCredGuid",
                table: "ProcessedAccessCodes");

            migrationBuilder.DropColumn(
                name: "PinCredGuid",
                table: "ProcessedAccessCodes");
        }
    }
}
