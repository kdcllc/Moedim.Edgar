using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moedim.Edgar.Services;
using Moedim.Edgar.Models.Fillings;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSecEdgar(options =>
        {
            options.AppName = "Moedim.Edgar.CLI";
            options.AppVersion = "1.0.0";
            options.Email = "cli@example.com";
            options.RequestDelay = TimeSpan.FromMilliseconds(100);
            options.MaxRetryCount = 3;
        });
    })
    .Build();

// Create a scope to resolve services
using var scope = host.Services.CreateScope();
var companyLookupService = scope.ServiceProvider.GetRequiredService<ICompanyLookupService>();
var companyFactsService = scope.ServiceProvider.GetRequiredService<ICompanyFactsService>();
var companyConceptService = scope.ServiceProvider.GetRequiredService<ICompanyConceptService>();
var edgarSearchService = scope.ServiceProvider.GetRequiredService<IEdgarSearchService>();
var latestFilingsService = scope.ServiceProvider.GetRequiredService<IEdgarLatestFilingsService>();
var filingDetailsService = scope.ServiceProvider.GetRequiredService<IFilingDetailsService>();

// Parse command line arguments
if (args.Length == 0)
{
    Console.WriteLine("Moedim Edgar CLI - SEC Filing Service");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  edgar-cli company <ticker|cik>              Get company information (CIK lookup)");
    Console.WriteLine("  edgar-cli filings <ticker|cik> [form]       Get company filings (e.g., 10-K, 10-Q)");
    Console.WriteLine("  edgar-cli filing-details <url>              Get filing details from documents URL");
    Console.WriteLine("  edgar-cli facts <cik>                       Get all company facts");
    Console.WriteLine("  edgar-cli concept <cik> <tag>               Get specific concept data (e.g., Revenues, Assets)");
    Console.WriteLine("  edgar-cli latest [form]                     Get latest filings across all companies");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  edgar-cli company AAPL");
    Console.WriteLine("  edgar-cli filings MSFT 10-K");
    Console.WriteLine("  edgar-cli facts 320193");
    Console.WriteLine("  edgar-cli concept 320193 Revenues");
    Console.WriteLine("  edgar-cli latest 10-K");
    return;
}

var command = args[0].ToLower();

try
{
    switch (command)
    {
        case "company":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Please provide a ticker or CIK");
                return;
            }
            await GetCompanyInfo(companyLookupService, args[1]);
            break;

        case "filings":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Please provide a ticker or CIK");
                return;
            }
            var formType = args.Length > 2 ? args[2] : null;
            await GetFilings(edgarSearchService, args[1], formType);
            break;

        case "filing-details":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Please provide a filing documents URL");
                Console.WriteLine("Usage: filing-details <documents-url>");
                return;
            }
            await GetFilingDetails(filingDetailsService, args[1]);
            break;

        case "facts":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Please provide a CIK");
                Console.WriteLine("Usage: facts <cik>");
                Console.WriteLine("Example: facts 320193");
                return;
            }
            if (!int.TryParse(args[1], out var factsCik))
            {
                Console.WriteLine("Error: CIK must be a number");
                return;
            }
            await GetCompanyFacts(companyFactsService, factsCik);
            break;

        case "concept":
            if (args.Length < 3)
            {
                Console.WriteLine("Error: Please provide CIK and concept tag");
                Console.WriteLine("Usage: concept <cik> <tag>");
                Console.WriteLine("Example: concept 320193 Revenues");
                return;
            }
            if (!int.TryParse(args[1], out var conceptCik))
            {
                Console.WriteLine("Error: CIK must be a number");
                return;
            }
            await GetCompanyConcept(companyConceptService, conceptCik, args[2]);
            break;

        case "latest":
            var latestFormType = args.Length > 1 ? args[1] : null;
            await GetLatestFilings(latestFilingsService, latestFormType);
            break;

        default:
            Console.WriteLine($"Unknown command: {command}");
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

static async Task GetCompanyInfo(ICompanyLookupService service, string tickerOrCik)
{
    Console.WriteLine($"Looking up company: {tickerOrCik}...");

    var cik = await service.GetCikFromSymbolAsync(tickerOrCik);

    if (string.IsNullOrEmpty(cik))
    {
        Console.WriteLine("Company not found");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"Symbol/Ticker: {tickerOrCik}");
    Console.WriteLine($"CIK: {cik}");
}

static async Task GetFilings(IEdgarSearchService service, string tickerOrCik, string? formType)
{
    var typeDesc = formType ?? "all types";
    Console.WriteLine($"Fetching {typeDesc} filings for {tickerOrCik}...");

    var query = new EdgarSearchQuery
    {
        Symbol = tickerOrCik,
        FilingType = formType,
        ResultsPerPage = EdgarSearchResultsPerPage.Entries40
    };

    var response = await service.SearchAsync(query);

    if (response.Results == null || response.Results.Length == 0)
    {
        Console.WriteLine("No filings found");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"Found {response.Results.Length} filing(s):");
    Console.WriteLine();

    foreach (var filing in response.Results.Take(20))
    {
        Console.WriteLine($"Form: {filing.Filing}");
        Console.WriteLine($"  Filed: {filing.FilingDate:yyyy-MM-dd}");
        Console.WriteLine($"  Description: {filing.Description}");
        if (!string.IsNullOrEmpty(filing.DocumentsUrl))
            Console.WriteLine($"  URL: {filing.DocumentsUrl}");
        Console.WriteLine();
    }
}

