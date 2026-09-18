namespace HealthApi;

/// <summary>
/// The single HTTP endpoint exposed by this service.
/// </summary>
public static class HealthEndpoint
{
    /// <summary>The literal value reported in the <c>status</c> field.</summary>
    public const string HealthyStatus = "healthy";

    /// <summary>Relative path of the health endpoint.</summary>
    public const string Path = "/health";

    /// <summary>
    /// Builds the health response for a request. Pure and side-effect free, so
    /// repeated and concurrent calls are safe.
    /// </summary>
    /// <param name="versionProvider">Provider of the application version.</param>
    /// <returns>The health response payload.</returns>
    public static HealthResponse Handle(IAppVersionProvider versionProvider)
    {
        ArgumentNullException.ThrowIfNull(versionProvider);

        return new HealthResponse(HealthyStatus, versionProvider.GetVersion());
    }

    /// <summary>
    /// Registers <c>GET /health</c> on the application. Requests using another
    /// HTTP method, or requests to an undefined route, are answered by the
    /// framework's built-in routing behavior (405 and 404 respectively).
    /// </summary>
    /// <param name="app">The application to register the endpoint on.</param>
    public static void MapHealthEndpoint(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet(Path, (IAppVersionProvider versionProvider) => Results.Ok(Handle(versionProvider)));
    }
}
