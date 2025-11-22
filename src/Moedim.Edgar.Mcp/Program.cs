using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moedim.Edgar.Mcp.Tools;
using Moedim.Edgar.Mcp.Prompts;

var builder = Host.CreateApplicationBuilder(args);

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Register Moedim.Edgar services with required configuration
builder.Services.AddSecEdgar(options =>
{
    // Get configuration from environment variables or use defaults
    options.AppName = Environment.GetEnvironmentVariable("EDGAR_APP_NAME") ?? "Moedim.Edgar.Mcp";
    options.AppVersion = Environment.GetEnvironmentVariable("EDGAR_APP_VERSION") ?? "1.0.0";
    options.Email = Environment.GetEnvironmentVariable("EDGAR_EMAIL") ?? "user@example.com";

    // Optional: Configure retry and delay settings
    options.RequestDelay = TimeSpan.FromMilliseconds(
        int.TryParse(Environment.GetEnvironmentVariable("EDGAR_REQUEST_DELAY_MS"), out var delay)
            ? delay : 100);

    options.MaxRetryCount = int.TryParse(Environment.GetEnvironmentVariable("EDGAR_MAX_RETRY_COUNT"), out var retryCount)
        ? retryCount : 3;
});

// Add the MCP services: the transport to use (stdio) and the tools/prompts to register.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<CompanyDataTools>()
    .WithTools<FilingSearchTools>()
    .WithTools<FilingDetailsTools>()
    .WithPrompts<EdgarPrompts>();

await builder.Build().RunAsync();
