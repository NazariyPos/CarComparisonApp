using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarComparisonApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGenerationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModelId",
                table: "GenerationVariants",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE gv
SET ModelId = g.ModelId
FROM dbo.GenerationVariants gv
INNER JOIN dbo.Generations g ON g.Id = gv.GenerationId;
");

            migrationBuilder.DropForeignKey(
                name: "FK_GenerationVariants_Generations_GenerationId",
                table: "GenerationVariants");

            migrationBuilder.DropIndex(
                name: "IX_GenerationVariants_GenerationId_VariantType_BodyStyleId_DoorsCount",
                table: "GenerationVariants");

            migrationBuilder.DropIndex(
                name: "IX_GenerationVariants_GenerationId",
                table: "GenerationVariants");

            migrationBuilder.DropColumn(
                name: "GenerationId",
                table: "GenerationVariants");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM dbo.GenerationVariants WHERE ModelId IS NULL)
BEGIN
    THROW 50000, 'Failed to backfill GenerationVariants.ModelId before dropping Generations.', 1;
END
");

            migrationBuilder.AlterColumn<int>(
                name: "ModelId",
                table: "GenerationVariants",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GenerationVariants_ModelId",
                table: "GenerationVariants",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_GenerationVariants_ModelId_VariantType_BodyStyleId_DoorsCount",
                table: "GenerationVariants",
                columns: new[] { "ModelId", "VariantType", "BodyStyleId", "DoorsCount" });

            migrationBuilder.AddForeignKey(
                name: "FK_GenerationVariants_CarModels_ModelId",
                table: "GenerationVariants",
                column: "ModelId",
                principalTable: "CarModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropTable(
                name: "Generations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GenerationVariants_CarModels_ModelId",
                table: "GenerationVariants");

            migrationBuilder.AddColumn<int>(
                name: "GenerationId",
                table: "GenerationVariants",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Generations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearFrom = table.Column<int>(type: "int", nullable: false),
                    YearTo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Generations_CarModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "CarModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
SET IDENTITY_INSERT dbo.Generations ON;
INSERT INTO dbo.Generations (Id, ModelId, Name, PhotoUrl, YearFrom, YearTo)
SELECT Id, ModelId, Name, PhotoUrl, YearFrom, YearTo
FROM dbo.GenerationVariants;
SET IDENTITY_INSERT dbo.Generations OFF;
");

            migrationBuilder.CreateIndex(
                name: "IX_Generations_ModelId",
                table: "Generations",
                column: "ModelId");

            migrationBuilder.Sql(@"
UPDATE gv
SET GenerationId = gv.Id
FROM dbo.GenerationVariants gv;
");

            migrationBuilder.DropIndex(
                name: "IX_GenerationVariants_ModelId_VariantType_BodyStyleId_DoorsCount",
                table: "GenerationVariants");

            migrationBuilder.DropIndex(
                name: "IX_GenerationVariants_ModelId",
                table: "GenerationVariants");

            migrationBuilder.AlterColumn<int>(
                name: "GenerationId",
                table: "GenerationVariants",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GenerationVariants_Generations_GenerationId",
                table: "GenerationVariants",
                column: "GenerationId",
                principalTable: "Generations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_GenerationVariants_GenerationId",
                table: "GenerationVariants",
                column: "GenerationId");

            migrationBuilder.CreateIndex(
                name: "IX_GenerationVariants_GenerationId_VariantType_BodyStyleId_DoorsCount",
                table: "GenerationVariants",
                columns: new[] { "GenerationId", "VariantType", "BodyStyleId", "DoorsCount" });

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "GenerationVariants");
        }
    }
}
