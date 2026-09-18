namespace HealthApi;

/// <summary>
/// Supplies the application version reported by the health endpoint.
/// </summary>
/// <remarks>
/// Abstracted so the health handler can be unit tested with a hand-written
/// fake instead of a mocking framework (see docs/adr/ADR-004-testing-stack.md).
/// </remarks>
public interface IAppVersionProvider
{
    /// <summary>
    /// Returns the application's current version, sourced from build metadata.
    /// </summary>
    /// <returns>The application version.</returns>
    string GetVersion();
}
