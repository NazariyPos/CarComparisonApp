using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarComparisonApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGenerationNameAndDoorsCountFromSearchCars : Migration
    {
        private static readonly string[] FilterCoreColumns = new[] { "BrandId", "ModelId", "GenerationId", "YearFrom", "YearTo", "SearchCarId" };
        private static readonly string[] PopularityColumns = new[] { "BrandId", "ModelId", "PopularityScore", "SearchCarId" };
        private static readonly string[] FacetsColumns = new[] { "BrandId", "ModelId", "GenerationId", "BodyStyleId", "VariantType", "FuelType", "TransmissionType" };
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop indices that depend on GenerationName
            migrationBuilder.DropIndex(
                name: "IX_SearchCars_FilterCore",
                table: "SearchCars");

            migrationBuilder.DropIndex(
                name: "IX_SearchCars_Popularity",
                table: "SearchCars");

            migrationBuilder.DropIndex(
                name: "IX_SearchCars_Facets",
                table: "SearchCars");

            // Drop the columns
            migrationBuilder.DropColumn(
                name: "GenerationName",
                table: "SearchCars");

            migrationBuilder.DropColumn(
                name: "DoorsCount",
                table: "SearchCars");

            // Recreate indices without GenerationName and DoorsCount
            migrationBuilder.CreateIndex(
                name: "IX_SearchCars_FilterCore",
                table: "SearchCars",
                columns: FilterCoreColumns,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SearchCars_Popularity",
                table: "SearchCars",
                columns: PopularityColumns,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SearchCars_Facets",
                table: "SearchCars",
                columns: FacetsColumns,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop recreated indices
            migrationBuilder.DropIndex(
                name: "IX_SearchCars_FilterCore",
                table: "SearchCars");

            migrationBuilder.DropIndex(
                name: "IX_SearchCars_Popularity",
                table: "SearchCars");

            migrationBuilder.DropIndex(
                name: "IX_SearchCars_Facets",
                table: "SearchCars");

            // Re-add columns with default values
            migrationBuilder.AddColumn<string>(
                name: "GenerationName",
                table: "SearchCars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DoorsCount",
                table: "SearchCars",
                type: "int",
                nullable: true);

            // Recreate original indices
            migrationBuilder.CreateIndex(
                name: "IX_SearchCars_FilterCore",
                table: "SearchCars",
                columns: FilterCoreColumns,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SearchCars_Popularity",
                table: "SearchCars",
                columns: PopularityColumns,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SearchCars_Facets",
                table: "SearchCars",
                columns: FacetsColumns,
                filter: "[IsActive] = 1");
        }
    }
}
