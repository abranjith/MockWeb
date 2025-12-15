using Microsoft.AspNetCore.Mvc;
using MockHttp.Services;

namespace MockHttp.Controllers;

/// <summary>
/// Handles various content format responses for testing content negotiation and serialization
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json", "application/xml", "text/html", "text/plain")]
public class FormatsController(IResponseGeneratorService responseGenerator) : ControllerBase
{
    /// <summary>
    /// Returns a simple, flat JSON object
    /// </summary>
    /// <returns>A basic JSON response with status and metadata</returns>
    /// <response code="200">Returns the simple JSON object</response>
    [HttpGet("json/simple")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSimpleJson() => Ok(responseGenerator.GetSimpleJson());

    /// <summary>
    /// Returns a complex, deeply nested JSON object with arrays and multiple levels
    /// </summary>
    /// <returns>A complex JSON structure suitable for testing parsers</returns>
    /// <response code="200">Returns the complex nested JSON object</response>
    [HttpGet("json/complex")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetComplexJson() => Ok(responseGenerator.GetComplexJson());

    /// <summary>
    /// Returns a fully formatted HTML document
    /// </summary>
    /// <returns>HTML content with proper structure and styling</returns>
    /// <response code="200">Returns HTML document</response>
    [HttpGet("html")]
    [Produces("text/html")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult GetHtml() => Content(responseGenerator.GetHtmlString(), "text/html");

    /// <summary>
    /// Returns an XML-formatted response
    /// </summary>
    /// <returns>XML object with nested structure</returns>
    /// <response code="200">Returns XML document</response>
    [HttpGet("xml")]
    [Produces("application/xml")]
    [ProducesResponseType(typeof(XmlSample), StatusCodes.Status200OK)]
    public IActionResult GetXml() => Ok(responseGenerator.GetXmlObject());

    /// <summary>
    /// Returns plain text content
    /// </summary>
    /// <returns>Plain text response</returns>
    /// <response code="200">Returns plain text</response>
    [HttpGet("txt")]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult GetText() => Content(responseGenerator.GetPlainText(), "text/plain");

    /// <summary>
    /// Returns a realistic user profile with nested data
    /// </summary>
    /// <param name="userId">The user ID to generate profile for</param>
    /// <returns>User profile with preferences and statistics</returns>
    /// <response code="200">Returns user profile</response>
    /// <response code="400">If userId is invalid</response>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetUserProfile([FromRoute] int userId)
    {
        if (userId <= 0)
            return BadRequest(new { error = "userId must be greater than 0", field = "userId" });

        return Ok(responseGenerator.GetUserProfile(userId));
    }

    /// <summary>
    /// Returns a paginated product catalog
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    /// <returns>Paginated list of products with metadata</returns>
    /// <response code="200">Returns product catalog</response>
    /// <response code="400">If pagination parameters are invalid</response>
    [HttpGet("products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page <= 0)
            return BadRequest(new { error = "page must be greater than 0", field = "page" });

        if (pageSize <= 0 || pageSize > 100)
            return BadRequest(new { error = "pageSize must be between 1 and 100", field = "pageSize" });

        return Ok(responseGenerator.GetProductCatalog(page, pageSize));
    }

    /// <summary>
    /// Returns detailed order information
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <returns>Complete order details including items, shipping, and payment</returns>
    /// <response code="200">Returns order details</response>
    /// <response code="400">If orderId is invalid</response>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetOrder([FromRoute] Guid orderId)
    {
        if (orderId == Guid.Empty)
            return BadRequest(new { error = "orderId cannot be empty", field = "orderId" });

        return Ok(responseGenerator.GetOrderDetails(orderId));
    }
}
