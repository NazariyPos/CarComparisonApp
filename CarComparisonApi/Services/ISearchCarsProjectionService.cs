namespace CarComparisonApi.Services;

/// <summary>
/// Maintains denormalized search read-model (SearchCars table).
/// </summary>
public interface ISearchCarsProjectionService
{
    Task EnsureInitializedAsync(CancellationToken cancellationToken = default);
    Task RebuildAsync(CancellationToken cancellationToken = default);
}
