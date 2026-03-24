using CarComparisonApi.Models.DTOs;

namespace CarComparisonApi.Models.DTOs
{
    public class GenerationWithTrimsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int YearFrom { get; set; }
        public int YearTo { get; set; }
        public string? PhotoUrl { get; set; }

        public BrandDto Brand { get; set; } = new();
        public ModelDto Model { get; set; } = new();
        public List<TrimBasicDto> Trims { get; set; } = new();
    }

    public class TrimBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TransmissionType { get; set; } = string.Empty;
        public int? DoorsCount { get; set; }
        public int? SeatsCount { get; set; }
    }
}
