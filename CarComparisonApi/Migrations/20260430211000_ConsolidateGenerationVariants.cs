using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarComparisonApi.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateGenerationVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration consolidates duplicate GenerationVariants that differ only by having BodyStyle appended to the name.
            // For each variant with ", Седан" (or other body style) in the name, we:
            // 1. Find the matching clean variant (without body style in name)
            // 2. Relink all trims from the duplicate to the clean variant
            // 3. Delete the duplicate variant

            migrationBuilder.Sql(@"
-- Step 1: Create a temporary mapping of variants to consolidate
-- For each 'dirty' variant (with body style in name), find the matching 'clean' variant
WITH CleanVariants AS (
    SELECT Id, GenerationId, VariantType, Name
    FROM dbo.GenerationVariants
    WHERE Name NOT LIKE N'%, %'  -- No comma in name
),
DirtyVariants AS (
    SELECT Id, GenerationId, VariantType, Name
    FROM dbo.GenerationVariants
    WHERE Name LIKE N'%, %'  -- Has comma (contains body style)
)
-- Step 2: Update all trims from dirty to clean variants
UPDATE t
SET GenerationVariantId = cv.Id
FROM dbo.Trims t
INNER JOIN DirtyVariants dv ON t.GenerationVariantId = dv.Id
INNER JOIN CleanVariants cv ON cv.GenerationId = dv.GenerationId 
    AND cv.VariantType = dv.VariantType
WHERE cv.Id <> dv.Id;
");

            migrationBuilder.Sql(@"
-- Step 3: Delete the duplicate variants that now have no trims
WITH UsedVariants AS (
    SELECT DISTINCT GenerationVariantId
    FROM dbo.Trims
)
DELETE FROM dbo.GenerationVariants
WHERE Name LIKE N'%, %'
  AND Id NOT IN (SELECT GenerationVariantId FROM UsedVariants);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback is not straightforward as deleted data cannot be easily restored.
            // In a production scenario, you would need to restore from backup.
            // This migration is intended to be permanent data cleanup.
        }
    }
}
