using CarComparisonApi.Models.DTOs;
using CarComparisonApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CarComparisonApi.Controllers
{
    /// <summary>
    /// Provides endpoints for generation variant image gallery management.
    /// </summary>
    [ApiController]
    [Route("api/generations/{generationId:int}/variants/{variantId:int}/images")]
    public class GenerationImagesController : ControllerBase
    {
        private readonly IGenerationImageService _generationImageService;

        public GenerationImagesController(IGenerationImageService generationImageService)
        {
            _generationImageService = generationImageService;
        }

        /// <summary>
        /// Returns all images for a generation variant ordered by primary and sort order.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get generation variant images")]
        [ProducesResponseType(typeof(IEnumerable<GenerationImageDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGenerationImages(int generationId, int variantId)
        {
            if (generationId <= 0 || variantId <= 0)
            {
                return BadRequest("ID мають бути додатними числами");
            }

            var result = await _generationImageService.GetByVariantIdAsync(generationId, variantId);
            return Ok(result);
        }

        /// <summary>
        /// Uploads a new generation variant image and stores its metadata in database.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(Summary = "Upload generation variant image")]
        [ProducesResponseType(typeof(GenerationImageDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadImage(
            int generationId,
            int variantId,
            [FromForm] UploadGenerationImageRequest request)
        {
            if (generationId <= 0 || variantId <= 0)
            {
                return BadRequest("ID мають бути додатними числами");
            }

            if (request.File == null)
            {
                return BadRequest("Файл зображення обов'язковий.");
            }

            try
            {
                var created = await _generationImageService.UploadAsync(generationId, variantId, request.File, request.IsPrimary, request.SortOrder);
                if (created == null)
                {
                    return NotFound("Покоління або його варіант не знайдено");
                }

                return CreatedAtAction(nameof(GetGenerationImages), new { generationId, variantId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Sets selected image as primary for the generation variant.
        /// </summary>
        [HttpPut("{imageId:int}/primary")]
        [Authorize]
        [SwaggerOperation(Summary = "Set generation variant image as primary")]
        [ProducesResponseType(typeof(GenerationImageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetPrimary(int generationId, int variantId, int imageId)
        {
            if (generationId <= 0 || variantId <= 0 || imageId <= 0)
            {
                return BadRequest("ID мають бути додатними числами");
            }

            var updated = await _generationImageService.SetPrimaryAsync(generationId, variantId, imageId);
            if (updated == null)
            {
                return NotFound("Зображення, покоління або варіант не знайдено");
            }

            return Ok(updated);
        }

        /// <summary>
        /// Deletes generation variant image metadata and file from disk.
        /// </summary>
        [HttpDelete("{imageId:int}")]
        [Authorize]
        [SwaggerOperation(Summary = "Delete generation variant image")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteImage(int generationId, int variantId, int imageId)
        {
            if (generationId <= 0 || variantId <= 0 || imageId <= 0)
            {
                return BadRequest("ID мають бути додатними числами");
            }

            var deleted = await _generationImageService.DeleteAsync(generationId, variantId, imageId);
            if (!deleted)
            {
                return NotFound("Зображення, покоління або варіант не знайдено");
            }

            return NoContent();
        }

        /// <summary>
        /// Form payload for uploading a generation image.
        /// </summary>
        public class UploadGenerationImageRequest
        {
            public IFormFile? File { get; set; }
            public bool IsPrimary { get; set; }
            public int? SortOrder { get; set; }
        }
    }
}
