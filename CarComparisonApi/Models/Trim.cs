namespace CarComparisonApi.Models
{
    /// <summary>
    /// Represents a trim level for a specific generation variant.
    /// </summary>
    public class Trim
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int GenerationVariantId { get; set; }
        public GenerationVariant? GenerationVariant { get; set; }
        public string? TransmissionType { get; set; }
        public int? DoorsCount { get; set; }
        public int? SeatsCount { get; set; }
        public TechnicalDetails? TechnicalDetails { get; set; }
        public List<Favorite> Favorites { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
    }
}
