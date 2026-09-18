using System.Text.Json.Serialization;

namespace HealthApi;

/// <summary>
/// The JSON contract returned by <c>GET /health</c>.
/// </summary>
/// <param name="Status">Health indicator for the service.</param>
/// <param name="Version">Application version sourced from build metadata.</param>
public sealed record HealthResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("version")] string Version);
