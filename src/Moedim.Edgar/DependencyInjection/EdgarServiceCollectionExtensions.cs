using Moedim.Edgar.Models;
using Moedim.Edgar.Options;
using Moedim.Edgar.Services;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring SEC EDGAR services in dependency injection
/// </summary>
public static class EdgarServiceCollectionExtensions
{
    /// <summary>
    /// Adds SEC EDGAR client services to the service collection
    /// </summary>
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
        services.AddSingleton<ICompanyFactsService, CompanyFactsService>();
        services.AddSingleton<ICompanyConceptService, CompanyConceptService>();

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
