using CarComparisonApi.Data;
using CarComparisonApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CarComparisonApi.Services;

/// <summary>
/// Builds and updates SearchCars denormalized read-model.
/// </summary>
public class SearchCarsProjectionService : ISearchCarsProjectionService
{
    private readonly CarComparisonDbContext _dbContext;
    private readonly ILogger<SearchCarsProjectionService> _logger;

    public SearchCarsProjectionService(
        CarComparisonDbContext dbContext,
        ILogger<SearchCarsProjectionService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        var trimsCount = await _dbContext.Trims.AsNoTracking().CountAsync(cancellationToken);
        var searchCarsCount = await _dbContext.SearchCars.AsNoTracking().CountAsync(cancellationToken);
        var rowsWithMissingVariantData = await _dbContext.SearchCars
            .AsNoTracking()
            .Where(x => x.GenerationVariantId == null)
            .CountAsync(cancellationToken);

        if (trimsCount == 0)
        {
            _logger.LogInformation("SearchCars initialization skipped: source trims table is empty");
            return;
        }

        if (trimsCount != searchCarsCount || rowsWithMissingVariantData > 0)
        {
            _logger.LogInformation(
                "SearchCars rebuild triggered (trims: {TrimsCount}, searchCars: {SearchCarsCount}, staleRows: {StaleRows})",
                trimsCount,
                searchCarsCount,
                rowsWithMissingVariantData);
            await RebuildAsync(cancellationToken);
        }
    }

    public async Task RebuildAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        var trims = await _dbContext.Trims
            .AsNoTracking()
            .Include(t => t.TechnicalDetails)
            .Include(t => t.GenerationVariant)
                .ThenInclude(v => v!.Images)
            .Include(t => t.GenerationVariant)
                .ThenInclude(v => v!.Model)
                    .ThenInclude(m => m!.Brand)
            .Include(t => t.GenerationVariant)
                .ThenInclude(v => v!.BodyStyle)
            .ToListAsync(cancellationToken);

        var rows = trims
            .Where(t => t.GenerationVariant?.Model?.Brand != null)
            .Select(t =>
            {
                var model = t.GenerationVariant!.Model!;
                var brand = model.Brand!;
                var variant = t.GenerationVariant!;

                var photoUrl = variant.Images.FirstOrDefault(i => i.IsPrimary)?.Url
                    ?? variant.PhotoUrl
                    ?? variant.PhotoUrl;

                return new SearchCar
                {
                    TrimId = t.Id,
                    BrandId = brand.Id,
                    BrandName = brand.Name,
                    ModelId = model.Id,
                    ModelName = model.Name,
                    GenerationId = variant.Id,
                    TrimName = t.Name,
                    GenerationVariantId = variant.Id,
                    GenerationVariantName = variant.Name,
                    VariantType = variant.VariantType,
                    BodyStyleId = variant.BodyStyleId,
                    BodyStyleName = variant.BodyStyle?.Name,
                    FuelType = t.TechnicalDetails?.FuelType,
                    TransmissionType = t.TransmissionType,
                    YearFrom = variant.YearFrom,
                    YearTo = variant.YearTo,
                    PhotoUrl = photoUrl,
                    PopularityScore = 0,
                    IsActive = true,
                    UpdatedAtUtc = DateTime.UtcNow,
                };
            })
            .ToList();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM dbo.SearchCars", cancellationToken);
        await _dbContext.SearchCars.AddRangeAsync(rows, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("SearchCars projection rebuilt successfully. Rows: {RowsCount}", rows.Count);
    }

