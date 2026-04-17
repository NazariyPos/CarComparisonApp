namespace CarComparisonApi.Models
{
    /// <summary>
    /// Represents a generation phase (pre-facelift/facelift).
    /// </summary>
    public class GenerationVariant
    {
        public int Id { get; set; }
        public int GenerationId { get; set; }
        public Generation? Generation { get; set; }
        public string Name { get; set; } = string.Empty;
        public GenerationVariantType VariantType { get; set; } = GenerationVariantType.Standard;
        public int YearFrom { get; set; }
        public int YearTo { get; set; }
        public bool IsDefault { get; set; }
        public string? PhotoUrl { get; set; }
        public List<GenerationImage> Images { get; set; } = new();
    }
}
