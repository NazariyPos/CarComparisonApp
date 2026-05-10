namespace CarComparisonApi.Models.DTOs
{
    /// <summary>
    /// Represents favorite trim data for current user.
    /// </summary>
    public class FavoriteDto
    {
        public int Id { get; set; }
        public int TrimId { get; set; }
        public string TrimName { get; set; } = string.Empty;
        public int GenerationId { get; set; }
        public string GenerationName { get; set; } = string.Empty;
        public string DisplayGenerationName { get; set; } = string.Empty;
        public int YearFrom { get; set; }
        public int YearTo { get; set; }
        public int ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public int TrimCount { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
