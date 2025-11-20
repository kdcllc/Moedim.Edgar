using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moedim.Edgar.Client;
using Moedim.Edgar.Client.Impl;
using Moedim.Edgar.Options;
using Moedim.Edgar.Services;
using Moedim.Edgar.Services.Impl;


namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring SEC EDGAR services in dependency injection
/// </summary>
public static class EdgarServiceCollectionExtensions
{
    /// <summary>
    /// Adds SEC EDGAR client services to the service collection
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <param name="configureOptions">Action to configure SEC EDGAR options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSecEdgar(
        this IServiceCollection services,
        Action<SecEdgarOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddOptions<SecEdgarOptions>()
                .Configure<IConfiguration>((options, configuration) =>
                {
                    configuration.GetSection("SecEdgar").Bind(options);
                    configureOptions(options);
                })
                .Validate(o => !string.IsNullOrWhiteSpace(o.AppName), "AppName is required for SEC user agent identification.")
                .Validate(o => !string.IsNullOrWhiteSpace(o.AppVersion), "AppVersion is required for SEC user agent identification.")
                .Validate(o => !string.IsNullOrWhiteSpace(o.Email), "Email is required for SEC user agent identification.")
                .Validate(o => !o.RetryCountOverride.HasValue || o.RetryCountOverride.Value >= 0, "RetryCountOverride cannot be negative.")
                .Validate(o => o.RetryBackoffMultiplier > 0, "RetryBackoffMultiplier must be greater than zero.")
                .Validate(o => !o.RetryDelay.HasValue || o.RetryDelay.Value > TimeSpan.Zero, "RetryDelay must be greater than zero when specified.")
                .PostConfigure(options =>
                {
                    options.UserAgent ??= $"{options.AppName}/{options.AppVersion} ({options.Email})";
                })
                .ValidateOnStart();


        services.TryAddTransient<ICompanyFactsService, CompanyFactsService>();
        services.TryAddTransient<ICompanyConceptService, CompanyConceptService>();
        services.TryAddTransient<ICompanyLookupService, CompanyLookupService>();
        services.TryAddTransient<IEdgarLatestFilingsService, EdgarLatestFilingsService>();
        services.TryAddTransient<IEdgarSearchService, EdgarSearchService>();
        services.TryAddTransient<IFilingDetailsService, FilingDetailsService>();

        // Configure named HttpClient for SEC Edgar
        services.AddHttpClient<ISecEdgarClient, SecEdgarClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SecEdgarOptions>>().Value;
            client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler((sp, _) =>
        {
            var options = sp.GetRequiredService<IOptions<SecEdgarOptions>>().Value;
            var logger = sp.GetRequiredService<ILogger<SecEdgarClient>>();
            return SecEdgarHttpPolicies.CreateRetryPolicy(options, logger);
        });

        return services;
    }
}
