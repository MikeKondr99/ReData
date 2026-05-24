using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReData.DemoApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class DataSetFkFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cleanup legacy orphans created while relationship was bound to DataSetEntityId.
            // Without this, adding FK on DataSetId can fail on existing databases.
            migrationBuilder.Sql("""
                DELETE FROM "Transformations" t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "DataSets" ds
                    WHERE ds."Id" = t."DataSetId"
                );
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Transformations"
                DROP CONSTRAINT IF EXISTS "FK_Transformations_DataSets_DataSetEntityId";
                """);

            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_Transformations_DataSetEntityId";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Transformations"
                DROP COLUMN IF EXISTS "DataSetEntityId";
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'FK_Transformations_DataSets_DataSetId'
                    ) THEN
                        ALTER TABLE "Transformations"
                        ADD CONSTRAINT "FK_Transformations_DataSets_DataSetId"
                        FOREIGN KEY ("DataSetId")
                        REFERENCES "DataSets" ("Id")
                        ON DELETE CASCADE;
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transformations_DataSets_DataSetId",
                table: "Transformations");

            migrationBuilder.AddColumn<Guid>(
                name: "DataSetEntityId",
                table: "Transformations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transformations_DataSetEntityId",
                table: "Transformations",
                column: "DataSetEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transformations_DataSets_DataSetEntityId",
                table: "Transformations",
                column: "DataSetEntityId",
                principalTable: "DataSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
