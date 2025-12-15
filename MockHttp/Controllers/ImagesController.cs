using Microsoft.AspNetCore.Mvc;
using MockHttp.Services;

namespace MockHttp.Controllers;

/// <summary>
/// Provides mock image responses in various formats for testing binary content handling
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ImagesController(IImageService imageService) : ControllerBase
{
    /// <summary>
    /// Returns a 1x1 pixel PNG image
    /// </summary>
    /// <param name="seed">Optional seed for consistent generation (for testing)</param>
    /// <returns>PNG image file</returns>
    /// <response code="200">Returns PNG image</response>
    [HttpGet("png")]
    [Produces("image/png")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPng([FromQuery] int? seed = null)
    {
        var bytes = imageService.GetPng();
        Response.Headers.Append("X-Image-Type", "PNG");
        Response.Headers.Append("X-Image-Size", $"{bytes.Length} bytes");
        if (seed.HasValue)
            Response.Headers.Append("X-Image-Seed", seed.Value.ToString());
        
        return File(bytes, "image/png", "mock-image.png");
    }

    /// <summary>
    /// Returns a 1x1 pixel JPEG image
    /// </summary>
    /// <param name="id">Optional identifier for cache testing</param>
    /// <returns>JPEG image file</returns>
    /// <response code="200">Returns JPEG image</response>
    [HttpGet("jpeg")]
    [Produces("image/jpeg")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetJpeg([FromQuery] int? id = null)
    {
        var bytes = imageService.GetJpeg();
        Response.Headers.Append("X-Image-Type", "JPEG");
        Response.Headers.Append("X-Image-Size", $"{bytes.Length} bytes");
        if (id.HasValue)
            Response.Headers.Append("X-Image-Id", id.Value.ToString());
        
        return File(bytes, "image/jpeg", "mock-image.jpg");
    }

    /// <summary>
    /// Returns image with specified dimensions info (still returns 1x1 actual image)
    /// </summary>
    /// <param name="width">Simulated width</param>
    /// <param name="height">Simulated height</param>
    /// <param name="format">Image format (png or jpeg)</param>
    /// <returns>Image file with dimension headers</returns>
    /// <response code="200">Returns image</response>
    /// <response code="400">If parameters are invalid</response>
    [HttpGet("{width:int}x{height:int}.{format}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetCustomImage(int width, int height, string format)
    {
        if (width <= 0 || width > 5000)
            return BadRequest(new { error = "width must be between 1 and 5000", field = "width" });
        
        if (height <= 0 || height > 5000)
            return BadRequest(new { error = "height must be between 1 and 5000", field = "height" });

        var normalizedFormat = format.ToLowerInvariant();
        if (normalizedFormat != "png" && normalizedFormat != "jpeg" && normalizedFormat != "jpg")
            return BadRequest(new { error = "format must be png or jpeg", field = "format" });

        Response.Headers.Append("X-Image-Dimensions", $"{width}x{height}");
        Response.Headers.Append("X-Image-Format", normalizedFormat);
        Response.Headers.Append("Cache-Control", "public, max-age=86400");

        if (normalizedFormat == "png")
        {
            return File(imageService.GetPng(), "image/png", $"mock-{width}x{height}.png");
        }
        
        return File(imageService.GetJpeg(), "image/jpeg", $"mock-{width}x{height}.jpg");
    }

    /// <summary>
    /// Simulates image upload endpoint
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <returns>Upload result with metadata</returns>
    /// <response code="200">Image uploaded successfully</response>
    /// <response code="400">If file is missing or invalid</response>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided or file is empty", field = "file" });

        var allowedTypes = new[] { "image/png", "image/jpeg", "image/jpg", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest(new { error = "Invalid file type. Only images are allowed.", field = "file" });

        if (file.Length > 10 * 1024 * 1024) // 10 MB
            return BadRequest(new { error = "File size exceeds 10MB limit", field = "file" });

        var mockId = Guid.NewGuid();
        
        // Simulate processing time
        await Task.Delay(100);

        return Ok(new
        {
            success = true,
            message = "Image uploaded successfully",
            data = new
            {
                id = mockId,
                fileName = file.FileName,
                contentType = file.ContentType,
                sizeBytes = file.Length,
                sizeFormatted = FormatBytes(file.Length),
                uploadedAt = DateTime.UtcNow,
                url = $"/api/images/{mockId}",
                thumbnailUrl = $"/api/images/{mockId}/thumbnail"
            }
        });
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
