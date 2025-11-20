using Microsoft.Extensions.Configuration;
using Moedim.Edgar.Models.Fillings;
using Moedim.Edgar.Services;

namespace Moedim.Edgar.Sample;

/// <summary>
/// Sample application demonstrating all Moedim.Edgar library features
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

        // Add SEC EDGAR services with configuration
        services.AddSecEdgar(options =>
        {
            options.AppName = "Moedim.Edgar.Sample";
            options.AppVersion = "1.0.0";
            options.Email = "sample@example.com";
            options.RequestDelay = TimeSpan.FromMilliseconds(100);
            options.MaxRetryCount = 3;
            options.TimeoutDelay = TimeSpan.FromSeconds(30);
            options.UseExponentialBackoff = true;
            options.RetryBackoffMultiplier = 2;
        });

        services.AddSingleton<IConfiguration>(configuration);

        var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("=== Moedim.Edgar Comprehensive Sample Application ===");
        logger.LogInformation("");

        // Run all service examples
        await RunCompanyLookupExamples(sp);
        await RunCompanyFactsExamples(sp);
        await RunCompanyConceptExamples(sp);
        await RunEdgarSearchExamples(sp);
        await RunEdgarLatestFilingsExamples(sp);
        await RunFilingDetailsExamples(sp);

        logger.LogInformation("");
        logger.LogInformation("=== All Samples Complete ===");
    }

    #region Company Lookup Service Examples

    static async Task RunCompanyLookupExamples(ServiceProvider sp)
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        var service = sp.GetRequiredService<ICompanyLookupService>();

        logger.LogInformation("========================================");
        logger.LogInformation("COMPANY LOOKUP SERVICE EXAMPLES");
        logger.LogInformation("========================================");
        logger.LogInformation("");

        // Example 1: Lookup by common stock symbol
        await Example("Lookup Apple by Symbol (AAPL)", async () =>
        {
            var cik = await service.GetCikFromSymbolAsync("AAPL");
            logger.LogInformation("  Symbol: AAPL -> CIK: {CIK}", cik);
        }, logger);

        // Example 2: Lookup by another symbol
        await Example("Lookup Microsoft by Symbol (MSFT)", async () =>
        {
            var cik = await service.GetCikFromSymbolAsync("MSFT");
            logger.LogInformation("  Symbol: MSFT -> CIK: {CIK}", cik);
        }, logger);

        // Example 3: Lookup Tesla
        await Example("Lookup Tesla by Symbol (TSLA)", async () =>
        {
            var cik = await service.GetCikFromSymbolAsync("TSLA");
            logger.LogInformation("  Symbol: TSLA -> CIK: {CIK}", cik);
        }, logger);

        logger.LogInformation("");
    }

    #endregion

    #region Company Facts Service Examples

    static async Task RunCompanyFactsExamples(ServiceProvider sp)
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        var service = sp.GetRequiredService<ICompanyFactsService>();

        logger.LogInformation("========================================");
        logger.LogInformation("COMPANY FACTS SERVICE EXAMPLES");
        logger.LogInformation("========================================");
        logger.LogInformation("");

        // Example 1: Get all facts for Apple
        await Example("Get All Facts for Apple Inc. (CIK: 320193)", async () =>
        {
            var facts = await service.QueryAsync(320193);
            logger.LogInformation("  Entity: {EntityName} (CIK: {CIK})", facts.EntityName, facts.CIK);
            logger.LogInformation("  Total Facts: {Count}", facts.Facts?.Length ?? 0);

            if (facts.Facts?.Length > 0)
            {
                logger.LogInformation("  Sample Facts (first 5):");
                foreach (var fact in facts.Facts.Take(5))
                {
                    logger.LogInformation("    - {Label} ({Tag}): {DataPoints} data points",
                        fact.Label ?? "N/A", fact.Tag, fact.DataPoints?.Length ?? 0);
                }
            }
        }, logger);

        // Example 2: Get facts for Microsoft
        await Example("Get All Facts for Microsoft (CIK: 789019)", async () =>
        {
            var facts = await service.QueryAsync(789019);
            logger.LogInformation("  Entity: {EntityName}", facts.EntityName);
            logger.LogInformation("  Total Facts: {Count}", facts.Facts?.Length ?? 0);

            // Find specific facts like Assets, Revenues, etc.
            if (facts.Facts?.Length > 0)
            {
                var revenue = facts.Facts.FirstOrDefault(f =>
                    f.Tag?.Contains("Revenue", StringComparison.OrdinalIgnoreCase) == true);
                if (revenue != null)
                {
                    logger.LogInformation("  Found Revenue fact: {Label} with {Count} data points",
                        revenue.Label, revenue.DataPoints?.Length ?? 0);
                }
            }
        }, logger);

        // Example 3: Get facts with taxonomy filtering
        await Example("Get Facts Analysis for Tesla (CIK: 1318605)", async () =>
        {
            var facts = await service.QueryAsync(1318605);
            logger.LogInformation("  Entity: {EntityName}", facts.EntityName);

            if (facts.Facts?.Length > 0)
            {
                // Analyze facts by their tags
                var factsByTag = facts.Facts.GroupBy(f => f.Tag?.Substring(0, Math.Min(3, f.Tag.Length)) ?? "N/A").ToList();
                logger.LogInformation("  Total Facts: {Count}", facts.Facts.Length);
                logger.LogInformation("  Fact Tag Prefixes:");
                foreach (var group in factsByTag.Take(5))
                {
                    logger.LogInformation("    {Prefix}*: {Count} facts", group.Key, group.Count());
                }
            }
        }, logger);

        logger.LogInformation("");
    }

    #endregion

    #region Company Concept Service Examples

    static async Task RunCompanyConceptExamples(ServiceProvider sp)
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        var service = sp.GetRequiredService<ICompanyConceptService>();

        logger.LogInformation("========================================");
        logger.LogInformation("COMPANY CONCEPT SERVICE EXAMPLES");
        logger.LogInformation("========================================");
        logger.LogInformation("");

        // Example 1: Query Revenue concept for Microsoft
        await Example("Query Revenues for Microsoft (CIK: 789019)", async () =>
        {
            var concept = await service.QueryAsync(789019, "Revenues");
            logger.LogInformation("  Entity: {EntityName} (CIK: {CIK})", concept.EntityName, concept.CIK);

            if (concept.Result != null)
            {
                logger.LogInformation("  Concept: {Label} ({Tag})", concept.Result.Label, concept.Result.Tag);
                logger.LogInformation("  Description: {Description}", concept.Result.Description);
                logger.LogInformation("  Total Data Points: {Count}", concept.Result.DataPoints?.Length ?? 0);

                if (concept.Result.DataPoints?.Length > 0)
                {
                    var recent = concept.Result.DataPoints
                        .Where(dp => dp.Period != default && dp.Value != 0)
                        .OrderByDescending(dp => dp.Filed)
                        .Take(3);

                    logger.LogInformation("  Recent Filings:");
                    foreach (var dp in recent)
                    {
                        logger.LogInformation("    Period: {Period:yyyy-MM-dd}, Value: ${Value:N0}, Filed: {Filed:yyyy-MM-dd}",
                            dp.Period, dp.Value, dp.Filed);
                    }
                }
            }
        }, logger);

        // Example 2: Query Assets for Apple
        await Example("Query Assets for Apple (CIK: 320193)", async () =>
        {
            var concept = await service.QueryAsync(320193, "Assets");

            if (concept.Result != null)
            {
                logger.LogInformation("  Concept: {Label}", concept.Result.Label);
                logger.LogInformation("  Data Points: {Count}", concept.Result.DataPoints?.Length ?? 0);

                if (concept.Result.DataPoints?.Length > 0)
                {
                    // Show different types of data points
                    var dataPointsWithValues = concept.Result.DataPoints.Where(dp => dp.Value != 0).ToList();
                    logger.LogInformation("  Data Points with Values: {Count} out of {Total}",
                        dataPointsWithValues.Count, concept.Result.DataPoints.Length);

                    if (dataPointsWithValues.Count > 0)
                    {
                        logger.LogInformation("  Recent Data Points:");
                        foreach (var dp in dataPointsWithValues.OrderByDescending(d => d.Filed).Take(3))
                        {
                            logger.LogInformation("    Period: {Period:yyyy-MM-dd}, Value: ${Value:N0}",
                                dp.Period, dp.Value);
                        }
                    }
                }
            }
        }, logger);

        // Example 3: Query Liabilities
        await Example("Query Liabilities for Tesla (CIK: 1318605)", async () =>
        {
            var concept = await service.QueryAsync(1318605, "Liabilities");

            if (concept.Result != null)
            {
                logger.LogInformation("  Tag: {Tag}, Label: {Label}", concept.Result.Tag, concept.Result.Label);

                if (concept.Result.DataPoints?.Length > 0)
                {
                    var latest = concept.Result.DataPoints
                        .Where(dp => dp.Value != 0)
                        .OrderByDescending(dp => dp.Filed)
                        .FirstOrDefault();

                    if (latest != null)
                    {
                        logger.LogInformation("  Latest Value: ${Value:N0} (Period: {Period:yyyy-MM-dd})",
                            latest.Value, latest.Period);
                    }
                }
            }
        }, logger);

        logger.LogInformation("");
    }

    #endregion

    #region Edgar Search Service Examples

    static async Task RunEdgarSearchExamples(ServiceProvider sp)
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        var service = sp.GetRequiredService<IEdgarSearchService>();

        logger.LogInformation("========================================");
        logger.LogInformation("EDGAR SEARCH SERVICE EXAMPLES");
        logger.LogInformation("========================================");
        logger.LogInformation("");

        // Example 1: Search all filings for a company by symbol
        await Example("Search All Filings for Apple (AAPL)", async () =>
        {
            var query = new EdgarSearchQuery
            {
                Symbol = "AAPL",
                ResultsPerPage = EdgarSearchResultsPerPage.Entries40
            };

            var response = await service.SearchAsync(query);
            logger.LogInformation("  Total Results: {Count}", response.Results?.Length ?? 0);
            logger.LogInformation("  Has Next Page: {HasNext}", !string.IsNullOrEmpty(response.NextPageUrl));

            if (response.Results?.Length > 0)
            {
                logger.LogInformation("  Recent Filings (first 5):");
                foreach (var filing in response.Results.Take(5))
                {
                    logger.LogInformation("    {Form} - {Description} ({FilingDate:yyyy-MM-dd})",
                        filing.Filing, filing.Description, filing.FilingDate);
                }
            }
        }, logger);

        // Example 2: Search for specific form type (10-K annual reports)
        await Example("Search 10-K Filings for Microsoft (MSFT)", async () =>
        {
            var query = new EdgarSearchQuery
            {
                Symbol = "MSFT",
                FilingType = "10-K",
                ResultsPerPage = EdgarSearchResultsPerPage.Entries100,
                OwnershipFilter = EdgarSearchOwnershipFilter.Exclude
            };

            var response = await service.SearchAsync(query);
            logger.LogInformation("  10-K Filings Found: {Count}", response.Results?.Length ?? 0);

            if (response.Results?.Length > 0)
            {
                logger.LogInformation("  Annual Reports:");
                foreach (var filing in response.Results.Take(3))
                {
                    logger.LogInformation("    {FilingDate:yyyy-MM-dd}: {Description}",
                        filing.FilingDate, filing.Description);
                    logger.LogInformation("      URL: {Url}", filing.DocumentsUrl);
                }
            }
        }, logger);

        // Example 3: Search with date filter
        await Example("Search Recent 8-K Filings for Tesla (TSLA)", async () =>
        {
            var query = new EdgarSearchQuery
            {
                Symbol = "TSLA",
                FilingType = "8-K",
                PriorTo = DateTime.Now.AddMonths(-6), // Last 6 months
                ResultsPerPage = EdgarSearchResultsPerPage.Entries40
            };

            var response = await service.SearchAsync(query);
            logger.LogInformation("  8-K Filings (last 6 months): {Count}", response.Results?.Length ?? 0);

            if (response.Results?.Length > 0)
            {
                logger.LogInformation("  Recent Current Reports:");
                foreach (var filing in response.Results.Take(5))
                {
                    logger.LogInformation("    {FilingDate:yyyy-MM-dd}: {Form}",
                        filing.FilingDate, filing.Filing);
                }
            }
        }, logger);

        // Example 4: Search by CIK instead of symbol
        await Example("Search by CIK for Apple (320193)", async () =>
        {
            var query = new EdgarSearchQuery
            {
                Symbol = "320193",
                FilingType = "10-Q", // Quarterly reports
                ResultsPerPage = EdgarSearchResultsPerPage.Entries40
            };

            var response = await service.SearchAsync(query);
            logger.LogInformation("  10-Q Filings: {Count}", response.Results?.Length ?? 0);
        }, logger);

        // Example 5: Pagination example
        await Example("Pagination - Get Next Page of Results", async () =>
        {
            var query = new EdgarSearchQuery
            {
                Symbol = "AAPL",
                ResultsPerPage = EdgarSearchResultsPerPage.Entries10
            };

            var firstPage = await service.SearchAsync(query);
            logger.LogInformation("  First Page: {Count} filings", firstPage.Results?.Length ?? 0);

            if (!string.IsNullOrEmpty(firstPage.NextPageUrl))
            {
                var secondPage = await service.NextPageAsync(firstPage.NextPageUrl);
                logger.LogInformation("  Second Page: {Count} filings", secondPage.Results?.Length ?? 0);
                logger.LogInformation("  Has More Pages: {HasMore}", !string.IsNullOrEmpty(secondPage.NextPageUrl));
            }
        }, logger);

        logger.LogInformation("");
    }

    #endregion

    #region Edgar Latest Filings Service Examples

    static async Task RunEdgarLatestFilingsExamples(ServiceProvider sp)
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        var service = sp.GetRequiredService<IEdgarLatestFilingsService>();

        logger.LogInformation("========================================");
        logger.LogInformation("EDGAR LATEST FILINGS SERVICE EXAMPLES");
        logger.LogInformation("========================================");
        logger.LogInformation("");

        // Example 1: Get all latest filings
        await Example("Get Latest Filings (All Forms)", async () =>
        {
            var query = new EdgarLatestFilingsQuery
            {
                ResultsPerPage = EdgarSearchResultsPerPage.Entries40,
                OwnershipFilter = EdgarSearchOwnershipFilter.Include
            };

            var results = await service.SearchAsync(query);
            logger.LogInformation("  Latest Filings Retrieved: {Count}", results?.Length ?? 0);

            if (results?.Length > 0)
            {
                logger.LogInformation("  Recent Filings Across All Companies:");
                foreach (var filing in results.Take(5))
                {
                    logger.LogInformation("    {Company}: {Form} ({FilingDate:yyyy-MM-dd})",
                        filing.EntityTitle, filing.Filing, filing.FilingDate);
                }
            }
        }, logger);

        // Example 2: Filter by form type (10-K only)
        await Example("Get Latest 10-K Filings Only", async () =>
        {
            var query = new EdgarLatestFilingsQuery
            {
                FormType = "10-K",
                ResultsPerPage = EdgarSearchResultsPerPage.Entries100,
                OwnershipFilter = EdgarSearchOwnershipFilter.Exclude
            };

            var results = await service.SearchAsync(query);
            logger.LogInformation("  Latest 10-K Filings: {Count}", results?.Length ?? 0);

            if (results?.Length > 0)
            {
                logger.LogInformation("  Recent Annual Reports:");
                foreach (var filing in results.Take(5))
                {
                    logger.LogInformation("    {Company} - CIK: {CIK} ({FilingDate:yyyy-MM-dd})",
                        filing.EntityTitle, filing.EntityCik, filing.FilingDate);
                }
            }
        }, logger);

        // Example 3: Get 8-K current reports
        await Example("Get Latest 8-K Current Reports", async () =>
        {
            var query = new EdgarLatestFilingsQuery
            {
                FormType = "8-K",
                ResultsPerPage = EdgarSearchResultsPerPage.Entries40,
                OwnershipFilter = EdgarSearchOwnershipFilter.Exclude
            };

            var results = await service.SearchAsync(query);
            logger.LogInformation("  Latest 8-K Filings: {Count}", results?.Length ?? 0);

            if (results?.Length > 0)
            {
                var grouped = results.GroupBy(f => f.Filing).ToList();
                logger.LogInformation("  Form Types Found:");
                foreach (var group in grouped)
                {
                    logger.LogInformation("    {FormType}: {Count} filings", group.Key, group.Count());
                }
            }
        }, logger);

        // Example 4: Include ownership filings
        await Example("Get Latest Filings Including Ownership (Form 3, 4, 5)", async () =>
        {
            var query = new EdgarLatestFilingsQuery
            {
                ResultsPerPage = EdgarSearchResultsPerPage.Entries40,
                OwnershipFilter = EdgarSearchOwnershipFilter.Only // Only ownership filings
            };

            var results = await service.SearchAsync(query);
            logger.LogInformation("  Ownership Filings: {Count}", results?.Length ?? 0);

            if (results?.Length > 0)
            {
                logger.LogInformation("  Recent Insider Transactions:");
                foreach (var filing in results.Take(5))
                {
                    logger.LogInformation("    {Company}: {Form} ({FilingDate:yyyy-MM-dd})",
                        filing.EntityTitle, filing.Filing, filing.FilingDate);
                }
            }
        }, logger);

        // Example 5: Different results per page options
        await Example("Get Latest 10-Q Filings with Different Page Sizes", async () =>
        {
            var sizes = new[]
            {
                EdgarSearchResultsPerPage.Entries10,
                EdgarSearchResultsPerPage.Entries40,
                EdgarSearchResultsPerPage.Entries80,
                EdgarSearchResultsPerPage.Entries100
            };

            foreach (var size in sizes)
            {
                var query = new EdgarLatestFilingsQuery
                {
                    FormType = "10-Q",
                    ResultsPerPage = size,
                    OwnershipFilter = EdgarSearchOwnershipFilter.Exclude
                };

                var results = await service.SearchAsync(query);
                logger.LogInformation("  Page Size {Size}: Retrieved {Count} filings",
                    size, results?.Length ?? 0);
            }
        }, logger);

        logger.LogInformation("");
    }

    #endregion

    #region Filing Details Service Examples

    static async Task RunFilingDetailsExamples(ServiceProvider sp)
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        var service = sp.GetRequiredService<IFilingDetailsService>();
        var searchService = sp.GetRequiredService<IEdgarSearchService>();

        logger.LogInformation("========================================");
        logger.LogInformation("FILING DETAILS SERVICE EXAMPLES");
        logger.LogInformation("========================================");
        logger.LogInformation("");

        // First, get a filing URL to work with
        string? filingUrl = null;
        await Example("Get a Sample Filing URL", async () =>
        {
            var query = new EdgarSearchQuery
            {
                Symbol = "AAPL",
                FilingType = "10-K",
                ResultsPerPage = EdgarSearchResultsPerPage.Entries10
            };

            var response = await searchService.SearchAsync(query);
            if (response.Results?.Length > 0)
            {
                var filing = response.Results[0];
                filingUrl = filing.DocumentsUrl;
                logger.LogInformation("  Using filing: {Form} from {Date:yyyy-MM-dd}",
                    filing.Filing, filing.FilingDate);
                logger.LogInformation("  Documents URL: {Url}", filingUrl);
            }
        }, logger);

        if (string.IsNullOrEmpty(filingUrl))
        {
            logger.LogWarning("  No filing URL available for remaining examples");
            logger.LogInformation("");
            return;
        }

        // Example 1: Get complete filing details
        await Example("Get Complete Filing Details", async () =>
        {
            var details = await service.GetFilingDetailsAsync(filingUrl);
            logger.LogInformation("  Filing Details:");
            logger.LogInformation("    Entity: {Name} (CIK: {CIK})", details.EntityName, details.EntityCik);
            logger.LogInformation("    Form Type: {Form}", details.Form);
            logger.LogInformation("    Accession Number: {P1}-{P2}-{P3}",
                details.AccessionNumberP1, details.AccessionNumberP2, details.AccessionNumberP3);
            logger.LogInformation("    Filing Date: {Date:yyyy-MM-dd}", details.FilingDate);
            logger.LogInformation("    Period of Report: {Date:yyyy-MM-dd}", details.PeriodOfReport);
            logger.LogInformation("    Accepted: {Date:yyyy-MM-dd HH:mm:ss}", details.Accepted);
            logger.LogInformation("    Document Format Files: {Count}", details.DocumentFormatFiles?.Length ?? 0);
            logger.LogInformation("    Data Files: {Count}", details.DataFiles?.Length ?? 0);
        }, logger);

        // Example 2: Extract CIK from filing
        await Example("Extract CIK from Filing URL", async () =>
        {
            var cik = await service.GetCikFromFilingAsync(filingUrl);
            logger.LogInformation("  Extracted CIK: {CIK}", cik);
        }, logger);

        // Example 3: Get document format files
        await Example("Get Document Format Files", async () =>
        {
            var documents = await service.GetDocumentFormatFilesAsync(filingUrl);
            logger.LogInformation("  Document Format Files: {Count}", documents?.Length ?? 0);

            if (documents?.Length > 0)
            {
                logger.LogInformation("  Documents:");
                foreach (var doc in documents.Take(5))
                {
                    logger.LogInformation("    [{Seq}] {Description} - {Name} ({Type}, {Size:N0} bytes)",
                        doc.Sequence, doc.Description, doc.DocumentName, doc.DocumentType, doc.Size);
                }
            }
        }, logger);

        // Example 4: Get data files
        await Example("Get Data Files (XBRL, etc.)", async () =>
        {
            var dataFiles = await service.GetDataFilesAsync(filingUrl);
            logger.LogInformation("  Data Files: {Count}", dataFiles?.Length ?? 0);

            if (dataFiles?.Length > 0)
            {
                logger.LogInformation("  Data Files:");
                foreach (var file in dataFiles.Take(5))
                {
                    logger.LogInformation("    [{Seq}] {Description} - {Name} ({Type})",
                        file.Sequence, file.Description, file.DocumentName, file.DocumentType);
                }

                // Find XBRL instance document
                var xbrlInstance = dataFiles.FirstOrDefault(f =>
                    f.Description?.Contains("XBRL INSTANCE", StringComparison.OrdinalIgnoreCase) == true ||
                    f.DocumentType?.Contains("INS", StringComparison.OrdinalIgnoreCase) == true);

                if (xbrlInstance != null)
                {
                    logger.LogInformation("  XBRL Instance Document Found: {Name}", xbrlInstance.DocumentName);
                }
            }
        }, logger);

        // Example 5: Download XBRL document
        await Example("Download XBRL Instance Document", async () =>
        {
            var xbrlStream = await service.DownloadXbrlDocumentAsync(filingUrl);
            logger.LogInformation("  XBRL Document downloaded successfully");
            logger.LogInformation("  Stream Length: {Length:N0} bytes", xbrlStream?.Length ?? 0);

            if (xbrlStream != null)
            {
                // Read first few bytes to verify it's XML
                var buffer = new byte[100];
                var bytesRead = await xbrlStream.ReadAsync(buffer, 0, buffer.Length);
                var preview = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                logger.LogInformation("  Content Preview: {Preview}...",
                    preview.Length > 50 ? preview.Substring(0, 50) : preview);

                xbrlStream.Dispose();
            }
        }, logger);

        // Example 6: Download a specific document
        await Example("Download Specific Document", async () =>
        {
            var documents = await service.GetDocumentFormatFilesAsync(filingUrl);
            if (documents?.Length > 0)
            {
                var firstDoc = documents[0];
                if (!string.IsNullOrEmpty(firstDoc.Url))
                {
                    logger.LogInformation("  Downloading: {Name}", firstDoc.DocumentName);
                    var stream = await service.DownloadDocumentAsync(firstDoc.Url);
                    logger.LogInformation("  Downloaded: {Length:N0} bytes", stream?.Length ?? 0);
                    stream?.Dispose();
                }
            }
        }, logger);

        logger.LogInformation("");
    }

    #endregion

    #region Helper Methods

    static async Task Example(string title, Func<Task> action, ILogger logger)
    {
        logger.LogInformation("Example: {Title}", title);
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "  Error in example: {Message}", ex.Message);
        }
        logger.LogInformation("");
    }

    #endregion
}
