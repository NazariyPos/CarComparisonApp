using System.ComponentModel.DataAnnotations.Schema;

namespace CarComparisonApi.Models
{
    /// <summary>
    /// Represents a car model within a brand.
    /// </summary>
    public class CarModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public CarBrand? Brand { get; set; }
        public string? BodyType { get; set; }
        public List<GenerationVariant> GenerationVariants { get; set; } = new();

        [NotMapped]
        public List<GenerationVariant> Generations
        {
            get => GenerationVariants;
            set => GenerationVariants = value;
        }
    }
}
