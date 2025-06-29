using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MitDenkt.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Services",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Services");
        }
    }
}
