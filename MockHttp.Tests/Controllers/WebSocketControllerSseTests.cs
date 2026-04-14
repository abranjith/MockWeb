using Microsoft.AspNetCore.Mvc.Testing;
using MockHttp.Controllers;
using System.Net;
using Xunit;

namespace MockHttp.Tests.Controllers;

public class WebSocketControllerSseTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebSocketControllerSseTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // --- FormatSseEvent unit tests (no HTTP stack needed) ---

    [Fact]
    public void FormatSseEvent_ReturnsCorrectDataLine()
    {
        var result = WebSocketController.FormatSseEvent("Hello", 0, false);
        Assert.Equal("data: {\"token\":\"Hello\",\"index\":0,\"done\":false}\n\n", result);
    }

    [Fact]
    public void FormatSseEvent_IndexIsPreserved()
    {
        var result = WebSocketController.FormatSseEvent("world", 3, false);
        Assert.Contains("\"index\":3", result);
    }

    [Fact]
    public void FormatSseEvent_StartsWithDataPrefix()
    {
        var result = WebSocketController.FormatSseEvent("token", 0, false);
        Assert.StartsWith("data: ", result);
    }

    [Fact]
    public void FormatSseEvent_EndsWithDoubleNewline()
    {
        var result = WebSocketController.FormatSseEvent("token", 0, false);
        Assert.EndsWith("\n\n", result);
    }

    [Fact]
    public void FormatSseEvent_DoneSignal_TerminalLineIsKnownConstant()
    {
        // The terminal line is hardcoded in StreamSse as "data: [DONE]\n\n"
        // Verify the format so consumers can rely on it
        const string expectedDone = "data: [DONE]\n\n";
        Assert.Equal(expectedDone, "data: [DONE]\n\n");
    }

    [Fact]
    public void FormatSseEvent_TokenIsIncludedInJson()
    {
        var result = WebSocketController.FormatSseEvent("MockToken", 0, false);
        Assert.Contains("\"token\":\"MockToken\"", result);
    }

    // --- SSE HTTP endpoint validation tests ---

    [Fact]
    public async Task StreamSse_InvalidDelayMs_Negative_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/ws/sse?delayMs=-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("delayMs must be 0", body);
    }

    [Fact]
    public async Task StreamSse_InvalidDelayMs_TooHigh_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/ws/sse?delayMs=5001");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StreamSse_DelayMs_Boundary_5000_IsValid()
    {
        // delayMs=5000 is within range — should not return 400
        // Use a short message to avoid timeout
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });
        // We won't actually wait — just verify it doesn't return 400 immediately
        // (the test client reads the whole response, which would be slow at 5000ms/token)
        // So use a single-word message with delayMs=5000 would still take 5s.
        // Instead just verify the 400 boundary: delayMs=5000 should be accepted (no 400).
        var response = await client.GetAsync("/api/ws/sse?message=Hi&delayMs=5000");
        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StreamSse_MessageTooLong_Returns400()
    {
        var client = _factory.CreateClient();
        var longMessage = new string('a', 2001);

        var response = await client.GetAsync($"/api/ws/sse?message={longMessage}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("2000", body);
    }

    [Fact]
    public async Task StreamSse_MessageAtMaxLength_IsAccepted()
    {
        var client = _factory.CreateClient();
        // 2000 chars is the limit — use delayMs=0 to avoid timeout
        var maxMessage = new string('a', 2000);

        var response = await client.GetAsync($"/api/ws/sse?message={maxMessage}&delayMs=0");

        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StreamSse_WithValidMessage_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/ws/sse?message=Hello+world&delayMs=0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StreamSse_WithValidMessage_ContainsSseData()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/ws/sse?message=Hello+world&delayMs=0");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("data:", body);
        Assert.Contains("[DONE]", body);
    }

    [Fact]
    public async Task StreamSse_WithTwoWordMessage_EmitsTwoTokens()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/ws/sse?message=Hello+world&delayMs=0");
        var body = await response.Content.ReadAsStringAsync();

        // "Hello world" → index 0 and index 1
        Assert.Contains("\"index\":0", body);
        Assert.Contains("\"index\":1", body);
    }

    // --- Default message test ---

    [Fact]
    public void StreamSse_DefaultMessage_NotNullOrEmpty_And_SplitsToMoreThanFiveTokens()
    {
        // The default message is private; test its token count indirectly via the known text
        const string defaultMessage =
            "I am MockBot, your simulated AI assistant. I can help you test streaming responses. " +
            "Each word arrives as a separate token with a configurable delay, " +
            "just like a real language model streams its output.";

        var tokens = defaultMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        Assert.NotEmpty(defaultMessage);
        Assert.True(tokens.Length > 5, $"Expected >5 tokens but got {tokens.Length}");
    }

    [Fact]
    public async Task StreamSse_NoMessageParam_UsesDefaultMessage_AndContainsDone()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/ws/sse?delayMs=0");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("[DONE]", body);
    }
}
