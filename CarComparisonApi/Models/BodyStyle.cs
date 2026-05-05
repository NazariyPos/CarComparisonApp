namespace CarComparisonApi.Models;

/// <summary>
/// Represents a body style type (sedan, wagon, hatchback, etc.).
/// </summary>
public class BodyStyle
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<GenerationVariant> GenerationVariants { get; set; } = new();
}
