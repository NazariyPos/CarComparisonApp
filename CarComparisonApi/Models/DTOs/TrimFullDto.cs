using CarComparisonApi.Models;

namespace CarComparisonApi.Models.DTOs
{
    public class TrimFullDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TransmissionType { get; set; } = string.Empty;
        public int? DoorsCount { get; set; }
        public int? SeatsCount { get; set; }

        public GenerationBasicDto Generation { get; set; } = new();
        public ModelBasicDto Model { get; set; } = new();
        public BrandBasicDto Brand { get; set; } = new();
        public TechnicalDetails? TechnicalDetails { get; set; }
    }

    public class GenerationBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int YearFrom { get; set; }
        public int YearTo { get; set; }
        public string? PhotoUrl { get; set; }
    }

    public class ModelBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BodyType { get; set; } = string.Empty;
    }

    public class BrandBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
