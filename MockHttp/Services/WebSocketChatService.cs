namespace MockHttp.Services;

/// <summary>
/// Singleton implementation that matches incoming messages against a keyword dictionary
/// and returns canned mock responses.
/// </summary>
public class WebSocketChatService : IWebSocketChatService
{
    /// <summary>
    /// Returns a mock response for <paramref name="message"/> based on keyword matching.
    /// </summary>
    /// <param name="message">The raw message text received from the client.</param>
    /// <returns>A canned response string, or a fallback quoting the original message.</returns>
    public string GetResponse(string message)
    {
        var originalMessage = message;
        var normalized = message.ToLowerInvariant().Trim();

        if (normalized.Contains("hello") || normalized.Contains("hi") || normalized.Contains("hey"))
            return "Hello! I'm MockBot, your friendly mock AI assistant. How can I help you today?";

        if (normalized.Contains("help"))
            return "I can echo messages, simulate chat sessions, or stream token-by-token responses. Try them all!";

        if (normalized.Contains("joke"))
            return "Why do programmers prefer dark mode? Because light attracts bugs!";

        if (normalized.Contains("weather"))
            return "It's always sunny in mock-land! Temp: 72°F, wind: 0 mph, chance of bugs: 100%.";

        if (normalized.Contains("time") || normalized.Contains("date"))
            return $"Current server time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC.";

        if (normalized.Contains("name") || normalized.Contains("who are you"))
            return "I'm MockBot — a stateless mock chat service for testing WebSocket clients.";

        if (normalized.Contains("bye") || normalized.Contains("goodbye") || normalized.Contains("exit"))
            return "Goodbye! Remember to close your WebSocket connection cleanly.";

        return $"Interesting! I'm just a mock. You said: \"{originalMessage}\"";
    }
}
