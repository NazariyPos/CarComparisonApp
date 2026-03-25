namespace CarComparisonApi.Models
{
    /// <summary>
    /// Represents a car brand with related models.
    /// </summary>
    public class CarBrand
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<CarModel> Models { get; set; } = new();
    }
}
