using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.WebSockets;
using System.Text;
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

    [Fact]
    public async Task Echo_WebSocketUpgrade_EchoesPayloadAndClosesCleanly()
    {
        var wsClient = _factory.Server.CreateWebSocketClient();
        var uri = new Uri(_factory.Server.BaseAddress, "/api/ws/echo");

        using var ws = await wsClient.ConnectAsync(uri, CancellationToken.None);
        Assert.Equal(WebSocketState.Open, ws.State);

        var payload = Encoding.UTF8.GetBytes("hello, world");
        await ws.SendAsync(new ArraySegment<byte>(payload),
            WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);

        var buffer = new byte[1024];
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        Assert.Equal(WebSocketMessageType.Text, result.MessageType);
        Assert.True(result.EndOfMessage);
        Assert.Equal("hello, world", Encoding.UTF8.GetString(buffer, 0, result.Count));

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        Assert.Equal(WebSocketState.Closed, ws.State);
    }

    [Fact]
    public async Task Echo_WebSocketUpgrade_NotInterceptedByHttpsRedirection()
    {
        // Regression: with a configured HTTPS port, UseHttpsRedirection would previously
        // issue a 307 for the WebSocket upgrade GET, surfacing as an abnormal close on
        // the client. Connecting successfully here proves the upgrade bypasses redirection.
        using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("https_port", "443");
        });

        var wsClient = factory.Server.CreateWebSocketClient();
        var uri = new Uri(factory.Server.BaseAddress, "/api/ws/echo");

        using var ws = await wsClient.ConnectAsync(uri, CancellationToken.None);

        Assert.Equal(WebSocketState.Open, ws.State);

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    [Fact]
    public async Task Echo_ServerHandlesClientCloseFrameGracefully()
    {
        var wsClient = _factory.Server.CreateWebSocketClient();
        var uri = new Uri(_factory.Server.BaseAddress, "/api/ws/echo");

        using var ws = await wsClient.ConnectAsync(uri, CancellationToken.None);

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);

        Assert.Equal(WebSocketState.Closed, ws.State);
        Assert.Equal(WebSocketCloseStatus.NormalClosure, ws.CloseStatus);
    }
}
