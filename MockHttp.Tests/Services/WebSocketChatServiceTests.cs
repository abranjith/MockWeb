using MockHttp.Services;
using Xunit;

namespace MockHttp.Tests.Services;

public class WebSocketChatServiceTests
{
    private readonly WebSocketChatService _service = new();

    // --- Hello / Hi / Hey ---

    [Fact]
    public void GetResponse_HelloKeyword_ReturnsGreeting()
    {
        var result = _service.GetResponse("hello");
        Assert.Contains("MockBot", result);
    }

    [Fact]
    public void GetResponse_HiKeyword_ReturnsGreeting()
    {
        var result = _service.GetResponse("Hi there");
        Assert.Contains("MockBot", result);
    }

    [Fact]
    public void GetResponse_HeyKeyword_ReturnsGreeting()
    {
        var result = _service.GetResponse("hey there");
        Assert.Contains("MockBot", result);
    }

    // --- Help ---

    [Fact]
    public void GetResponse_HelpKeyword_ReturnsHelp()
    {
        var result = _service.GetResponse("help me");
        Assert.Contains("echo messages", result);
    }

    // --- Joke ---

    [Fact]
    public void GetResponse_JokeKeyword_ReturnsJoke()
    {
        var result = _service.GetResponse("tell me a joke");
        Assert.True(result.Contains("dark mode") || result.Contains("bugs"),
            $"Expected joke response but got: {result}");
    }

    // --- Weather ---

    [Fact]
    public void GetResponse_WeatherKeyword_ReturnsWeather()
    {
        var result = _service.GetResponse("what's the weather");
        Assert.Contains("mock-land", result);
    }

    // --- Time / Date ---

    [Fact]
    public void GetResponse_TimeKeyword_ReturnsTimestamp()
    {
        var result = _service.GetResponse("what time is it");
        Assert.Contains(DateTime.UtcNow.Year.ToString(), result);
    }

    [Fact]
    public void GetResponse_DateKeyword_ReturnsTimestamp()
    {
        var result = _service.GetResponse("what's the date");
        Assert.Contains(DateTime.UtcNow.Year.ToString(), result);
    }

    // --- Name / Who are you ---

    [Fact]
    public void GetResponse_NameKeyword_ReturnsName()
    {
        var result = _service.GetResponse("what's your name");
        Assert.Contains("MockBot", result);
    }

    [Fact]
    public void GetResponse_WhoAreYouKeyword_ReturnsName()
    {
        var result = _service.GetResponse("who are you");
        Assert.Contains("MockBot", result);
    }

    // --- Bye / Goodbye / Exit ---

    [Fact]
    public void GetResponse_ByeKeyword_ReturnsGoodbye()
    {
        var result = _service.GetResponse("bye");
        Assert.Contains("Goodbye", result);
    }

    [Fact]
    public void GetResponse_GoodbyeKeyword_ReturnsGoodbye()
    {
        var result = _service.GetResponse("goodbye");
        Assert.Contains("Goodbye", result);
    }

    [Fact]
    public void GetResponse_ExitKeyword_ReturnsGoodbye()
    {
        var result = _service.GetResponse("exit");
        Assert.Contains("Goodbye", result);
    }

    // --- Fallback ---

    [Fact]
    public void GetResponse_UnknownInput_ReturnsFallback()
    {
        var result = _service.GetResponse("zxqwerty");
        Assert.Contains("zxqwerty", result);
    }

    [Fact]
    public void GetResponse_FallbackIncludesOriginalMessage()
    {
        var original = "xyzzy_unique_test_string";
        var result = _service.GetResponse(original);
        Assert.Contains(original, result);
    }

    // --- Case insensitivity ---

    [Fact]
    public void GetResponse_IsCaseInsensitive()
    {
        var lower = _service.GetResponse("hello");
        var upper = _service.GetResponse("HELLO");
        Assert.Equal(lower, upper);
    }

    [Fact]
    public void GetResponse_MixedCaseJoke_ReturnsJoke()
    {
        var result = _service.GetResponse("JOKE");
        Assert.True(result.Contains("dark mode") || result.Contains("bugs"));
    }

    // --- Edge cases ---

    [Fact]
    public void GetResponse_EmptyString_DoesNotThrow()
    {
        var ex = Record.Exception(() => _service.GetResponse(""));
        Assert.Null(ex);
    }

    [Fact]
    public void GetResponse_EmptyString_ReturnsFallback()
    {
        var result = _service.GetResponse("");
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetResponse_WhitespaceOnly_ReturnsFallback()
    {
        var result = _service.GetResponse("   ");
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetResponse_ReturnsNonNullForAnyInput()
    {
        string[] inputs = ["hello", "HELP", "joke", "weather", "time", "name", "bye", "unknown xyz"];
        foreach (var input in inputs)
        {
            var result = _service.GetResponse(input);
            Assert.NotNull(result);
        }
    }
}
