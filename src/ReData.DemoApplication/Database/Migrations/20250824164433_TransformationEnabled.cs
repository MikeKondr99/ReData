using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReData.DemoApplication.Database.Migrations
{
    /// <inheritdoc />
    public partial class TransformationEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "Transformations",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "Transformations");
        }
    }
}
