namespace CarComparisonApi.Models;

/// <summary>
/// Denormalized read-model row used for fast catalog search.
/// One record equals one trim.
/// </summary>
public class SearchCar
{
    public long SearchCarId { get; set; }
    public int TrimId { get; set; }

    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;

    public int ModelId { get; set; }
    public string ModelName { get; set; } = string.Empty;

    public int GenerationId { get; set; }
    public string GenerationName { get; set; } = string.Empty;

    public string TrimName { get; set; } = string.Empty;

    public int? GenerationVariantId { get; set; }
    public string? GenerationVariantName { get; set; }
    public GenerationVariantType? VariantType { get; set; }
    public int? BodyStyleId { get; set; }
    public string? BodyStyleName { get; set; }
    public int? DoorsCount { get; set; }
    public string? FuelType { get; set; }
    public string? TransmissionType { get; set; }

    public int YearFrom { get; set; }
    public int YearTo { get; set; }

    public string? PhotoUrl { get; set; }
    public int PopularityScore { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
