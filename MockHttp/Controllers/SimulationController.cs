using Microsoft.AspNetCore.Mvc;

namespace MockHttp.Controllers;

/// <summary>
/// Simulates various HTTP scenarios including status codes, delays, and error conditions
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SimulationController : ControllerBase
{
    /// <summary>
    /// Returns a specific HTTP status code
    /// </summary>
    /// <param name="code">HTTP status code to return (100-599)</param>
    /// <returns>Response with the specified status code</returns>
    /// <response code="100-599">Returns specified status code</response>
    [HttpGet("status/{code:int}")]
    [HttpPost("status/{code:int}")]
    [HttpPut("status/{code:int}")]
    [HttpDelete("status/{code:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ReturnStatus(int code)
    {
        if (code < 100 || code > 599)
            return BadRequest(new
            {
                error = "Status code must be between 100 and 599",
                field = "code",
                providedValue = code,
                timestamp = DateTime.UtcNow
            });

        var message = GetStatusMessage(code);
        var category = GetStatusCategory(code);

        Response.Headers.Append("X-Status-Code", code.ToString());
        Response.Headers.Append("X-Status-Category", category);

        return StatusCode(code, new
        {
            statusCode = code,
            message,
            category,
            timestamp = DateTime.UtcNow,
            documentation = $"https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/{code}"
        });
    }

    /// <summary>
    /// Delays response by specified duration
    /// </summary>
    /// <param name="seconds">Delay duration in seconds (0-30)</param>
    /// <returns>Response after delay</returns>
    /// <response code="200">Returns after delay</response>
    /// <response code="400">If delay parameter is invalid</response>
    [HttpGet("delay/{seconds:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delay(int seconds)
    {
        if (seconds < 0 || seconds > 30)
            return BadRequest(new
            {
                error = "Delay must be between 0 and 30 seconds",
                field = "seconds",
                providedValue = seconds,
                maxAllowed = 30,
                timestamp = DateTime.UtcNow
            });

        var startTime = DateTime.UtcNow;
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        var endTime = DateTime.UtcNow;
        var actualDelay = (endTime - startTime).TotalSeconds;

        Response.Headers.Append("X-Delay-Seconds", actualDelay.ToString("F3"));

        return Ok(new
        {
            delayed = true,
            requestedDelay = seconds,
            actualDelay = Math.Round(actualDelay, 3),
            unit = "seconds",
            startTime,
            endTime,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Delays response by specified milliseconds
    /// </summary>
    /// <param name="milliseconds">Delay duration in milliseconds (0-30000)</param>
    /// <returns>Response after delay</returns>
    /// <response code="200">Returns after delay</response>
    /// <response code="400">If delay parameter is invalid</response>
    [HttpGet("delay-ms/{milliseconds:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DelayMilliseconds(int milliseconds)
    {
        if (milliseconds < 0 || milliseconds > 30000)
            return BadRequest(new
            {
                error = "Delay must be between 0 and 30000 milliseconds",
                field = "milliseconds",
                providedValue = milliseconds,
                maxAllowed = 30000,
                timestamp = DateTime.UtcNow
            });

        var startTime = DateTime.UtcNow;
        await Task.Delay(milliseconds);
        var endTime = DateTime.UtcNow;
        var actualDelay = (endTime - startTime).TotalMilliseconds;

        Response.Headers.Append("X-Delay-Milliseconds", actualDelay.ToString("F0"));

        return Ok(new
        {
            delayed = true,
            requestedDelay = milliseconds,
            actualDelay = Math.Round(actualDelay, 0),
            unit = "milliseconds",
            startTime,
            endTime,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Simulates random response time
    /// </summary>
    /// <param name="minMs">Minimum delay in milliseconds</param>
    /// <param name="maxMs">Maximum delay in milliseconds</param>
    /// <returns>Response after random delay</returns>
    /// <response code="200">Returns after random delay</response>
    /// <response code="400">If parameters are invalid</response>
    [HttpGet("random-delay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RandomDelay([FromQuery] int minMs = 100, [FromQuery] int maxMs = 1000)
    {
        if (minMs < 0 || minMs > 30000)
            return BadRequest(new { error = "minMs must be between 0 and 30000", field = "minMs" });

        if (maxMs < minMs || maxMs > 30000)
            return BadRequest(new { error = "maxMs must be between minMs and 30000", field = "maxMs" });

        var random = new Random();
        var delayMs = random.Next(minMs, maxMs);
        
        var startTime = DateTime.UtcNow;
        await Task.Delay(delayMs);
        var endTime = DateTime.UtcNow;

        return Ok(new
        {
            delayed = true,
            randomDelay = delayMs,
            range = new { min = minMs, max = maxMs },
            actualDelay = Math.Round((endTime - startTime).TotalMilliseconds, 0),
            unit = "milliseconds",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Simulates a request that times out (delays then returns 408)
    /// </summary>
    /// <param name="seconds">Seconds before timeout (1-30)</param>
    /// <returns>408 Request Timeout after delay</returns>
    /// <response code="408">Request timeout</response>
    /// <response code="400">If parameter is invalid</response>
    [HttpGet("timeout/{seconds:int}")]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SimulateTimeout(int seconds)
    {
        if (seconds < 1 || seconds > 30)
            return BadRequest(new { error = "Timeout must be between 1 and 30 seconds", field = "seconds" });

        await Task.Delay(TimeSpan.FromSeconds(seconds));

        return StatusCode(408, new
        {
            error = "Request Timeout",
            message = "The server timed out waiting for the request",
            timeoutAfter = seconds,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Simulates a rate limit exceeded scenario
    /// </summary>
    /// <returns>429 Too Many Requests</returns>
    /// <response code="429">Too many requests</response>
    [HttpGet("rate-limit")]
    [HttpPost("rate-limit")]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult RateLimit()
    {
        Response.Headers.Append("X-RateLimit-Limit", "100");
        Response.Headers.Append("X-RateLimit-Remaining", "0");
        Response.Headers.Append("X-RateLimit-Reset", DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds().ToString());
        Response.Headers.Append("Retry-After", "900");

        return StatusCode(429, new
        {
            error = "Too Many Requests",
            message = "API rate limit exceeded",
            rateLimit = new
            {
                limit = 100,
                remaining = 0,
                reset = DateTimeOffset.UtcNow.AddMinutes(15),
                retryAfter = 900
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Simulates an internal server error with stack trace
    /// </summary>
    /// <returns>500 Internal Server Error</returns>
    /// <response code="500">Internal server error</response>
    [HttpGet("error")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult SimulateError()
    {
        var mockException = new
        {
            type = "System.InvalidOperationException",
            message = "A simulated internal server error occurred",
            stackTrace = "   at MockHttp.Controllers.SimulationController.SimulateError() in SimulationController.cs:line 42\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.Execute()",
            timestamp = DateTime.UtcNow
        };

        return StatusCode(500, new
        {
            error = "Internal Server Error",
            message = "An unexpected error occurred while processing the request",
            details = mockException,
            requestId = Guid.NewGuid(),
            timestamp = DateTime.UtcNow
        });
    }

    private static string GetStatusMessage(int code) => code switch
    {
        200 => "OK",
        201 => "Created",
        204 => "No Content",
        301 => "Moved Permanently",
        302 => "Found",
        304 => "Not Modified",
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        405 => "Method Not Allowed",
        408 => "Request Timeout",
        409 => "Conflict",
        410 => "Gone",
        418 => "I'm a teapot",
        429 => "Too Many Requests",
        500 => "Internal Server Error",
        501 => "Not Implemented",
        502 => "Bad Gateway",
        503 => "Service Unavailable",
        504 => "Gateway Timeout",
        _ => $"Status {code}"
    };

    private static string GetStatusCategory(int code) => code switch
    {
        >= 100 and < 200 => "Informational",
        >= 200 and < 300 => "Success",
        >= 300 and < 400 => "Redirection",
        >= 400 and < 500 => "Client Error",
        >= 500 and < 600 => "Server Error",
        _ => "Unknown"
    };
}