static async Task GetFilingDetails(IFilingDetailsService service, string documentsUrl)
{
    Console.WriteLine($"Fetching filing details from {documentsUrl}...");

    var details = await service.GetFilingDetailsAsync(documentsUrl);

    if (details == null)
    {
        Console.WriteLine("Filing details not found");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("Filing Details:");
    Console.WriteLine($"  Entity: {details.EntityName} (CIK: {details.EntityCik})");
    Console.WriteLine($"  Form Type: {details.Form}");
    Console.WriteLine($"  Accession: {details.AccessionNumberP1}-{details.AccessionNumberP2}-{details.AccessionNumberP3}");
    Console.WriteLine($"  Filing Date: {details.FilingDate:yyyy-MM-dd}");
    Console.WriteLine($"  Period of Report: {details.PeriodOfReport:yyyy-MM-dd}");
    Console.WriteLine($"  Accepted: {details.Accepted:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine();

    if (details.DocumentFormatFiles?.Length > 0)
    {
        Console.WriteLine($"Document Format Files ({details.DocumentFormatFiles.Length}):");
        foreach (var doc in details.DocumentFormatFiles.Take(10))
        {
            Console.WriteLine($"  [{doc.Sequence}] {doc.Description}");
            Console.WriteLine($"      File: {doc.DocumentName} ({doc.DocumentType}, {doc.Size:N0} bytes)");
        }
        Console.WriteLine();
    }

    if (details.DataFiles?.Length > 0)
    {
        Console.WriteLine($"Data Files ({details.DataFiles.Length}):");
        foreach (var file in details.DataFiles.Take(10))
        {
            Console.WriteLine($"  [{file.Sequence}] {file.Description}");
            Console.WriteLine($"      File: {file.DocumentName} ({file.DocumentType})");
        }
    }
}

static async Task GetCompanyFacts(ICompanyFactsService service, int cik)
{
    Console.WriteLine($"Fetching all facts for CIK {cik}...");

    var facts = await service.QueryAsync(cik);

    if (facts == null)
    {
        Console.WriteLine("No facts found");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"Entity: {facts.EntityName} (CIK: {facts.CIK})");
    Console.WriteLine($"Total Facts: {facts.Facts?.Length ?? 0}");
    Console.WriteLine();

    if (facts.Facts?.Length > 0)
    {
        Console.WriteLine("Sample Facts (first 20):");
        foreach (var fact in facts.Facts.Take(20))
        {
            var dataPointCount = fact.DataPoints?.Length ?? 0;
            Console.WriteLine($"  - {fact.Label ?? "N/A"}");
            Console.WriteLine($"    Tag: {fact.Tag}, Data Points: {dataPointCount}");

            if (dataPointCount > 0)
            {
                var recent = fact.DataPoints!.Where(dp => dp.Value != 0)
                    .OrderByDescending(dp => dp.Filed)
                    .FirstOrDefault();

                if (recent != null)
                {
                    Console.WriteLine($"    Latest: ${recent.Value:N0} (Period: {recent.End:yyyy-MM-dd})");
                }
            }
        }
    }
}

static async Task GetCompanyConcept(ICompanyConceptService service, int cik, string tag)
{
    Console.WriteLine($"Fetching {tag} for CIK {cik}...");

    var concept = await service.QueryAsync(cik, tag);

    if (concept?.Result == null)
    {
        Console.WriteLine($"Concept '{tag}' not found");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"Entity: {concept.EntityName} (CIK: {concept.CIK})");
    Console.WriteLine($"Concept: {concept.Result.Label} ({concept.Result.Tag})");
    Console.WriteLine($"Description: {concept.Result.Description}");
    Console.WriteLine($"Total Data Points: {concept.Result.DataPoints?.Length ?? 0}");
    Console.WriteLine();

    if (concept.Result.DataPoints?.Length > 0)
    {
        var recentPoints = concept.Result.DataPoints
            .Where(dp => dp.End != default && dp.Value != 0)
            .OrderByDescending(dp => dp.Filed)
            .Take(10);

        Console.WriteLine("Recent Filings:");
        foreach (var dp in recentPoints)
        {
            Console.WriteLine($"  Period: {dp.End:yyyy-MM-dd}");
            Console.WriteLine($"    Value: ${dp.Value:N0}");
            Console.WriteLine($"    Filed: {dp.Filed:yyyy-MM-dd}");
            Console.WriteLine($"    Form: {dp.FromForm}");
        }
    }
}

static async Task GetLatestFilings(IEdgarLatestFilingsService service, string? formType)
{
    var typeDesc = formType ?? "all forms";
    Console.WriteLine($"Fetching latest filings ({typeDesc})...");

    var query = new EdgarLatestFilingsQuery
    {
        FormType = formType,
        ResultsPerPage = EdgarSearchResultsPerPage.Entries40,
        OwnershipFilter = EdgarSearchOwnershipFilter.Exclude
    };

    var results = await service.SearchAsync(query);

    if (results == null || results.Length == 0)
    {
        Console.WriteLine("No filings found");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"Latest {results.Length} filing(s):");
    Console.WriteLine();

    foreach (var filing in results.Take(20))
    {
        Console.WriteLine($"Company: {filing.EntityTitle}");
        Console.WriteLine($"  CIK: {filing.EntityCik}");
        Console.WriteLine($"  Form: {filing.Filing}");
        Console.WriteLine($"  Filed: {filing.FilingDate:yyyy-MM-dd}");
        if (!string.IsNullOrEmpty(filing.Description))
            Console.WriteLine($"  Description: {filing.Description}");
        Console.WriteLine();
    }
}
