using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReData.DemoApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class DataSetMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FieldList",
                table: "DataSets",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RowsCount",
                table: "DataSets",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FieldList",
                table: "DataSets");

            migrationBuilder.DropColumn(
                name: "RowsCount",
                table: "DataSets");
        }
    }
}
