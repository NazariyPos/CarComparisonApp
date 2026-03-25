using CarComparisonApi.Models;

namespace CarComparisonApi.Models.DTOs
{
    /// <summary>
    /// Full trim response with hierarchy and technical details.
    /// </summary>
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

    /// <summary>
    /// Basic generation data.
    /// </summary>
    public class GenerationBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int YearFrom { get; set; }
        public int YearTo { get; set; }
        public string? PhotoUrl { get; set; }
    }

    /// <summary>
    /// Basic model data.
    /// </summary>
    public class ModelBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BodyType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Basic brand data.
    /// </summary>
    public class BrandBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
