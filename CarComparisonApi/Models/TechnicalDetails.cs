namespace CarComparisonApi.Models
{
    public class TechnicalDetails
    {
        public int Id { get; set; }
        public int TrimId { get; set; }

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