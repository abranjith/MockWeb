using Microsoft.AspNetCore.Mvc;

namespace MockHttp.Controllers;

/// <summary>
/// Handles cookie operations for testing cookie management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CookiesController : ControllerBase
{
    /// <summary>
    /// Returns all cookies sent in the request
    /// </summary>
    /// <returns>Dictionary of cookie names and values</returns>
    /// <response code="200">Returns all cookies</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCookies()
    {
        var cookies = Request.Cookies
            .ToDictionary(c => c.Key, c => c.Value);

        return Ok(new
        {
            success = true,
            count = cookies.Count,
            cookies,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns a specific cookie value
    /// </summary>
    /// <param name="name">Cookie name</param>
    /// <returns>Cookie value if found</returns>
    /// <response code="200">Returns cookie value</response>
    /// <response code="404">If cookie not found</response>
    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetCookie(string name)
    {
        if (!Request.Cookies.TryGetValue(name, out var value))
        {
            return NotFound(new
            {
                error = "Cookie not found",
                cookieName = name,
                availableCookies = Request.Cookies.Keys.ToArray(),
                timestamp = DateTime.UtcNow
            });
        }

        return Ok(new
        {
            success = true,
            name,
            value,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Sets cookies from query parameters
    /// </summary>
    /// <returns>Confirmation of set cookies</returns>
    /// <response code="200">Cookies set successfully</response>
    /// <response code="400">If no cookies provided</response>
    /// <remarks>
    /// Example: /api/cookies/set?session=abc123&amp;user=john
    /// </remarks>
    [HttpGet("set")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SetCookies()
    {
        if (Request.Query.Count == 0)
        {
            return BadRequest(new
            {
                error = "No cookies provided",
                message = "Use query parameters to set cookies. Example: ?name=value&another=value2",
                timestamp = DateTime.UtcNow
            });
        }

        var setCookies = new Dictionary<string, string>();
        
        foreach (var query in Request.Query)
        {
            var options = new CookieOptions
            {
                HttpOnly = false,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };
            
            Response.Cookies.Append(query.Key, query.Value.ToString(), options);
            setCookies[query.Key] = query.Value.ToString();
        }

        return Ok(new
        {
            success = true,
            message = "Cookies set successfully",
            count = setCookies.Count,
            cookies = setCookies,
            expires = DateTimeOffset.UtcNow.AddDays(7),
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Sets a single cookie with custom options
    /// </summary>
    /// <param name="request">Cookie configuration</param>
    /// <returns>Confirmation of set cookie</returns>
    /// <response code="200">Cookie set successfully</response>
    /// <response code="400">If request is invalid</response>
    [HttpPost("set")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SetCookie([FromBody] SetCookieRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Name))
        {
            return BadRequest(new
            {
                error = "Cookie name is required",
                field = "name",
                timestamp = DateTime.UtcNow
            });
        }

        if (string.IsNullOrWhiteSpace(request.Value))
        {
            return BadRequest(new
            {
                error = "Cookie value is required",
                field = "value",
                timestamp = DateTime.UtcNow
            });
        }

        var options = new CookieOptions
        {
            HttpOnly = request.HttpOnly,
            Secure = request.Secure,
            SameSite = request.SameSite switch
            {
                "None" => SameSiteMode.None,
                "Strict" => SameSiteMode.Strict,
                _ => SameSiteMode.Lax
            },
            Expires = request.ExpiresInDays.HasValue 
                ? DateTimeOffset.UtcNow.AddDays(request.ExpiresInDays.Value) 
                : null,
            Path = request.Path ?? "/",
            Domain = request.Domain
        };

        Response.Cookies.Append(request.Name, request.Value, options);

        return Ok(new
        {
            success = true,
            message = "Cookie set successfully",
            cookie = new
            {
                name = request.Name,
                value = request.Value,
                options = new
                {
                    httpOnly = options.HttpOnly,
                    secure = options.Secure,
                    sameSite = options.SameSite.ToString(),
                    expires = options.Expires,
                    path = options.Path,
                    domain = options.Domain
                }
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Deletes cookies specified in query parameters
    /// </summary>
    /// <returns>Confirmation of deleted cookies</returns>
    /// <response code="200">Cookies deleted successfully</response>
    /// <response code="400">If no cookies specified</response>
    /// <remarks>
    /// Example: /api/cookies/delete?name=session&amp;name=user
    /// </remarks>
    [HttpGet("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult DeleteCookies([FromQuery(Name = "name")] string[]? names)
    {
        if (names == null || names.Length == 0)
        {
            return BadRequest(new
            {
                error = "No cookie names provided",
                message = "Use query parameters to specify cookies. Example: ?name=session&name=user",
                timestamp = DateTime.UtcNow
            });
        }

        var deleted = new List<string>();
        
        foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            Response.Cookies.Delete(name);
            deleted.Add(name);
        }

        return Ok(new
        {
            success = true,
            message = "Cookies deleted successfully",
            count = deleted.Count,
            deleted,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Deletes a specific cookie
    /// </summary>
    /// <param name="name">Cookie name to delete</param>
    /// <returns>Confirmation of deletion</returns>
    /// <response code="200">Cookie deleted successfully</response>
    /// <response code="400">If cookie name is invalid</response>
    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult DeleteCookie(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                error = "Cookie name cannot be empty",
                field = "name",
                timestamp = DateTime.UtcNow
            });
        }

        Response.Cookies.Delete(name);

        return Ok(new
        {
            success = true,
            message = "Cookie deleted successfully",
            deleted = name,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Deletes all cookies
    /// </summary>
    /// <returns>Confirmation of deletion</returns>
    /// <response code="200">All cookies deleted</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult DeleteAllCookies()
    {
        var cookieNames = Request.Cookies.Keys.ToList();
        
        foreach (var name in cookieNames)
        {
            Response.Cookies.Delete(name);
        }

        return Ok(new
        {
            success = true,
            message = "All cookies deleted successfully",
            count = cookieNames.Count,
            deleted = cookieNames,
            timestamp = DateTime.UtcNow
        });
    }
}

public record SetCookieRequest(
    string Name,
    string Value,
    bool HttpOnly = false,
    bool Secure = false,
    string SameSite = "Lax",
    int? ExpiresInDays = 7,
    string? Path = "/",
    string? Domain = null
);
