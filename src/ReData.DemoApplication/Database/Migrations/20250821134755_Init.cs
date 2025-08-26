using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReData.DemoApplication.Database.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transformations",
                columns: table => new
                {
                    DataSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<long>(type: "bigint", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false),
                    DataSetEntityId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transformations", x => new { x.DataSetId, x.Order });
                    table.ForeignKey(
                        name: "FK_Transformations_DataSets_DataSetEntityId",
                        column: x => x.DataSetEntityId,
                        principalTable: "DataSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_Name",
                table: "DataSets",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transformations_DataSetEntityId",
                table: "Transformations",
                column: "DataSetEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transformations");

            migrationBuilder.DropTable(
                name: "DataSets");
        }
    }
}
