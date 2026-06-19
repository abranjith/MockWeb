using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;

namespace MockHttp.Tests.Controllers;

public class DataControllerQuickArrayTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DataControllerQuickArrayTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetQuickArray_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/data/quick-array");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetQuickArray_ReturnsJsonArrayWithExpectedItemCount()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/data/quick-array");
        var body = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(body);
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        Assert.Equal(3, document.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task GetQuickArray_ItemsContainExpectedPropertiesAndTypes()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/data/quick-array");
        var body = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(body);

        foreach (var item in document.RootElement.EnumerateArray())
        {
            Assert.True(item.TryGetProperty("id", out var idProperty));
            Assert.Equal(JsonValueKind.Number, idProperty.ValueKind);

            Assert.True(item.TryGetProperty("name", out var nameProperty));
            Assert.Equal(JsonValueKind.String, nameProperty.ValueKind);
            Assert.False(string.IsNullOrWhiteSpace(nameProperty.GetString()));

            Assert.True(item.TryGetProperty("category", out var categoryProperty));
            Assert.Equal(JsonValueKind.String, categoryProperty.ValueKind);

            Assert.True(item.TryGetProperty("isActive", out var isActiveProperty));
            Assert.True(
                isActiveProperty.ValueKind == JsonValueKind.True ||
                isActiveProperty.ValueKind == JsonValueKind.False);
        }
    }
}
