namespace CarComparisonApi.Models.DTOs
{
    /// <summary>
    /// Lightweight brand representation for API responses.
    /// </summary>
    public class BrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
