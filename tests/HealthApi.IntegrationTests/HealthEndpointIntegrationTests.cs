using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HealthApi.IntegrationTests;

/// <summary>
/// Exercises the real HTTP pipeline of the application, in memory, for the
/// success, negative, and unauthenticated request paths (PRD AC-1, AC-2,
/// AC-3, AC-6, AC-7, AC-11).
/// </summary>
public sealed class HealthEndpointIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    /// <summary>The canonical path under test.</summary>
    private const string HealthPath = "/health";

    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_ReturnsOkWithStatusAndVersionFields()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(HealthPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument document = JsonDocument.Parse(body);

        Assert.True(
            document.RootElement.TryGetProperty("status", out JsonElement status),
            "The response body must contain a 'status' field.");
        Assert.Equal("healthy", status.GetString());

        Assert.True(
            document.RootElement.TryGetProperty("version", out JsonElement version),
            "The response body must contain a 'version' field.");
        Assert.False(string.IsNullOrWhiteSpace(version.GetString()));
    }

    [Fact]
    public async Task GetHealth_ReportsTheApplicationAssemblyVersion()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(HealthPath);

        HealthResponse payload = await response.Content.ReadFromJsonAsync<HealthResponse>()
            ?? throw new InvalidOperationException("The health response body could not be deserialized.");

        Assert.Equal(ExpectedApplicationVersion(), payload.Version);
    }

    [Fact]
    public async Task GetHealth_ReturnsJsonContentType()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(HealthPath);

        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task PostHealth_ReturnsMethodNotAllowed()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsync(HealthPath, content: null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task UnknownRoute_ReturnsNotFound()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/not-a-real-route");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetHealth_WithoutCredentials_Succeeds()
    {
        using HttpClient client = _factory.CreateClient();
        using HttpRequestMessage request = new(HttpMethod.Get, HealthPath);

        using HttpResponseMessage response = await client.SendAsync(request);

        Assert.Null(request.Headers.Authorization);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static string ExpectedApplicationVersion()
    {
        Assembly applicationAssembly = typeof(AssemblyAppVersionProvider).Assembly;

        string? informationalVersion = applicationAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        Assert.False(
            string.IsNullOrWhiteSpace(informationalVersion),
            "The application assembly must expose an informational version.");

        return informationalVersion!;
    }
}
