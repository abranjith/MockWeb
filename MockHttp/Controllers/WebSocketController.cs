using Microsoft.AspNetCore.Mvc;
using MockHttp.Dtos;
using MockHttp.Services;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MockHttp.Controllers;

/// <summary>
/// Provides real-time communication endpoints: WebSocket echo, WebSocket chat,
/// and SSE token streaming.
/// </summary>
/// <remarks>
/// No <c>[ApiController]</c> attribute is applied — this avoids ASP.NET's automatic
/// 400/model-binding pipeline interfering with the raw WebSocket upgrade.
/// </remarks>
[Route("api/ws")]
public class WebSocketController(IWebSocketChatService chatService) : ControllerBase
{
    private const int BufferSize = 4096;

    private static readonly string _defaultSseMessage =
        "I am MockBot, your simulated AI assistant. I can help you test streaming responses. " +
        "Each word arrives as a separate token with a configurable delay, " +
        "just like a real language model streams its output.";

    /// <summary>
    /// Bidirectional WebSocket echo endpoint. Echoes every received message frame back verbatim.
    /// </summary>
    /// <remarks>
    /// Swagger displays this as a plain GET, but a WebSocket client must be used.
    /// Example: <c>ws://localhost:5218/api/ws/echo</c>
    /// Closes gracefully when a Close frame is received.
    /// Returns 400 if the request is not a WebSocket upgrade.
    /// </remarks>
    [HttpGet("echo")]
    public async Task Echo()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            Response.StatusCode = 400;
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "WebSocket upgrade required",
                timestamp = DateTime.UtcNow
            }));
            return;
        }

        using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[BufferSize];

        while (ws.State == WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                break;
            }
            await ws.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count),
                               result.MessageType, result.EndOfMessage, CancellationToken.None);
        }
    }

    /// <summary>
    /// Bidirectional WebSocket chat endpoint that returns canned mock replies from MockBot.
    /// </summary>
    /// <remarks>
    /// Swagger displays this as a plain GET, but a WebSocket client must be used.
    /// Example: <c>ws://localhost:5218/api/ws/chat</c>
    /// Send JSON: <c>{"message":"hello"}</c>.
    /// Malformed JSON returns an error frame while keeping the connection open.
    /// Returns 400 if the request is not a WebSocket upgrade.
    /// </remarks>
    [HttpGet("chat")]
    public async Task Chat()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            Response.StatusCode = 400;
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "WebSocket upgrade required",
                timestamp = DateTime.UtcNow
            }));
            return;
        }

        using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[BufferSize];

        while (ws.State == WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                break;
            }

            var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
            try
            {
                var dto = JsonSerializer.Deserialize<ChatMessageDto>(text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var reply = chatService.GetResponse(dto!.Message);
                var payload = JsonSerializer.SerializeToUtf8Bytes(new
                {
                    response = reply,
                    messageId = Guid.NewGuid(),
                    timestamp = DateTime.UtcNow
                });
                await ws.SendAsync(new ArraySegment<byte>(payload),
                                   WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (JsonException)
            {
                var err = JsonSerializer.SerializeToUtf8Bytes(new
                {
                    error = "Invalid JSON. Expected: {\"message\":\"<text>\"}",
                    timestamp = DateTime.UtcNow
                });
                await ws.SendAsync(new ArraySegment<byte>(err),
                                   WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    /// <summary>
    /// SSE token-streaming endpoint that simulates LLM-style word-by-word output.
    /// </summary>
    /// <param name="message">
    /// The text to stream as tokens. Defaults to a built-in mock message when omitted or blank.
    /// Maximum 2000 characters.
    /// </param>
    /// <param name="delayMs">
    /// Delay in milliseconds between tokens (0–5000). Default: 50.
    /// </param>
    /// <remarks>
    /// This is a regular HTTP GET endpoint — no WebSocket upgrade needed.
    /// Consume with the browser's <c>EventSource</c> API or <c>fetch</c> with streaming.
    /// Each token arrives as: <c>data: {"token":"word","index":0,"done":false}\n\n</c>.
    /// The stream terminates with: <c>data: [DONE]\n\n</c>.
    /// Client disconnects are handled silently via <see cref="OperationCanceledException"/>.
    /// </remarks>
    [HttpGet("sse")]
    [Produces("text/event-stream")]
    public async Task StreamSse(
        [FromQuery] string? message = null,
        [FromQuery] int delayMs = 50)
    {
        if (delayMs < 0 || delayMs > 5000)
        {
            Response.StatusCode = 400;
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "delayMs must be 0–5000",
                timestamp = DateTime.UtcNow
            }));
            return;
        }

        if (message?.Length > 2000)
        {
            Response.StatusCode = 400;
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "message must be ≤2000 characters",
                timestamp = DateTime.UtcNow
            }));
            return;
        }

        var effectiveMessage = string.IsNullOrWhiteSpace(message) ? _defaultSseMessage : message;

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");
        Response.Headers.Append("Connection", "keep-alive");

        var ct = HttpContext.RequestAborted;
        var tokens = effectiveMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        try
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                await Response.WriteAsync(FormatSseEvent(tokens[i], i, false), ct);
                await Response.Body.FlushAsync(ct);
                await Task.Delay(delayMs, ct);
            }
            await Response.WriteAsync("data: [DONE]\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // client disconnected — exit silently
        }
    }

    /// <summary>
    /// Formats a single SSE data line for a streaming token.
    /// </summary>
    /// <param name="token">The token text to include in the event.</param>
    /// <param name="index">Zero-based token index within the stream.</param>
    /// <param name="done">Whether this is the terminal signal (unused in current output; <c>[DONE]</c> is written directly).</param>
    /// <returns>A formatted SSE <c>data:</c> line ending with <c>\n\n</c>.</returns>
    internal static string FormatSseEvent(string token, int index, bool done)
    {
        var json = JsonSerializer.Serialize(new { token, index, done });
        return $"data: {json}\n\n";
    }
}
