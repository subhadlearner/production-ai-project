namespace HealthApi.UnitTests;

/// <summary>
/// Hand-written test double for <see cref="IAppVersionProvider"/>. A mocking
/// framework is deliberately not used (see ADR-004).
/// </summary>
internal sealed class FakeAppVersionProvider(string version) : IAppVersionProvider
{
    public string GetVersion() => version;
}
