using Microsoft.AspNetCore.Mvc;
using MockHttp.Dtos;
using System.Text;

namespace MockHttp.Controllers;

/// <summary>
/// Echoes back request details for inspection and debugging
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EchoController : ControllerBase
{
    /// <summary>
    /// Accepts any HTTP method and returns complete request details
    /// </summary>
    /// <returns>Request details including method, headers, query params, and body</returns>
    /// <response code="200">Returns request details</response>
    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    [HttpPatch]
    [HttpOptions]
    [HttpHead]
    [ProducesResponseType(typeof(RequestDetailsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> EchoRequest()
    {
        var req = Request;
        var dto = new RequestDetailsDto
        {
            Method = req.Method,
            Url = $"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}",
            Timestamp = DateTime.UtcNow
        };

        // Capture headers
        foreach (var h in req.Headers.Where(h => !h.Key.StartsWith(":")))
            dto.Headers[h.Key] = string.Join("; ", h.Value.ToArray());

        // Capture query parameters
        foreach (var q in req.Query)
            dto.QueryParams[q.Key] = q.Value.ToString();

        // Capture form data if present
        if (req.HasFormContentType && req.Form != null)
        {
            foreach (var f in req.Form)
                dto.FormData[f.Key] = f.Value.ToString();
        }

        // Capture body content
        if (req.ContentLength > 0)
        {
            try
            {
                req.EnableBuffering();
                using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: true);
                dto.BodyContent = await reader.ReadToEndAsync();
                req.Body.Position = 0;
                dto.BodySize = req.ContentLength ?? 0;
            }
            catch (Exception ex)
            {
                dto.BodyContent = $"Error reading body: {ex.Message}";
            }
        }

        // Add connection info
        dto.ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        dto.Protocol = req.Protocol;
        dto.Scheme = req.Scheme;
        dto.Host = req.Host.ToString();
        dto.Path = req.Path.ToString();
        dto.QueryString = req.QueryString.ToString();

        Response.Headers.Append("X-Echo-Timestamp", DateTime.UtcNow.ToString("o"));
        
        return Ok(dto);
    }

    /// <summary>
    /// Returns request headers only
    /// </summary>
    /// <returns>Dictionary of request headers</returns>
    /// <response code="200">Returns headers</response>
    [HttpGet("headers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHeaders()
    {
        var headers = Request.Headers
            .Where(h => !h.Key.StartsWith(":"))
            .ToDictionary(h => h.Key, h => string.Join("; ", h.Value.ToArray()));

        return Ok(new
        {
            headers,
            count = headers.Count,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns user agent information
    /// </summary>
    /// <returns>User agent string and parsed information</returns>
    /// <response code="200">Returns user agent details</response>
    [HttpGet("user-agent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetUserAgent()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        
        return Ok(new
        {
            userAgent,
            raw = userAgent,
            headers = new
            {
                userAgent = Request.Headers.UserAgent.ToString(),
                accept = Request.Headers.Accept.ToString(),
                acceptEncoding = Request.Headers.AcceptEncoding.ToString(),
                acceptLanguage = Request.Headers.AcceptLanguage.ToString()
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns client IP address information
    /// </summary>
    /// <returns>IP address and connection details</returns>
    /// <response code="200">Returns IP information</response>
    [HttpGet("ip")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetIpAddress()
    {
        var connection = HttpContext.Connection;
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();

        return Ok(new
        {
            ip = connection.RemoteIpAddress?.ToString() ?? "unknown",
            remoteIpAddress = connection.RemoteIpAddress?.ToString(),
            remotePort = connection.RemotePort,
            localIpAddress = connection.LocalIpAddress?.ToString(),
            localPort = connection.LocalPort,
            forwardedFor,
            realIp,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Delays response by specified seconds then echoes request
    /// </summary>
    /// <param name="seconds">Delay in seconds (max 30)</param>
    /// <returns>Request details after delay</returns>
    /// <response code="200">Returns request details after delay</response>
    /// <response code="400">If delay is invalid</response>
    [HttpGet("delay/{seconds:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DelayedEcho(int seconds)
    {
        if (seconds < 0 || seconds > 30)
            return BadRequest(new { error = "Delay must be between 0 and 30 seconds", field = "seconds" });

        var startTime = DateTime.UtcNow;
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        var endTime = DateTime.UtcNow;

        return Ok(new
        {
            delayed = true,
            requestedDelay = seconds,
            actualDelay = (endTime - startTime).TotalSeconds,
            startTime,
            endTime,
            method = Request.Method,
            url = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}"
        });
    }
}
