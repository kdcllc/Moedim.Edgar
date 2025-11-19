using Moedim.Edgar.Services.Data;

namespace Moedim.Edgar.Sample;

/// <summary>
/// Sample application demonstrating Moedim.Edgar library usage
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Moedim.Edgar Sample Application ===\n");

        // Setup dependency injection
        var services = new ServiceCollection();

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

        var serviceProvider = services.BuildServiceProvider();

        // Run examples
        await RunCompanyFactsExample(serviceProvider);
        await RunCompanyConceptExample(serviceProvider);

        Console.WriteLine("\n=== Sample Complete ===");
    }

    static async Task RunCompanyFactsExample(ServiceProvider serviceProvider)
    {
        Console.WriteLine("--- Company Facts Example ---");

        var companyFactsService = serviceProvider.GetRequiredService<ICompanyFactsService>();

        // Apple Inc. CIK: 320193
        Console.WriteLine("Fetching company facts for Apple Inc. (CIK: 320193)...\n");

        try
        {
            var facts = await companyFactsService.QueryAsync(320193);

            Console.WriteLine($"Entity Name: {facts.EntityName}");
            Console.WriteLine($"CIK: {facts.CIK}");
            Console.WriteLine($"Total Facts: {facts.Facts?.Length ?? 0}");

            if (facts.Facts?.Length > 0)
            {
                Console.WriteLine("\nSample Facts:");
                foreach (var fact in facts.Facts.Take(5))
                {
                    Console.WriteLine($"  - {fact.Label ?? fact.Tag}: (has {fact.DataPoints?.Length ?? 0} data points)");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    static async Task RunCompanyConceptExample(ServiceProvider serviceProvider)
    {
        Console.WriteLine("--- Company Concept Example ---");

        var companyConceptService = serviceProvider.GetRequiredService<ICompanyConceptService>();

        // Microsoft CIK: 789019, Concept: Revenues
        Console.WriteLine("Fetching Revenue data for Microsoft (CIK: 789019)...\n");

        try
        {
            var concept = await companyConceptService.QueryAsync(789019, "Revenues");

            Console.WriteLine($"Entity Name: {concept.EntityName}");
            Console.WriteLine($"CIK: {concept.CIK}");

            if (concept.Result != null)
            {
                Console.WriteLine($"Tag: {concept.Result.Tag}");
                Console.WriteLine($"Label: {concept.Result.Label}");
                Console.WriteLine($"Total Data Points: {concept.Result.DataPoints?.Length ?? 0}");

                if (concept.Result.DataPoints?.Length > 0)
                {
                    Console.WriteLine("\nRecent Revenue Data:");
                    foreach (var dataPoint in concept.Result.DataPoints.OrderByDescending(d => d.Filed).Take(5))
                    {
                        Console.WriteLine($"  Period: {dataPoint.Period}, Value: {dataPoint.Value:N0}, Filed: {dataPoint.Filed:yyyy-MM-dd}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine();
    }
}
