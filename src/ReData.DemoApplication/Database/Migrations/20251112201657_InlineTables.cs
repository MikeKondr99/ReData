using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReData.DemoApplication.Database.Migrations
{
    /// <inheritdoc />
    public partial class InlineTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FieldList",
                table: "DataSets",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TableId",
                table: "DataSets",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FieldList",
                table: "DataSets");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "DataSets");
        }
    }
}
