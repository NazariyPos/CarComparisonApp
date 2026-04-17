namespace CarComparisonApi.Models.DTOs
{
    /// <summary>
    /// Represents a generation variant for API responses.
    /// </summary>
    public class GenerationVariantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string VariantType { get; set; } = string.Empty;
        public int YearFrom { get; set; }
        public int YearTo { get; set; }
        public bool IsDefault { get; set; }
        public string? PhotoUrl { get; set; }
        public List<GenerationImageDto> Images { get; set; } = new();
    }
}
