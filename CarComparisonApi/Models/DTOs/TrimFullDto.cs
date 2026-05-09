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
        public GenerationVariantBasicDto GenerationVariant { get; set; } = new();
        public ModelBasicDto Model { get; set; } = new();
        public BrandBasicDto Brand { get; set; } = new();
        public TechnicalDetailsFullDto? TechnicalDetails { get; set; }
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
    }

    /// <summary>
    /// Basic brand data.
    /// </summary>
    public class BrandBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Basic generation variant data.
    /// </summary>
    public class GenerationVariantBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string VariantType { get; set; } = string.Empty;
        public int BodyStyleId { get; set; }
        public string BodyStyleName { get; set; } = string.Empty;
        public int DoorsCount { get; set; }
    }

    /// <summary>
    /// Full technical details for a trim.
    /// </summary>
    public class TechnicalDetailsFullDto
    {
        public int? MaxSpeed { get; set; }
        public decimal? Acceleration0To100 { get; set; }

        public string? EngineCode { get; set; }
        public string? EngineType { get; set; }
        public int? CylindersCount { get; set; }
        public int? ValvesCount { get; set; }
        public decimal? CompressionRatio { get; set; }
        public string? FuelType { get; set; }
        public int? Power { get; set; }
        public int? Torque { get; set; }
        public int? MaxPowerAtRPM { get; set; }
        public int? MaxTorqueAtRPM { get; set; }
        public decimal? EngineDisplacement { get; set; }

        public string? DriveType { get; set; }

        public decimal? FuelConsumptionCity { get; set; }
        public decimal? FuelConsumptionMixed { get; set; }
        public decimal? FuelConsumptionHighway { get; set; }
        public decimal? ElectricRange { get; set; }

        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Wheelbase { get; set; }
        public decimal? FrontTrack { get; set; }
        public decimal? RearTrack { get; set; }
        public decimal? CurbWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? FuelTankCapacity { get; set; }
        public decimal? TurningCircle { get; set; }

        public string? FrontBrakes { get; set; }
        public string? RearBrakes { get; set; }

        public string? FrontSuspension { get; set; }
        public string? RearSuspension { get; set; }
    }
}
