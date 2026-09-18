namespace HealthApi.UnitTests;

public sealed class HealthEndpointTests
{
    [Fact]
    public void Handle_ReturnsHealthyStatusAndProviderVersion()
    {
        const string expectedVersion = "9.9.9-test";
        FakeAppVersionProvider versionProvider = new(expectedVersion);

        HealthResponse response = HealthEndpoint.Handle(versionProvider);

        Assert.Equal(HealthEndpoint.HealthyStatus, response.Status);
        Assert.Equal("healthy", response.Status);
        Assert.Equal(expectedVersion, response.Version);
    }

    [Fact]
    public void Handle_ThrowsWhenVersionProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => HealthEndpoint.Handle(null!));
    }
}
