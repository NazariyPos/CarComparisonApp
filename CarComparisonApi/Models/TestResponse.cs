using System;

namespace CarComparisonApi.Models
{
    /// <summary>
    /// Response payload for health/test endpoint.
    /// </summary>
    public class TestResponse
    {
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
