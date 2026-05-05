using System.ComponentModel.DataAnnotations.Schema;

namespace CarComparisonApi.Models
{
    /// <summary>
    /// Represents a model generation.
    /// </summary>
    public class Generation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ModelId { get; set; }
        public CarModel? Model { get; set; }
        public int YearFrom { get; set; }
        public int YearTo { get; set; }
        public string? PhotoUrl { get; set; } = string.Empty;
        public List<GenerationVariant> Variants { get; set; } = new();

        [NotMapped]
        public List<Trim> Trims { get; set; } = new();
    }
}
