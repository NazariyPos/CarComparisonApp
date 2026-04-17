namespace CarComparisonApi.Models.DTOs
{
    /// <summary>
    /// Represents generation image metadata returned by API.
    /// </summary>
    public class GenerationImageDto
    {
        public int Id { get; set; }
        public int GenerationVariantId { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
