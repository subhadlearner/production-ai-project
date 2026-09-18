using HealthApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// The version never changes while the process is running.
builder.Services.AddSingleton<IAppVersionProvider, AssemblyAppVersionProvider>();

WebApplication app = builder.Build();

app.MapHealthEndpoint();

app.Run();

// Exposed so integration tests can host the application through
// WebApplicationFactory<Program> (see ADR-004).
public partial class Program
{
}
