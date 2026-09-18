using System.Reflection;

namespace HealthApi;

/// <summary>
/// Reads the application version from this assembly's build metadata, so the
/// value reported by the health endpoint can never drift from the compiled
/// build.
/// </summary>
/// <remarks>
/// The assembly that defines this provider (the application assembly) is used
/// rather than <see cref="Assembly.GetEntryAssembly"/>, because the entry
/// assembly is the test host when the application is hosted by
/// <c>WebApplicationFactory</c>. Reading the application assembly keeps the
/// reported version correct in every hosting context.
/// </remarks>
public sealed class AssemblyAppVersionProvider : IAppVersionProvider
{
    /// <summary>Fallback used only if no version metadata is available.</summary>
    internal const string UnknownVersion = "unknown";

    /// <inheritdoc />
    public string GetVersion()
    {
        Assembly applicationAssembly = typeof(AssemblyAppVersionProvider).Assembly;

        string? informationalVersion = applicationAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return informationalVersion;
        }

        Version? assemblyVersion = applicationAssembly.GetName().Version;

        return assemblyVersion is null ? UnknownVersion : assemblyVersion.ToString();
    }
}
