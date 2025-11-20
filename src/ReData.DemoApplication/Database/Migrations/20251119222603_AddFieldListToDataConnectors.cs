using Microsoft.EntityFrameworkCore.Migrations;
using ReData.Query.Core.Types;

#nullable disable

namespace ReData.DemoApplication.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldListToDataConnectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            
                
            // lang=json
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
    
            migrationBuilder.Sql(
              $"""
              UPDATE "DataConnectors" 
              SET "FieldList" = '{fieldListJson}'
              WHERE "Id" = '{Guid.Empty}'
              """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FieldList",
                table: "DataConnectors");
        }
    }
}
