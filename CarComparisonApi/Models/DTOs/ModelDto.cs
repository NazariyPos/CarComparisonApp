namespace CarComparisonApi.Models.DTOs
{
    /// <summary>
    /// Lightweight model representation for API responses.
    /// </summary>
    public class ModelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BodyType { get; set; } = string.Empty;
        public int BrandId { get; set; }
    }
}
