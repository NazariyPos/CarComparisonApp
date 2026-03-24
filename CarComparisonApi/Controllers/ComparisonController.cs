using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarComparisonApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarComparisonApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComparisonController : ControllerBase
    {
        private readonly ICarService _carService;

        public ComparisonController(ICarService carService)
        {
            _carService = carService;
        }

        [HttpGet("compare")]
        public async Task<IActionResult> Compare([FromQuery] string trimIds)
        {
            var ids = trimIds.Split(',')
                .Select(id => int.TryParse(id, out var num) ? num : -1)
                .Where(id => id > 0)
                .ToList();

            if (ids.Count == 0 || ids.Count > 4)
                return BadRequest("����� ���������� �� 1 �� 4 ������������");

            var trims = await _carService.GetTrimsForComparisonAsync(ids);

            var comparisonResult = new
            {
                Trims = trims,
                Highlights = GetHighlights(trims)
            };

            return Ok(comparisonResult);
        }

        private static Dictionary<string, List<int>> GetHighlights(IEnumerable<CarComparisonApi.Models.Trim> trims)
        {
            var highlights = new Dictionary<string, List<int>>();
            var trimList = trims.ToList();

            if (trimList.All(t => t.TechnicalDetails != null))
            {
                var maxSpeeds = trimList.ConvertAll(t => t.TechnicalDetails!.MaxSpeed ?? 0);
                HighlightParameter("MaxSpeed", maxSpeeds, true, highlights);

                var accelerations = trimList.ConvertAll(t => t.TechnicalDetails!.Acceleration0To100 ?? decimal.MaxValue);
                HighlightParameter("Acceleration0To100", accelerations, false, highlights);

                var powers = trimList.ConvertAll(t => t.TechnicalDetails!.Power ?? 0);
                HighlightParameter("Power", powers, true, highlights);

                var torques = trimList.ConvertAll(t => t.TechnicalDetails!.Torque ?? 0);
                HighlightParameter("Torque", torques, true, highlights);

                var fuelConsumptions = trimList.ConvertAll(t => t.TechnicalDetails!.FuelConsumptionMixed ?? decimal.MaxValue);
                HighlightParameter("FuelConsumption", fuelConsumptions, false, highlights);
            }

            return highlights;
        }

        private static void HighlightParameter<T>(string parameterName, List<T> values, bool higherIsBetter,
            Dictionary<string, List<int>> highlights) where T : IComparable
        {
            if (values.Count == 0) return;

            T bestValue = values[0];
            T worstValue = values[0];
            var bestIndices = new List<int> { 0 };
            var worstIndices = new List<int> { 0 };

            for (int i = 1; i < values.Count; i++)
            {
                int comparison = values[i].CompareTo(bestValue);
                if ((higherIsBetter && comparison > 0) || (!higherIsBetter && comparison < 0))
                {
                    bestValue = values[i];
                    bestIndices = new List<int> { i };
                }
                else if (comparison == 0)
                {
                    bestIndices.Add(i);
                }

                comparison = values[i].CompareTo(worstValue);
                if ((higherIsBetter && comparison < 0) || (!higherIsBetter && comparison > 0))
                {
                    worstValue = values[i];
                    worstIndices = new List<int> { i };
                }
                else if (comparison == 0)
                {
                    worstIndices.Add(i);
                }
            }

            if (bestIndices.Count > 0 && values.Count > 1)
            {
                highlights[$"{parameterName}_Best"] = bestIndices;
            }

            if (worstIndices.Count > 0 && values.Count > 1)
            {
                highlights[$"{parameterName}_Worst"] = worstIndices;
            }
        }
    }
}
