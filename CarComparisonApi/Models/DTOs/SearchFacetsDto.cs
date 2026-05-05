namespace CarComparisonApi.Models.DTOs
{
    /// <summary>
    /// Aggregated filter values for catalog search UI.
    /// </summary>
    public class SearchFacetsDto
    {
        public List<BodyStyleOptionDto> BodyStyles { get; set; } = new();
        public List<string> VariantTypes { get; set; } = new();
        public List<string> TransmissionTypes { get; set; } = new();
        public List<string> FuelTypes { get; set; } = new();
    }

    /// <summary>
    /// Body style option for search filters.
    /// </summary>
    public class BodyStyleOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}