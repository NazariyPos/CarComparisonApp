using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarComparisonApi.Migrations
{
    /// <inheritdoc />
    public partial class RefactorBodyStyleVariants : Migration
    {
        private static readonly string[] columns = new[] { "GenerationId", "VariantType", "BodyStyleId", "DoorsCount" };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BodyStyles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_BodyStyles", x => x.Id));

            migrationBuilder.AddColumn<int>(
                name: "BodyStyleId",
                table: "GenerationVariants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoorsCount",
                table: "GenerationVariants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GenerationVariantId",
                table: "Trims",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BodyStyles_Code",
                table: "BodyStyles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GenerationVariants_BodyStyleId",
                table: "GenerationVariants",
                column: "BodyStyleId");

            migrationBuilder.DropIndex(
                name: "IX_GenerationVariants_GenerationId_VariantType",
                table: "GenerationVariants");

            migrationBuilder.CreateIndex(
                name: "IX_GenerationVariants_GenerationId_VariantType_BodyStyleId_DoorsCount",
                table: "GenerationVariants",
                columns: columns);

            migrationBuilder.CreateIndex(
                name: "IX_Trims_GenerationVariantId",
                table: "Trims",
                column: "GenerationVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_GenerationVariants_BodyStyles_BodyStyleId",
                table: "GenerationVariants",
                column: "BodyStyleId",
                principalTable: "BodyStyles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trims_GenerationVariants_GenerationVariantId",
                table: "Trims",
                column: "GenerationVariantId",
                principalTable: "GenerationVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM dbo.BodyStyles WHERE Code = N'sedan')
BEGIN
    INSERT INTO dbo.BodyStyles (Code, Name)
    VALUES
        (N'sedan', N'Седан'),
        (N'hatchback', N'Хетчбек'),
        (N'wagon', N'Універсал'),
        (N'crossover', N'Кросовер'),
        (N'suv', N'Позашляховик'),
        (N'coupe', N'Купе'),
        (N'liftback', N'Ліфтбек');
END;" );

            migrationBuilder.Sql(@"
UPDATE gv
SET BodyStyleId = bs.Id
FROM dbo.GenerationVariants gv
INNER JOIN dbo.Generations g ON g.Id = gv.GenerationId
INNER JOIN dbo.CarModels m ON m.Id = g.ModelId
INNER JOIN dbo.BodyStyles bs ON bs.Name = CASE
    WHEN m.BodyType LIKE N'%Седан%' THEN N'Седан'
    WHEN m.BodyType LIKE N'%Хетчбек%' THEN N'Хетчбек'
    WHEN m.BodyType LIKE N'%Універсал%' THEN N'Універсал'
    WHEN m.BodyType LIKE N'%Кросовер%' THEN N'Кросовер'
    WHEN m.BodyType LIKE N'%Позашляховик%' THEN N'Позашляховик'
    WHEN m.BodyType LIKE N'%Купе%' THEN N'Купе'
    WHEN m.BodyType LIKE N'%Ліфтбек%' THEN N'Ліфтбек'
    ELSE N'Седан'
END
WHERE gv.BodyStyleId IS NULL;" );

            migrationBuilder.Sql(@"
DECLARE @defaultBodyStyleId INT = (SELECT TOP 1 Id FROM dbo.BodyStyles ORDER BY Id);
UPDATE dbo.GenerationVariants
SET BodyStyleId = @defaultBodyStyleId
WHERE BodyStyleId IS NULL;" );

            migrationBuilder.Sql(@"
UPDATE gv
SET DoorsCount = ISNULL((
    SELECT MAX(t.DoorsCount)
    FROM dbo.Trims t
    WHERE t.GenerationId = gv.GenerationId
), 0)
FROM dbo.GenerationVariants gv
WHERE gv.DoorsCount = 0;" );


            migrationBuilder.Sql(@"
UPDATE t
SET GenerationVariantId = gv.Id
FROM dbo.Trims t
CROSS APPLY (
    SELECT TOP 1 gv2.Id
    FROM dbo.GenerationVariants gv2
    WHERE gv2.GenerationId = t.GenerationId
    ORDER BY gv2.IsDefault DESC, gv2.Id
) gv
WHERE t.GenerationVariantId IS NULL;" );

            migrationBuilder.Sql(@"
UPDATE dbo.Trims
SET GenerationVariantId = (
    SELECT TOP 1 Id
    FROM dbo.GenerationVariants
    ORDER BY IsDefault DESC, Id
)
WHERE GenerationVariantId IS NULL;" );

            migrationBuilder.AlterColumn<int>(
                name: "BodyStyleId",
                table: "GenerationVariants",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GenerationVariantId",
                table: "Trims",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropForeignKey(
                name: "FK_Trims_Generations_GenerationId",
                table: "Trims");

            migrationBuilder.DropIndex(
                name: "IX_Trims_GenerationId",
                table: "Trims");

            migrationBuilder.DropColumn(
                name: "GenerationId",
                table: "Trims");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.SearchCars', 'GenerationVariantId') IS NULL
    ALTER TABLE dbo.SearchCars ADD GenerationVariantId INT NULL;
IF COL_LENGTH('dbo.SearchCars', 'GenerationVariantName') IS NULL
    ALTER TABLE dbo.SearchCars ADD GenerationVariantName NVARCHAR(120) NULL;
IF COL_LENGTH('dbo.SearchCars', 'VariantType') IS NULL
    ALTER TABLE dbo.SearchCars ADD VariantType INT NULL;
IF COL_LENGTH('dbo.SearchCars', 'BodyStyleId') IS NULL
    ALTER TABLE dbo.SearchCars ADD BodyStyleId INT NULL;
IF COL_LENGTH('dbo.SearchCars', 'BodyStyleName') IS NULL
    ALTER TABLE dbo.SearchCars ADD BodyStyleName NVARCHAR(60) NULL;
IF COL_LENGTH('dbo.SearchCars', 'DoorsCount') IS NULL
    ALTER TABLE dbo.SearchCars ADD DoorsCount INT NULL;
IF COL_LENGTH('dbo.SearchCars', 'BodyType') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SearchCars_FilterCore' AND object_id = OBJECT_ID('dbo.SearchCars'))
        DROP INDEX IX_SearchCars_FilterCore ON dbo.SearchCars;
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SearchCars_Facets' AND object_id = OBJECT_ID('dbo.SearchCars'))
        DROP INDEX IX_SearchCars_Facets ON dbo.SearchCars;
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SearchCars_YearRange' AND object_id = OBJECT_ID('dbo.SearchCars'))
        DROP INDEX IX_SearchCars_YearRange ON dbo.SearchCars;
    ALTER TABLE dbo.SearchCars DROP COLUMN BodyType;
END" );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GenerationId",
                table: "Trims",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
UPDATE t
SET GenerationId = gv.GenerationId
FROM dbo.Trims t
INNER JOIN dbo.GenerationVariants gv ON gv.Id = t.GenerationVariantId;" );

            migrationBuilder.DropForeignKey(
                name: "FK_Trims_GenerationVariants_GenerationVariantId",
                table: "Trims");

            migrationBuilder.DropIndex(
                name: "IX_Trims_GenerationVariantId",
                table: "Trims");

            migrationBuilder.DropColumn(
                name: "GenerationVariantId",
                table: "Trims");

            migrationBuilder.DropForeignKey(
                name: "FK_GenerationVariants_BodyStyles_BodyStyleId",
                table: "GenerationVariants");

            migrationBuilder.DropIndex(
                name: "IX_GenerationVariants_BodyStyleId",
                table: "GenerationVariants");

            migrationBuilder.DropIndex(
                name: "IX_GenerationVariants_GenerationId_VariantType_BodyStyleId_DoorsCount",
                table: "GenerationVariants");

            migrationBuilder.CreateIndex(
                name: "IX_GenerationVariants_GenerationId_VariantType",
                table: "GenerationVariants",
                columns: columns);

            migrationBuilder.DropColumn(
                name: "BodyStyleId",
                table: "GenerationVariants");

            migrationBuilder.DropColumn(
                name: "DoorsCount",
                table: "GenerationVariants");

            migrationBuilder.DropTable(
                name: "BodyStyles");

            migrationBuilder.CreateIndex(
                name: "IX_Trims_GenerationId",
                table: "Trims",
                column: "GenerationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trims_Generations_GenerationId",
                table: "Trims",
                column: "GenerationId",
                principalTable: "Generations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

