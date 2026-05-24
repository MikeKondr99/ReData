using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Migrations;
using ReData.Query.Core.Types;

#nullable disable

namespace ReData.DemoApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDataConnectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "DataSets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTimeOffset.UtcNow);

            migrationBuilder.AddColumn<Guid>(
                name: "DataConnectorId",
                table: "DataSets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "DataSets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTimeOffset.UtcNow);

            migrationBuilder.CreateTable(
                name: "DataConnectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FieldList = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataConnectors", x => x.Id);
                });
            
            migrationBuilder.CreateIndex(
                name: "IX_DataSets_DataConnectorId",
                table: "DataSets",
                column: "DataConnectorId");

            migrationBuilder.CreateIndex(
                name: "IX_DataConnectors_Name",
                table: "DataConnectors",
                column: "Name",
                unique: true);


            
            var fieldListJson = 
                $$"""
                [
                    {"Alias": "id", "DataType": {{(int)DataType.Integer}}, "CanBeNull": false},
                    {"Alias": "customer_name", "DataType": {{(int)DataType.Text}}, "CanBeNull": true},
                    {"Alias": "email", "DataType": {{(int)DataType.Text}}, "CanBeNull": true},
                    {"Alias": "age", "DataType": {{(int)DataType.Integer}}, "CanBeNull": true},
                    {"Alias": "account_balance", "DataType": {{(int)DataType.Number}}, "CanBeNull": true},
                    {"Alias": "is_active", "DataType": {{(int)DataType.Bool}}, "CanBeNull": true},
                    {"Alias": "signup_date", "DataType": {{(int)DataType.DateTime}}, "CanBeNull": true},
                    {"Alias": "last_login", "DataType": {{(int)DataType.DateTime}}, "CanBeNull": true},
                    {"Alias": "customer_category", "DataType": {{(int)DataType.Text}}, "CanBeNull": true},
                    {"Alias": "random_number", "DataType": {{(int)DataType.Number}}, "CanBeNull": true},
                    {"Alias": "notes", "DataType": {{(int)DataType.Text}}, "CanBeNull": true},
                    {"Alias": "purchase_count", "DataType": {{(int)DataType.Integer}}, "CanBeNull": true}
                ]
                """; // Escape single quotes for SQL
            migrationBuilder.Sql($"""
                INSERT INTO "DataConnectors" ("Id", "Name", "TableName", "FieldList", "CreatedAt", "UpdatedAt") VALUES 
                ('{Guid.Empty}', 'Demo', 'test_data','{fieldListJson}','{DateTimeOffset.UtcNow:s}','{DateTimeOffset.UtcNow:s}')
                """);
            
            migrationBuilder.AddForeignKey(
                name: "FK_DataSets_DataConnectors_DataConnectorId",
                table: "DataSets",
                column: "DataConnectorId",
                principalTable: "DataConnectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetDefault);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataSets_DataConnectors_DataConnectorId",
                table: "DataSets");

            migrationBuilder.DropTable(
                name: "DataConnectors");

            migrationBuilder.DropIndex(
                name: "IX_DataSets_DataConnectorId",
                table: "DataSets");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DataSets");

            migrationBuilder.DropColumn(
                name: "DataConnectorId",
                table: "DataSets");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DataSets");
        }
    }
}
