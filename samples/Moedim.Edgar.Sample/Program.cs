using Microsoft.Extensions.Configuration;
using Moedim.Edgar.Services.Data;

namespace Moedim.Edgar.Sample;

/// <summary>
/// Sample application demonstrating Moedim.Edgar library usage
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {

        // Setup dependency injection
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add SEC EDGAR services
        services.AddSecEdgar(options =>
        {
            options.AppName = "Moedim.Edgar.Sample";
            options.AppVersion = "1.0.0";
            options.Email = "sample@example.com";
            options.RequestDelay = TimeSpan.FromMilliseconds(100);
            options.MaxRetryCount = 3;
        });

        services.AddSingleton<IConfiguration>(configuration);

        var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("=== Moedim.Edgar Sample Application ===");

        // Run examples
        await RunCompanyFactsExample(sp);
        await RunCompanyConceptExample(sp);

        logger.LogInformation("=== Sample Complete ===");
    }

    static async Task RunCompanyFactsExample(ServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("--- Company Facts Example ---");

        var companyFactsService = serviceProvider.GetRequiredService<ICompanyFactsService>();

        // Apple Inc. CIK: 320193
        logger.LogInformation("Fetching company facts for Apple Inc. (CIK: 320193)...");

        try
        {
            var facts = await companyFactsService.QueryAsync(320193);

            logger.LogInformation("Entity Name: {EntityName}", facts.EntityName);
            logger.LogInformation("CIK: {CIK}", facts.CIK);
            logger.LogInformation("Total Facts: {TotalFacts}", facts.Facts?.Length ?? 0);

            if (facts.Facts?.Length > 0)
            {
                logger.LogInformation("Sample Facts:");
                foreach (var fact in facts.Facts.Take(5))
                {
                    logger.LogInformation("  - {Label}: (has {DataPoints} data points)", fact.Label ?? fact.Tag, fact.DataPoints?.Length ?? 0);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching company facts");
        }
    }

    static async Task RunCompanyConceptExample(ServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("--- Company Concept Example ---");

        var companyConceptService = serviceProvider.GetRequiredService<ICompanyConceptService>();

        // Microsoft CIK: 789019, Concept: Revenues
        logger.LogInformation("Fetching Revenue data for Microsoft (CIK: 789019)...");

        try
        {
            var concept = await companyConceptService.QueryAsync(789019, "Revenues");

            logger.LogInformation("Entity Name: {EntityName}", concept.EntityName);
            logger.LogInformation("CIK: {CIK}", concept.CIK);

            if (concept.Result != null)
            {
                logger.LogInformation("Tag: {Tag}", concept.Result.Tag);
                logger.LogInformation("Label: {Label}", concept.Result.Label);
                logger.LogInformation("Total Data Points: {TotalDataPoints}", concept.Result.DataPoints?.Length ?? 0);

                if (concept.Result.DataPoints?.Length > 0)
                {
                    logger.LogInformation("Recent Revenue Data:");
                    foreach (var dataPoint in concept.Result.DataPoints.OrderByDescending(d => d.Filed).Take(5))
                    {
                        logger.LogInformation("  Period: {Period}, Value: {Value:N0}, Filed: {Filed:yyyy-MM-dd}", dataPoint.Period, dataPoint.Value, dataPoint.Filed);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching company concept");
        }
    }
}
