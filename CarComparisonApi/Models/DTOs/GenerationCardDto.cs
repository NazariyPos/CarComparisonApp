namespace CarComparisonApi.Models.DTOs
{
    /// <summary>
    /// Card-like summary of a generation used in search responses.
    /// </summary>
    public class GenerationCardDto
    {
        public int GenerationId { get; set; }
        public string GenerationName { get; set; } = string.Empty;
        public int YearFrom { get; set; }
        public int YearTo { get; set; }
        public string? PhotoUrl { get; set; }

        public int ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string BodyType { get; set; } = string.Empty;

        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;

        public int TrimCount { get; set; }
    }
}
