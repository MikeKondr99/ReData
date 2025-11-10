using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReData.DemoApplication.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTransformBlockDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Transformations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Transformations");
        }
    }
}
