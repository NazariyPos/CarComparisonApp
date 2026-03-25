using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace CarComparisonApi.Models
{
    /// <summary>
    /// Represents a trim level for a specific generation.
    /// </summary>
    public class Trim
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int GenerationId { get; set; }
        public string? TransmissionType { get; set; }
        public int? DoorsCount { get; set; }
        public int? SeatsCount { get; set; }
        public TechnicalDetails? TechnicalDetails { get; set; }
        public List<Review> Reviews { get; set; } = new();
    }
}
