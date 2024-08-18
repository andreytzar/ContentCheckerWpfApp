using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentCheckerWpfApp.Migrations
{
    /// <inheritdoc />
    public partial class MT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "Pages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "Pages");
        }
    }
}
