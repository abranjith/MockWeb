using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace MockHttp.Tests.Controllers;

public class WebSocketControllerEchoTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebSocketControllerEchoTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Echo_NonWebSocketRequest_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/ws/echo");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("WebSocket upgrade required", body);
    }

    [Fact]
    public async Task Echo_NonWebSocketRequest_ResponseBodyIsJson()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/ws/echo");
        var body = await response.Content.ReadAsStringAsync();

        // Body must be valid JSON containing both required fields
        Assert.Contains("\"error\"", body);
        Assert.Contains("\"timestamp\"", body);
    }

    [Fact]
    public async Task Chat_NonWebSocketRequest_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/ws/chat");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("WebSocket upgrade required", body);
    }
}
