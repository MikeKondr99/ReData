using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReData.DemoApp.Jobs.Migrations
{
    /// <inheritdoc />
    public partial class TickerQ9_1_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeTicker_Status_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers");

            migrationBuilder.DropIndex(
                name: "IX_Function_Expression_Request",
                schema: "ticker",
                table: "CronTickers");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_Status_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers",
                columns: new[] { "Status", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Function_Expression",
                schema: "ticker",
                table: "CronTickers",
                columns: new[] { "Function", "Expression" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeTicker_Status_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers");

            migrationBuilder.DropIndex(
                name: "IX_Function_Expression",
                schema: "ticker",
                table: "CronTickers");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_Status_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers",
                columns: new[] { "Status", "ExecutionTime", "Request" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Function_Expression_Request",
                schema: "ticker",
                table: "CronTickers",
                columns: new[] { "Function", "Expression", "Request" },
                unique: true);
        }
    }
}
