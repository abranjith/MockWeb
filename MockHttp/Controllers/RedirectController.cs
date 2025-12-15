using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;

namespace MockHttp.Controllers;

/// <summary>
/// Handles various HTTP redirect scenarios for testing redirect behavior
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RedirectController : ControllerBase
{
    /// <summary>
    /// Redirects to a specified URL with 302 Found (temporary redirect)
    /// </summary>
    /// <param name="url">Target URL to redirect to</param>
    /// <returns>302 redirect response</returns>
    /// <response code="302">Temporary redirect</response>
    /// <response code="400">If URL is invalid</response>
    [HttpGet("to")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult RedirectTo([FromQuery] string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new
            {
                error = "URL parameter is required",
                field = "url",
                example = "/api/redirect/to?url=https://example.com",
                timestamp = DateTime.UtcNow
            });
        }

        if (!IsValidUrl(url))
        {
            return BadRequest(new
            {
                error = "Invalid URL format",
                field = "url",
                providedValue = url,
                timestamp = DateTime.UtcNow
            });
        }

        Response.Headers.Append("X-Redirect-Type", "Temporary");
        Response.Headers.Append("X-Original-Url", Request.GetDisplayUrl());
        
        return Redirect(url);
    }

    /// <summary>
    /// Performs a 301 Moved Permanently redirect
    /// </summary>
    /// <param name="url">Target URL to redirect to</param>
    /// <returns>301 redirect response</returns>
    /// <response code="301">Permanent redirect</response>
    /// <response code="400">If URL is invalid</response>
    [HttpGet("permanent")]
    [ProducesResponseType(StatusCodes.Status301MovedPermanently)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetPermanentRedirect([FromQuery] string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new
            {
                error = "URL parameter is required",
                field = "url",
                example = "/api/redirect/permanent?url=https://example.com",
                timestamp = DateTime.UtcNow
            });
        }

        if (!IsValidUrl(url))
        {
            return BadRequest(new
            {
                error = "Invalid URL format",
                field = "url",
                timestamp = DateTime.UtcNow
            });
        }

        Response.Headers.Append("X-Redirect-Type", "Permanent");
        Response.Headers.Append("X-Original-Url", Request.GetDisplayUrl());
        
        return RedirectPermanent(url);
    }

    /// <summary>
    /// Performs a 307 Temporary Redirect (preserves HTTP method)
    /// </summary>
    /// <param name="url">Target URL to redirect to</param>
    /// <returns>307 redirect response</returns>
    /// <response code="307">Temporary redirect preserving method</response>
    /// <response code="400">If URL is invalid</response>
    [HttpGet("temporary")]
    [HttpPost("temporary")]
    [ProducesResponseType(StatusCodes.Status307TemporaryRedirect)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult RedirectTemporary([FromQuery] string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new
            {
                error = "URL parameter is required",
                field = "url",
                timestamp = DateTime.UtcNow
            });
        }

        if (!IsValidUrl(url))
        {
            return BadRequest(new
            {
                error = "Invalid URL format",
                field = "url",
                timestamp = DateTime.UtcNow
            });
        }

        Response.Headers.Append("X-Redirect-Type", "Temporary-PreserveMethod");
        Response.Headers.Append("X-Original-Method", Request.Method);
        
        return RedirectPreserveMethod(url);
    }

    /// <summary>
    /// Performs a 308 Permanent Redirect (preserves HTTP method)
    /// </summary>
    /// <param name="url">Target URL to redirect to</param>
    /// <returns>308 redirect response</returns>
    /// <response code="308">Permanent redirect preserving method</response>
    /// <response code="400">If URL is invalid</response>
    [HttpGet("permanent-preserve-method")]
    [HttpPost("permanent-preserve-method")]
    [ProducesResponseType(StatusCodes.Status308PermanentRedirect)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetPermanentRedirectPreserveMethod([FromQuery] string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new
            {
                error = "URL parameter is required",
                field = "url",
                timestamp = DateTime.UtcNow
            });
        }

        if (!IsValidUrl(url))
        {
            return BadRequest(new
            {
                error = "Invalid URL format",
                field = "url",
                timestamp = DateTime.UtcNow
            });
        }

        Response.Headers.Append("X-Redirect-Type", "Permanent-PreserveMethod");
        Response.Headers.Append("X-Original-Method", Request.Method);
        
        return RedirectPermanentPreserveMethod(url);
    }

    /// <summary>
    /// Creates a redirect chain with specified number of hops
    /// </summary>
    /// <param name="count">Number of redirects in the chain (1-10)</param>
    /// <returns>Redirect to next hop or final destination</returns>
    /// <response code="302">Redirect to next hop</response>
    /// <response code="400">If count is invalid</response>
    [HttpGet("chain/{count:int}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult RedirectChain(int count)
    {
        if (count < 1 || count > 10)
        {
            return BadRequest(new
            {
                error = "Redirect chain count must be between 1 and 10",
                field = "count",
                providedValue = count,
                timestamp = DateTime.UtcNow
            });
        }

        if (count == 1)
        {
            return Ok(new
            {
                success = true,
                message = "Final destination reached",
                redirectChainCompleted = true,
                timestamp = DateTime.UtcNow
            });
        }

        var nextUrl = $"{Request.Scheme}://{Request.Host}/api/redirect/chain/{count - 1}";
        Response.Headers.Append("X-Redirect-Hop", $"{11 - count}");
        Response.Headers.Append("X-Remaining-Hops", $"{count - 1}");
        
        return Redirect(nextUrl);
    }

    /// <summary>
    /// Redirects to a specific status code response
    /// </summary>
    /// <param name="code">HTTP status code to redirect to (100-599)</param>
    /// <returns>Redirect to status code endpoint</returns>
    /// <response code="302">Redirect to status endpoint</response>
    [HttpGet("to-status/{code:int}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult RedirectToStatus(int code)
    {
        var targetUrl = $"{Request.Scheme}://{Request.Host}/api/simulation/status/{code}";
        Response.Headers.Append("X-Redirect-Target-Status", code.ToString());
        
        return Redirect(targetUrl);
    }

    /// <summary>
    /// Redirects to different endpoints based on the method used
    /// </summary>
    /// <returns>Redirect based on HTTP method</returns>
    /// <response code="302">Redirect to method-specific endpoint</response>
    [HttpGet("method-based")]
    [HttpPost("method-based")]
    [HttpPut("method-based")]
    [HttpDelete("method-based")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult RedirectBasedOnMethod()
    {
        var targetUrl = Request.Method.ToUpperInvariant() switch
        {
            "GET" => $"{Request.Scheme}://{Request.Host}/api/echo/headers",
            "POST" => $"{Request.Scheme}://{Request.Host}/api/data",
            "PUT" => $"{Request.Scheme}://{Request.Host}/api/data",
            "DELETE" => $"{Request.Scheme}://{Request.Host}/api/data",
            _ => $"{Request.Scheme}://{Request.Host}/api/echo"
        };

        Response.Headers.Append("X-Original-Method", Request.Method);
        Response.Headers.Append("X-Redirect-Reason", "Method-based routing");
        
        return Redirect(targetUrl);
    }

    /// <summary>
    /// Delays then redirects to specified URL
    /// </summary>
    /// <param name="url">Target URL to redirect to</param>
    /// <param name="delaySeconds">Delay in seconds before redirect (0-10)</param>
    /// <returns>Redirect after delay</returns>
    /// <response code="302">Redirect after delay</response>
    /// <response code="400">If parameters are invalid</response>
    [HttpGet("delayed")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DelayedRedirect([FromQuery] string? url, [FromQuery] int delaySeconds = 2)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new
            {
                error = "URL parameter is required",
                field = "url",
                timestamp = DateTime.UtcNow
            });
        }

        if (delaySeconds < 0 || delaySeconds > 10)
        {
            return BadRequest(new
            {
                error = "Delay must be between 0 and 10 seconds",
                field = "delaySeconds",
                timestamp = DateTime.UtcNow
            });
        }

        if (!IsValidUrl(url))
        {
            return BadRequest(new
            {
                error = "Invalid URL format",
                field = "url",
                timestamp = DateTime.UtcNow
            });
        }

        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        
        Response.Headers.Append("X-Delay-Applied", $"{delaySeconds} seconds");
        
        return Redirect(url);
    }

    /// <summary>
    /// Redirects to a relative path within the API
    /// </summary>
    /// <param name="path">Relative path (e.g., /api/echo)</param>
    /// <returns>Redirect to relative path</returns>
    /// <response code="302">Redirect to relative path</response>
    /// <response code="400">If path is invalid</response>
    [HttpGet("relative")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult RedirectRelative([FromQuery] string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest(new
            {
                error = "Path parameter is required",
                field = "path",
                example = "/api/redirect/relative?path=/api/echo",
                timestamp = DateTime.UtcNow
            });
        }

        if (!path.StartsWith('/'))
        {
            return BadRequest(new
            {
                error = "Path must start with '/'",
                field = "path",
                providedValue = path,
                timestamp = DateTime.UtcNow
            });
        }

        Response.Headers.Append("X-Redirect-Type", "Relative");
        
        return LocalRedirect(path);
    }

    /// <summary>
    /// Simulates a redirect loop (redirects back to itself)
    /// </summary>
    /// <param name="maxHops">Maximum number of hops before showing warning (1-5)</param>
    /// <returns>Redirect to self or warning</returns>
    /// <response code="302">Redirect to self</response>
    /// <response code="508">Loop detected</response>
    [HttpGet("loop")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status508LoopDetected)]
    public IActionResult RedirectLoop([FromQuery] int maxHops = 3)
    {
        var hopCountHeader = Request.Headers["X-Redirect-Loop-Count"].FirstOrDefault();
        var hopCount = int.TryParse(hopCountHeader, out var count) ? count : 0;
        
        if (hopCount >= maxHops)
        {
            return StatusCode(508, new
            {
                error = "Loop Detected",
                message = "Maximum redirect hops reached",
                hops = hopCount,
                maxHops,
                timestamp = DateTime.UtcNow
            });
        }

        Response.Headers.Append("X-Redirect-Loop-Count", (hopCount + 1).ToString());
        Response.Headers.Append("X-Warning", $"This endpoint creates a redirect loop. Hop {hopCount + 1}/{maxHops}");
        
        var targetUrl = $"{Request.Scheme}://{Request.Host}/api/redirect/loop?maxHops={maxHops}";
        return Redirect(targetUrl);
    }

    /// <summary>
    /// Returns information about redirect types without actually redirecting
    /// </summary>
    /// <returns>Redirect information and examples</returns>
    /// <response code="200">Returns redirect information</response>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetRedirectInfo()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}/api/redirect";
        
        return Ok(new
        {
            redirectTypes = new[]
            {
                new
                {
                    statusCode = 301,
                    name = "Moved Permanently",
                    description = "Resource has permanently moved to a new location",
                    endpoint = $"{baseUrl}/permanent?url=https://example.com",
                    cacheability = "Browsers cache this redirect"
                },
                new
                {
                    statusCode = 302,
                    name = "Found (Temporary Redirect)",
                    description = "Resource temporarily available at different location",
                    endpoint = $"{baseUrl}/to?url=https://example.com",
                    cacheability = "Not cached by default"
                },
                new
                {
                    statusCode = 307,
                    name = "Temporary Redirect",
                    description = "Temporary redirect that preserves HTTP method",
                    endpoint = $"{baseUrl}/temporary?url=https://example.com",
                    cacheability = "Not cached, method preserved"
                },
                new
                {
                    statusCode = 308,
                    name = "Permanent Redirect",
                    description = "Permanent redirect that preserves HTTP method",
                    endpoint = $"{baseUrl}/permanent-preserve-method?url=https://example.com",
                    cacheability = "Cached, method preserved"
                }
            },
            examples = new
            {
                basicRedirect = $"{baseUrl}/to?url=https://example.com",
                permanentRedirect = $"{baseUrl}/permanent?url=https://example.com",
                redirectChain = $"{baseUrl}/chain/5",
                delayedRedirect = $"{baseUrl}/delayed?url=https://example.com&delaySeconds=2",
                relativeRedirect = $"{baseUrl}/relative?path=/api/echo",
                methodBased = $"{baseUrl}/method-based",
                loop = $"{baseUrl}/loop?maxHops=3"
            },
            timestamp = DateTime.UtcNow
        });
    }

    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // Allow relative URLs starting with /
        if (url.StartsWith('/'))
            return true;

        // Validate absolute URLs
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
