using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Dal.Migrations
{
    /// <inheritdoc />
    public partial class addAccessCodeData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedAccessCodes",
                columns: table => new
                {
                    AccessCodeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccessCodeCarRego = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AccessCodePeriodFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccessCodePeriodTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SecurityAreaId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SecurityAreaName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BookingId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BookingName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BookingArrival = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookingDeparture = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuestId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedAccessCodes", x => x.AccessCodeId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedAccessCodes");
        }
    }
}
