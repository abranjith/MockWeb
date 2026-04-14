namespace MockHttp.Services;

/// <summary>Resolves a client message to a canned mock chat response.</summary>
public interface IWebSocketChatService
{
    /// <summary>Returns a mock response for <paramref name="message"/>.</summary>
    string GetResponse(string message);
}
