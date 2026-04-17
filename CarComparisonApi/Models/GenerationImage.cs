namespace CarComparisonApi.Models
{
    /// <summary>
    /// Represents an image attached to a car generation.
    /// </summary>
    public class GenerationImage
    {
        public int Id { get; set; }
        public int GenerationVariantId { get; set; }
        public GenerationVariant? GenerationVariant { get; set; }
        public string Url { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
