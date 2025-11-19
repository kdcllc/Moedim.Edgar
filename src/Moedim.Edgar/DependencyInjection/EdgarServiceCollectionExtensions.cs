using Moedim.Edgar.Client;
using Moedim.Edgar.Client.Impl;
using Moedim.Edgar.Models;
using Moedim.Edgar.Options;

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

        var options = new SecEdgarOptions();
        configureOptions(options);
        options.Validate();

        services.AddSingleton(options);
        services.AddSingleton<ISecEdgarClient, SecEdgarClient>();
        services.AddSingleton<ICompanyFactsService>(sp =>
            new CompanyFactsService(sp.GetRequiredService<ISecEdgarClient>(), options));
        services.AddSingleton<ICompanyConceptService>(sp =>
            new CompanyConceptService(sp.GetRequiredService<ISecEdgarClient>(), options));

        // Configure named HttpClient for SEC Edgar
        services.AddHttpClient("SecEdgar", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
