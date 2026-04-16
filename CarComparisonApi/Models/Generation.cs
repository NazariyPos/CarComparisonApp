namespace CarComparisonApi.Models
{
    /// <summary>
    /// Represents a model generation and its trims.
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
        public List<Trim> Trims { get; set; } = new();
    }
}
