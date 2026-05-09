using System.ComponentModel.DataAnnotations.Schema;

namespace CarComparisonApi.Models
{
    /// <summary>
    /// Represents a generation phase and body style combination.
    /// </summary>
    public class GenerationVariant
    {
        public int Id { get; set; }
        public int ModelId { get; set; }
        public CarModel? Model { get; set; }
        [NotMapped]
        public int GenerationId
        {
            get => ModelId;
            set => ModelId = value;
        }

        [NotMapped]
        public Generation? Generation { get; set; }
        public int BodyStyleId { get; set; }
        public BodyStyle BodyStyle { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public GenerationVariantType VariantType { get; set; } = GenerationVariantType.Standard;
        public int YearFrom { get; set; }
        public int YearTo { get; set; }
        public bool IsDefault { get; set; }
        public string? PhotoUrl { get; set; }
        public List<GenerationImage> Images { get; set; } = new();
        public List<Trim> Trims { get; set; } = new();
        public int DoorsCount { get; set; }
    }
}
