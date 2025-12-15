using Microsoft.AspNetCore.Mvc;
using MockHttp.Services;

namespace MockHttp.Controllers;

/// <summary>
/// Simulates a CRUD data store for testing REST operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly IMockDataStore _store;
    public DataController(IMockDataStore store) => _store = store;

    /// <summary>
    /// Retrieves all stored items
    /// </summary>
    /// <returns>List of all items with their IDs</returns>
    /// <response code="200">Returns all items</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var items = _store.GetAll();
        return Ok(new
        {
            success = true,
            count = items.Count,
            data = items.Select(kvp => new
            {
                id = kvp.Key,
                value = kvp.Value,
                links = new
                {
                    self = $"/api/data/{kvp.Key}",
                    update = $"/api/data/{kvp.Key}",
                    delete = $"/api/data/{kvp.Key}"
                }
            }).ToArray(),
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Retrieves a specific item by ID
    /// </summary>
    /// <param name="id">The item ID</param>
    /// <returns>The requested item</returns>
    /// <response code="200">Returns the item</response>
    /// <response code="404">If item not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var items = _store.GetAll();
        if (!items.TryGetValue(id, out var value))
            return NotFound(new
            {
                error = "Item not found",
                itemId = id,
                timestamp = DateTime.UtcNow
            });

        return Ok(new
        {
            success = true,
            data = new
            {
                id,
                value,
                createdAt = DateTime.UtcNow.AddMinutes(-30), // Mock creation time
                updatedAt = DateTime.UtcNow.AddMinutes(-5)  // Mock update time
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Creates a new item
    /// </summary>
    /// <param name="request">The item data</param>
    /// <returns>Created item with ID and location</returns>
    /// <response code="201">Item created successfully</response>
    /// <response code="400">If request is invalid</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Value))
            return BadRequest(new
            {
                error = "Value is required and cannot be empty",
                field = "value",
                timestamp = DateTime.UtcNow
            });

        if (request.Value.Length > 1000)
            return BadRequest(new
            {
                error = "Value exceeds maximum length of 1000 characters",
                field = "value",
                maxLength = 1000,
                actualLength = request.Value.Length,
                timestamp = DateTime.UtcNow
            });

        var id = _store.Add(request.Value);
        
        var location = $"/api/data/{id}";
        Response.Headers.Append("Location", location);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            new
            {
                success = true,
                message = "Item created successfully",
                data = new
                {
                    id,
                    value = request.Value,
                    createdAt = DateTime.UtcNow
                },
                links = new
                {
                    self = location,
                    update = location,
                    delete = location,
                    all = "/api/data"
                },
                timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Updates an existing item
    /// </summary>
    /// <param name="id">The item ID to update</param>
    /// <param name="request">The updated item data</param>
    /// <returns>Success status</returns>
    /// <response code="200">Item updated successfully</response>
    /// <response code="400">If request is invalid</response>
    /// <response code="404">If item not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] UpdateItemRequest request)
    {
        if (id == Guid.Empty)
            return BadRequest(new
            {
                error = "Invalid ID",
                field = "id",
                timestamp = DateTime.UtcNow
            });

        if (string.IsNullOrWhiteSpace(request?.Value))
            return BadRequest(new
            {
                error = "Value is required and cannot be empty",
                field = "value",
                timestamp = DateTime.UtcNow
            });

        if (request.Value.Length > 1000)
            return BadRequest(new
            {
                error = "Value exceeds maximum length of 1000 characters",
                field = "value",
                maxLength = 1000,
                actualLength = request.Value.Length,
                timestamp = DateTime.UtcNow
            });

        if (!_store.Update(id, request.Value))
            return NotFound(new
            {
                error = "Item not found",
                itemId = id,
                timestamp = DateTime.UtcNow
            });

        return Ok(new
        {
            success = true,
            message = "Item updated successfully",
            data = new
            {
                id,
                value = request.Value,
                updatedAt = DateTime.UtcNow
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Partially updates an item
    /// </summary>
    /// <param name="id">The item ID to patch</param>
    /// <param name="request">The partial update data</param>
    /// <returns>Success status</returns>
    /// <response code="200">Item patched successfully</response>
    /// <response code="404">If item not found</response>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Patch(Guid id, [FromBody] PatchItemRequest request)
    {
        var items = _store.GetAll();
        if (!items.TryGetValue(id, out var currentValue))
            return NotFound(new
            {
                error = "Item not found",
                itemId = id,
                timestamp = DateTime.UtcNow
            });

        var newValue = string.IsNullOrWhiteSpace(request?.Value) ? currentValue : request.Value;
        _store.Update(id, newValue);

        return Ok(new
        {
            success = true,
            message = "Item patched successfully",
            data = new
            {
                id,
                value = newValue,
                patchedAt = DateTime.UtcNow
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Deletes an item
    /// </summary>
    /// <param name="id">The item ID to delete</param>
    /// <returns>Success status</returns>
    /// <response code="200">Item deleted successfully</response>
    /// <response code="404">If item not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        if (!_store.Delete(id))
            return NotFound(new
            {
                error = "Item not found",
                itemId = id,
                timestamp = DateTime.UtcNow
            });

        return Ok(new
        {
            success = true,
            message = "Item deleted successfully",
            data = new { id, deletedAt = DateTime.UtcNow },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Deletes all items
    /// </summary>
    /// <returns>Count of deleted items</returns>
    /// <response code="200">All items deleted successfully</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult DeleteAll()
    {
        var items = _store.GetAll();
        var count = items.Count;
        
        foreach (var id in items.Keys.ToList())
        {
            _store.Delete(id);
        }

        return Ok(new
        {
            success = true,
            message = "All items deleted successfully",
            deletedCount = count,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Uploads a file with additional metadata using multipart/form-data
    /// </summary>
    /// <param name="request">The upload request containing file and metadata</param>
    /// <returns>Upload result with metadata</returns>
    /// <response code="201">File uploaded successfully</response>
    /// <response code="400">If file is missing or validation fails</response>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadWithMetadata([FromForm] UploadFileRequest request)
    {
        var file = request.File;
        var title = request.Title;
        var description = request.Description;
        var category = request.Category;
        var tags = request.Tags;
        var isPublic = request.IsPublic;

        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                error = "No file provided or file is empty",
                field = "file",
                timestamp = DateTime.UtcNow
            });
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return BadRequest(new
            {
                error = "Title is required",
                field = "title",
                timestamp = DateTime.UtcNow
            });
        }

        if (title.Length > 200)
        {
            return BadRequest(new
            {
                error = "Title exceeds maximum length of 200 characters",
                field = "title",
                maxLength = 200,
                actualLength = title.Length,
                timestamp = DateTime.UtcNow
            });
        }

        if (file.Length > 50 * 1024 * 1024) // 50 MB
        {
            return BadRequest(new
            {
                error = "File size exceeds 50MB limit",
                field = "file",
                maxSize = "50MB",
                actualSize = FormatFileSize(file.Length),
                timestamp = DateTime.UtcNow
            });
        }

        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".csv", ".json", ".xml" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest(new
            {
                error = "File type not allowed",
                field = "file",
                providedExtension = fileExtension,
                allowedExtensions,
                timestamp = DateTime.UtcNow
            });
        }

        var itemId = Guid.NewGuid();
        var uploadId = Guid.NewGuid();
        var tagList = string.IsNullOrWhiteSpace(tags) 
            ? Array.Empty<string>() 
            : tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Simulate reading file content (in real scenario, you'd save to storage)
        string fileHash;
        using (var stream = file.OpenReadStream())
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            fileHash = Convert.ToHexString(hashBytes);
        }

        // Store metadata in the data store
        var metadata = System.Text.Json.JsonSerializer.Serialize(new
        {
            uploadId,
            title,
            description,
            category,
            tags = tagList,
            isPublic,
            file = new
            {
                fileName = file.FileName,
                contentType = file.ContentType,
                sizeBytes = file.Length,
                extension = fileExtension,
                hash = fileHash
            }
        });

        _store.Add(metadata);

        var location = $"/api/data/upload/{uploadId}";
        Response.Headers.Append("Location", location);

        return CreatedAtAction(
            nameof(GetById),
            new { id = itemId },
            new
            {
                success = true,
                message = "File uploaded successfully",
                data = new
                {
                    id = itemId,
                    uploadId,
                    title,
                    description,
                    category,
                    tags = tagList,
                    isPublic,
                    file = new
                    {
                        fileName = file.FileName,
                        originalName = file.FileName,
                        contentType = file.ContentType,
                        sizeBytes = file.Length,
                        sizeFormatted = FormatFileSize(file.Length),
                        extension = fileExtension,
                        hash = fileHash
                    },
                    uploadedAt = DateTime.UtcNow
                },
                links = new
                {
                    self = location,
                    download = $"/api/data/download/{uploadId}",
                    delete = $"/api/data/{itemId}"
                },
                timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Uploads multiple files with shared metadata
    /// </summary>
    /// <param name="request">The batch upload request</param>
    /// <returns>Batch upload result</returns>
    /// <response code="201">Files uploaded successfully</response>
    /// <response code="400">If files are missing or validation fails</response>
    [HttpPost("upload/batch")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadBatch([FromForm] UploadBatchRequest request)
    {
        var files = request.Files;
        var title = request.Title;
        var description = request.Description;

        if (files == null || files.Count == 0)
        {
            return BadRequest(new
            {
                error = "No files provided",
                field = "files",
                timestamp = DateTime.UtcNow
            });
        }

        if (files.Count > 10)
        {
            return BadRequest(new
            {
                error = "Maximum 10 files allowed per batch",
                field = "files",
                maxFiles = 10,
                providedFiles = files.Count,
                timestamp = DateTime.UtcNow
            });
        }

        var totalSize = files.Sum(f => f.Length);
        if (totalSize > 100 * 1024 * 1024) // 100 MB total
        {
            return BadRequest(new
            {
                error = "Total file size exceeds 100MB limit",
                field = "files",
                maxTotalSize = "100MB",
                actualTotalSize = FormatFileSize(totalSize),
                timestamp = DateTime.UtcNow
            });
        }

        var batchId = Guid.NewGuid();
        var uploadedFiles = new List<object>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            var fileId = Guid.NewGuid();
            string fileHash;
            
            using (var stream = file.OpenReadStream())
            {
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var hashBytes = await sha256.ComputeHashAsync(stream);
                fileHash = Convert.ToHexString(hashBytes);
            }

            uploadedFiles.Add(new
            {
                id = fileId,
                fileName = file.FileName,
                contentType = file.ContentType,
                sizeBytes = file.Length,
                sizeFormatted = FormatFileSize(file.Length),
                extension = Path.GetExtension(file.FileName).ToLowerInvariant(),
                hash = fileHash,
                downloadUrl = $"/api/data/download/{fileId}"
            });
        }

        return StatusCode(201, new
        {
            success = true,
            message = "Files uploaded successfully",
            data = new
            {
                batchId,
                title,
                description,
                filesCount = uploadedFiles.Count,
                totalSize = totalSize,
                totalSizeFormatted = FormatFileSize(totalSize),
                files = uploadedFiles,
                uploadedAt = DateTime.UtcNow
            },
            timestamp = DateTime.UtcNow
        });
    }

    private static string FormatFileSize(long bytes)
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

public record CreateItemRequest(string Value);
public record UpdateItemRequest(string Value);
public record PatchItemRequest(string? Value);

public record UploadFileRequest(
    IFormFile? File,
    string? Title,
    string? Description,
    string? Category,
    string? Tags,
    bool IsPublic = false
);

public record UploadBatchRequest(
    List<IFormFile>? Files,
    string? Title,
    string? Description
);
