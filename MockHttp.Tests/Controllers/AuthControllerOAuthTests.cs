using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using Xunit;

namespace MockHttp.Tests.Controllers;

public class AuthControllerOAuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthControllerOAuthTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OAuthAuthorize_WithoutUsernamePassword_RedirectsWithAuthorizationCode()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/auth/authorize?response_type=code&client_id=mock-client&redirect_uri=https://example.com/callback");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var redirectUri = response.Headers.Location!;
        var query = QueryHelpers.ParseQuery(redirectUri.Query);

        Assert.Equal("https://example.com/callback", redirectUri.GetLeftPart(UriPartial.Path));
        Assert.True(query.ContainsKey("code"));
        Assert.False(string.IsNullOrWhiteSpace(query["code"].ToString()));
    }

    [Fact]
    public async Task OAuthAuthorize_WithState_IncludesStateInRedirect()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/auth/authorize?response_type=code&client_id=mock-client&redirect_uri=https://example.com/callback&state=xyz123");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var redirectUri = response.Headers.Location!;
        var query = QueryHelpers.ParseQuery(redirectUri.Query);

        Assert.Equal("xyz123", query["state"].ToString());
        Assert.True(query.ContainsKey("code"));
    }

    [Fact]
    public async Task OAuthAuthorize_MissingRedirectUri_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/authorize?response_type=code&client_id=mock-client");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OAuthCallback_WithCode_ReturnsSuccessPayload()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/callback?code=abc123&state=state1");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"success\":true", body);
        Assert.Contains("OAuth callback received successfully", body);
        Assert.Contains("\"hasCode\":true", body);
        Assert.Contains("\"hasState\":true", body);
    }

    [Fact]
    public async Task OAuthCallback_WithoutCode_StillReturnsSuccessPayload()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/callback");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"success\":true", body);
        Assert.Contains("OAuth callback received successfully", body);
        Assert.Contains("\"hasCode\":false", body);
    }
}
