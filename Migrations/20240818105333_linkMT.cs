using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentCheckerWpfApp.Migrations
{
    /// <inheritdoc />
    public partial class linkMT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "Links",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "Links");

            migrationBuilder.AddColumn<int>(
                name: "_Id",
                table: "Pages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
