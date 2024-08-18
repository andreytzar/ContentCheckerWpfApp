using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentCheckerWpfApp.Migrations
{
    /// <inheritdoc />
    public partial class link : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateTested",
                table: "Links",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTested",
                table: "Links");

            migrationBuilder.AddColumn<DateTime>(
                name: "_Scanned",
                table: "Pages",
                type: "datetime2",
                nullable: true);
        }
    }
}