    private async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        const string sql = @"
IF OBJECT_ID(N'dbo.SearchCars', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SearchCars
    (
        SearchCarId BIGINT IDENTITY(1,1) NOT NULL,
        TrimId INT NOT NULL,
        BrandId INT NOT NULL,
        BrandName NVARCHAR(120) NOT NULL,
        ModelId INT NOT NULL,
        ModelName NVARCHAR(120) NOT NULL,
        GenerationId INT NOT NULL,
        TrimName NVARCHAR(150) NOT NULL,
        GenerationVariantId INT NULL,
        GenerationVariantName NVARCHAR(120) NULL,
        VariantType INT NULL,
        BodyStyleId INT NULL,
        BodyStyleName NVARCHAR(60) NULL,
        FuelType NVARCHAR(60) NULL,
        TransmissionType NVARCHAR(60) NULL,
        YearFrom INT NOT NULL,
        YearTo INT NOT NULL,
        PhotoUrl NVARCHAR(500) NULL,
        PopularityScore INT NOT NULL CONSTRAINT DF_SearchCars_PopularityScore DEFAULT (0),
        IsActive BIT NOT NULL CONSTRAINT DF_SearchCars_IsActive DEFAULT (1),
        UpdatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_SearchCars_UpdatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_SearchCars PRIMARY KEY CLUSTERED (SearchCarId),
        CONSTRAINT UQ_SearchCars_TrimId UNIQUE (TrimId)
    );
END;

IF COL_LENGTH('dbo.SearchCars', 'GenerationVariantId') IS NULL
    ALTER TABLE dbo.SearchCars ADD GenerationVariantId INT NULL;
IF COL_LENGTH('dbo.SearchCars', 'GenerationVariantName') IS NULL
    ALTER TABLE dbo.SearchCars ADD GenerationVariantName NVARCHAR(120) NULL;
IF COL_LENGTH('dbo.SearchCars', 'VariantType') IS NULL
    ALTER TABLE dbo.SearchCars ADD VariantType INT NULL;
IF COL_LENGTH('dbo.SearchCars', 'BodyStyleId') IS NULL
    ALTER TABLE dbo.SearchCars ADD BodyStyleId INT NULL;
IF COL_LENGTH('dbo.SearchCars', 'BodyStyleName') IS NULL
    ALTER TABLE dbo.SearchCars ADD BodyStyleName NVARCHAR(60) NULL;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SearchCars_FilterCore' AND object_id = OBJECT_ID(N'dbo.SearchCars'))
    DROP INDEX IX_SearchCars_FilterCore ON dbo.SearchCars;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SearchCars_Facets' AND object_id = OBJECT_ID(N'dbo.SearchCars'))
    DROP INDEX IX_SearchCars_Facets ON dbo.SearchCars;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SearchCars_Popularity' AND object_id = OBJECT_ID(N'dbo.SearchCars'))
    DROP INDEX IX_SearchCars_Popularity ON dbo.SearchCars;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SearchCars_YearRange' AND object_id = OBJECT_ID(N'dbo.SearchCars'))
    DROP INDEX IX_SearchCars_YearRange ON dbo.SearchCars;

CREATE NONCLUSTERED INDEX IX_SearchCars_FilterCore
ON dbo.SearchCars (BrandId, ModelId, GenerationId, YearFrom, YearTo, SearchCarId)
INCLUDE (BodyStyleId, VariantType, FuelType, TransmissionType, BrandName, ModelName, GenerationVariantName, TrimName, PhotoUrl, PopularityScore)
WHERE IsActive = 1;

CREATE NONCLUSTERED INDEX IX_SearchCars_Facets
ON dbo.SearchCars (BrandId, ModelId, GenerationId, BodyStyleId, VariantType, FuelType, TransmissionType)
WHERE IsActive = 1;

CREATE NONCLUSTERED INDEX IX_SearchCars_Popularity
ON dbo.SearchCars (BrandId, ModelId, PopularityScore DESC, SearchCarId)
INCLUDE (GenerationId, TrimId, BrandName, ModelName, GenerationVariantName, TrimName, PhotoUrl, YearFrom, YearTo)
WHERE IsActive = 1;

CREATE NONCLUSTERED INDEX IX_SearchCars_YearRange
ON dbo.SearchCars (YearFrom, YearTo, SearchCarId)
INCLUDE (BrandId, ModelId, GenerationId, BodyStyleId, VariantType, FuelType, TransmissionType)
WHERE IsActive = 1;";

        await _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
