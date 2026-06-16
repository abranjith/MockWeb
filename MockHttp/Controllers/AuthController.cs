using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace MockHttp.Controllers;

/// <summary>
/// Simulates various authentication and authorization scenarios
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// Tests HTTP Basic Authentication
    /// </summary>
    /// <param name="user">Expected username</param>
    /// <param name="pass">Expected password</param>
    /// <returns>200 if credentials match, 401 otherwise</returns>
    /// <response code="200">Authenticated successfully</response>
    /// <response code="401">Authentication failed</response>
    [HttpGet("basic/{user}/{pass}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult BasicAuth(string user, string pass)
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            Response.Headers.Append("WWW-Authenticate", "Basic realm=\"MockHttp API\"");
            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "Authorization header is missing",
                required = "Basic authentication",
                timestamp = DateTime.UtcNow
            });
        }

        var headerValue = authHeader.ToString();
        if (!headerValue.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Response.Headers.Append("WWW-Authenticate", "Basic realm=\"MockHttp API\"");
            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "Invalid authorization scheme. Expected 'Basic'",
                timestamp = DateTime.UtcNow
            });
        }

        try
        {
            var encodedCredentials = headerValue.Substring("Basic ".Length).Trim();
            var decodedBytes = Convert.FromBase64String(encodedCredentials);
            var decodedString = Encoding.UTF8.GetString(decodedBytes);
            var parts = decodedString.Split(':', 2);

            if (parts.Length != 2)
            {
                Response.Headers.Append("WWW-Authenticate", "Basic realm=\"MockHttp API\"");
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "Invalid credentials format",
                    timestamp = DateTime.UtcNow
                });
            }

            var providedUser = parts[0];
            var providedPass = parts[1];

            if (providedUser == user && providedPass == pass)
            {
                return Ok(new
                {
                    authenticated = true,
                    user = providedUser,
                    method = "Basic",
                    timestamp = DateTime.UtcNow,
                    message = "Authentication successful"
                });
            }

            Response.Headers.Append("WWW-Authenticate", "Basic realm=\"MockHttp API\"");
            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "Invalid username or password",
                expected = new { user, pass },
                provided = new { user = providedUser },
                timestamp = DateTime.UtcNow
            });
        }
        catch (FormatException)
        {
            Response.Headers.Append("WWW-Authenticate", "Basic realm=\"MockHttp API\"");
            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "Invalid Base64 encoding in credentials",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Tests Bearer Token Authentication
    /// </summary>
    /// <returns>200 if bearer token is present, 401 otherwise</returns>
    /// <response code="200">Token validated</response>
    /// <response code="401">Token missing or invalid</response>
    [HttpGet("bearer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult BearerAuth()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            Response.Headers.Append("WWW-Authenticate", "Bearer realm=\"MockHttp API\"");
            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "Authorization header is missing",
                required = "Bearer token",
                timestamp = DateTime.UtcNow
            });
        }

        var headerValue = authHeader.ToString();
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            Response.Headers.Append("WWW-Authenticate", "Bearer realm=\"MockHttp API\"");
            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "Invalid authorization scheme. Expected 'Bearer'",
                timestamp = DateTime.UtcNow
            });
        }

        var token = headerValue.Substring("Bearer ".Length).Trim();
        
        if (string.IsNullOrWhiteSpace(token))
        {
            Response.Headers.Append("WWW-Authenticate", "Bearer realm=\"MockHttp API\"");
            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "Bearer token is empty",
                timestamp = DateTime.UtcNow
            });
        }

        return Ok(new
        {
            authenticated = true,
            method = "Bearer",
            token = token.Length > 20 ? $"{token.Substring(0, 20)}..." : token,
            tokenLength = token.Length,
            timestamp = DateTime.UtcNow,
            message = "Token validated successfully"
        });
    }

    /// <summary>
    /// Tests Bearer Token with specific expected value
    /// </summary>
    /// <param name="token">Expected token value</param>
    /// <returns>200 if token matches, 401 otherwise</returns>
    /// <response code="200">Token matches</response>
    /// <response code="401">Token missing or doesn't match</response>
    [HttpGet("bearer/{token}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult BearerAuthWithToken(string token)
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            Response.Headers.Append("WWW-Authenticate", $"Bearer realm=\"MockHttp API\"");
            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "Authorization header is missing",
                timestamp = DateTime.UtcNow
            });
        }

        var headerValue = authHeader.ToString();
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            Response.Headers.Append("WWW-Authenticate", $"Bearer realm=\"MockHttp API\"");
            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "Invalid authorization scheme",
                timestamp = DateTime.UtcNow
            });
        }

        var providedToken = headerValue.Substring("Bearer ".Length).Trim();

        if (providedToken == token)
        {
            return Ok(new
            {
                authenticated = true,
                method = "Bearer",
                tokenMatched = true,
                timestamp = DateTime.UtcNow
            });
        }

        return Unauthorized(new
        {
            error = "Unauthorized",
            message = "Token does not match expected value",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Tests Digest Authentication (simplified check)
    /// </summary>
    /// <returns>200 if digest header is present, 401 otherwise</returns>
    /// <response code="200">Digest header present</response>
    /// <response code="401">Digest header missing</response>
    [HttpGet("digest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult DigestAuth()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            Response.Headers.Append("WWW-Authenticate", 
                $"Digest realm=\"MockHttp API\", qop=\"auth\", nonce=\"{nonce}\", opaque=\"{Convert.ToBase64String(Guid.NewGuid().ToByteArray())}\"");
            
            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "Authorization header is missing",
                required = "Digest authentication",
                timestamp = DateTime.UtcNow
            });
        }

        var headerValue = authHeader.ToString();
        if (!headerValue.StartsWith("Digest ", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "Invalid authorization scheme. Expected 'Digest'",
                timestamp = DateTime.UtcNow
            });
        }

        return Ok(new
        {
            authenticated = true,
            method = "Digest",
            message = "Digest authentication header detected (simplified validation)",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Tests API Key authentication via header
    /// </summary>
    /// <param name="expectedKey">Expected API key value</param>
    /// <returns>200 if API key matches, 401 otherwise</returns>
    /// <response code="200">API key valid</response>
    /// <response code="401">API key missing or invalid</response>
    [HttpGet("api-key/{expectedKey}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult ApiKeyAuth(string expectedKey)
    {
        if (!Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "X-API-Key header is missing",
                timestamp = DateTime.UtcNow
            });
        }

        var providedKey = apiKeyHeader.ToString();
        
        if (providedKey == expectedKey)
        {
            return Ok(new
            {
                authenticated = true,
                method = "API Key",
                keyMatched = true,
                timestamp = DateTime.UtcNow
            });
        }

        return Unauthorized(new
        {
            error = "Unauthorized",
            message = "Invalid API key",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Simulates a forbidden resource (403)
    /// </summary>
    /// <returns>403 Forbidden</returns>
    /// <response code="403">Access forbidden</response>
    [HttpGet("forbidden")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Forbidden()
    {
        return StatusCode(403, new
        {
            error = "Forbidden",
            message = "You don't have permission to access this resource",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns current authentication state from headers
    /// </summary>
    /// <returns>Authentication information detected</returns>
    /// <response code="200">Returns auth info</response>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAuthInfo()
    {
        var hasAuth = Request.Headers.TryGetValue("Authorization", out var authHeader);
        var hasApiKey = Request.Headers.TryGetValue("X-API-Key", out var apiKey);

        string? authType = null;
        string? authValue = null;

        if (hasAuth)
        {
            var headerValue = authHeader.ToString();
            if (headerValue.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                authType = "Basic";
                authValue = headerValue.Substring("Basic ".Length);
            }
            else if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                authType = "Bearer";
                authValue = headerValue.Substring("Bearer ".Length);
            }
            else if (headerValue.StartsWith("Digest ", StringComparison.OrdinalIgnoreCase))
            {
                authType = "Digest";
                authValue = "Present";
            }
        }

        return Ok(new
        {
            authenticated = hasAuth || hasApiKey,
            methods = new
            {
                authorization = hasAuth ? new { type = authType, valueLength = authValue?.Length ?? 0 } : null,
                apiKey = hasApiKey ? new { present = true, valueLength = apiKey.ToString().Length } : null
            },
            headers = new
            {
                authorization = hasAuth ? "Present" : "Missing",
                apiKey = hasApiKey ? "Present" : "Missing"
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Simulates OAuth 2.0 Authorization Endpoint
    /// </summary>
    /// <param name="responseType">Must be 'code'</param>
    /// <param name="clientId">Client Identifier</param>
    /// <param name="redirectUri">URL to redirect back to</param>
    /// <param name="state">Opaque value used to maintain state between the request and the callback</param>
    /// <returns>Redirects to redirect_uri with code</returns>
    [HttpGet("authorize")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult OAuthAuthorize(
        [FromQuery(Name = "response_type")] string responseType,
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromQuery(Name = "state")] string? state = null)
    {
        if (string.IsNullOrEmpty(responseType) || !responseType.Equals("code", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "unsupported_response_type", message = "Only 'code' response_type is supported." });
        }

        if (string.IsNullOrEmpty(clientId))
        {
            return BadRequest(new { error = "invalid_request", message = "client_id is required." });
        }

        if (string.IsNullOrEmpty(redirectUri))
        {
            return BadRequest(new { error = "invalid_request", message = "redirect_uri is required." });
        }

        // Generate a mock authorization code
        var code = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        
        var separator = redirectUri.Contains('?') ? "&" : "?";
        var callbackUrl = $"{redirectUri}{separator}code={code}";
        
        if (!string.IsNullOrEmpty(state))
        {
            callbackUrl += $"&state={System.Net.WebUtility.UrlEncode(state)}";
        }

        return Redirect(callbackUrl);
    }

    /// <summary>
    /// Simulates OAuth 2.0 Callback Endpoint
    /// </summary>
    /// <param name="code">Authorization code from authorization endpoint</param>
    /// <param name="state">State value provided by client</param>
    /// <returns>Success response for redirect URI handling</returns>
    [HttpGet("callback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult OAuthCallback(
        [FromQuery] string? code = null,
        [FromQuery] string? state = null)
    {
        return Ok(new
        {
            success = true,
            message = "OAuth callback received successfully",
            received = new
            {
                hasCode = !string.IsNullOrWhiteSpace(code),
                hasState = !string.IsNullOrWhiteSpace(state)
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Simulates OAuth 2.0 Token Endpoint
    /// </summary>
    /// <param name="grantType">Grant type (authorization_code or refresh_token)</param>
    /// <param name="code">Authorization code (required for authorization_code grant)</param>
    /// <param name="redirectUri">Redirect URI (required for authorization_code grant)</param>
    /// <param name="clientId">Client ID</param>
    /// <param name="refreshToken">Refresh token (required for refresh_token grant)</param>
    /// <returns>Access token response</returns>
    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult OAuthToken(
        [FromForm(Name = "grant_type")] string grantType,
        [FromForm(Name = "code")] string? code = null,
        [FromForm(Name = "redirect_uri")] string? redirectUri = null,
        [FromForm(Name = "client_id")] string? clientId = null,
        [FromForm(Name = "refresh_token")] string? refreshToken = null)
    {
        if (string.IsNullOrEmpty(grantType))
        {
            return BadRequest(new { error = "invalid_request", message = "grant_type is required." });
        }

        if (grantType.Equals("authorization_code", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { error = "invalid_request", message = "code is required for authorization_code grant." });
            }
            
            // In a real app, we would validate the code, client_id, and redirect_uri here.
            
            return Ok(CreateTokenResponse());
        }
        else if (grantType.Equals("refresh_token", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { error = "invalid_request", message = "refresh_token is required for refresh_token grant." });
            }

            return Ok(CreateTokenResponse());
        }

        return BadRequest(new { error = "unsupported_grant_type", message = $"Grant type '{grantType}' is not supported." });
    }

    private object CreateTokenResponse()
    {
        return new
        {
            access_token = $"mock_access_{Guid.NewGuid():N}",
            token_type = "Bearer",
            expires_in = 3600,
            refresh_token = $"mock_refresh_{Guid.NewGuid():N}",
            scope = "api.read api.write"
        };
    }
}
