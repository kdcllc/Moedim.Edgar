using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Moedim.Edgar.Models.Fillings;
using Moedim.Edgar.Services;

namespace Moedim.Edgar.Mcp.Tools;

/// <summary>
/// MCP tools for searching SEC EDGAR filings.
/// Provides access to latest filings and company-specific filing searches.
/// </summary>
internal class FilingSearchTools
{
    private readonly IEdgarSearchService _searchService;
    private readonly IEdgarLatestFilingsService _latestFilingsService;
    private readonly ILogger<FilingSearchTools> _logger;

    public FilingSearchTools(
        IEdgarSearchService searchService,
        IEdgarLatestFilingsService latestFilingsService,
        ILogger<FilingSearchTools> logger)
    {
        _searchService = searchService;
        _latestFilingsService = latestFilingsService;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Searches for SEC filings by company symbol or CIK. Returns paginated results with filing metadata including dates, forms, and document URLs.")]
    public async Task<string> SearchCompanyFilings(
        [Description("Company stock symbol (e.g., 'AAPL') or CIK number")] string symbol,
        [Description("Optional: Filter by form type (e.g., '10-K', '10-Q', '8-K', '13F')")] string? formType = null,
        [Description("Optional: Maximum number of results to return (default: 10)")] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching filings for: {Symbol}, Form: {Form}", symbol, formType);

            var query = new EdgarSearchQuery
            {
                Symbol = symbol,
                FilingType = formType
            };

            var response = await _searchService.SearchAsync(query, cancellationToken);

            var summary = new
            {
                TotalResults = response.Results?.Length ?? 0,
                HasMore = response.HasNextPage,
                NextPageUrl = response.NextPageUrl,
                Filings = response.Results?.Take(maxResults).Select(result => new
                {
                    FormType = result.Filing,
                    FilingDate = result.FilingDate,
                    Description = result.Description,
                    DocumentsUrl = result.DocumentsUrl,
                    InteractiveDataUrl = result.InteractiveDataUrl
                }).ToList()
            };

            return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching filings for: {Symbol}", symbol);
            return $"Error: Failed to search filings for {symbol}. {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Retrieves the latest SEC filings across all companies. Useful for monitoring recent regulatory activity.")]
    public async Task<string> GetLatestFilings(
        [Description("Optional: Filter by form type (e.g., '10-K', '10-Q', '8-K')")] string? formType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching latest filings, Form: {Form}", formType);

            var query = new EdgarLatestFilingsQuery
            {
                FormType = formType
            };

            var results = await _latestFilingsService.SearchAsync(query, cancellationToken);

            var summary = new
            {
                ResultCount = results?.Length ?? 0,
                Filings = results?.Select(r => new
                {
                    CompanyName = r.EntityTitle,
                    CIK = r.EntityCik,
                    FormType = r.Filing,
                    FilingDate = r.FilingDate,
                    Description = r.Description,
                    DocumentsUrl = r.DocumentsUrl
                }).ToList()
            };

            return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest filings");
            return $"Error: Failed to retrieve latest filings. {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Gets the next page of filing search results using the URL from a previous search.")]
    public async Task<string> GetNextFilingsPage(
        [Description("The next page URL returned from a previous search")] string nextPageUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching next page of results");

            var response = await _searchService.NextPageAsync(nextPageUrl, cancellationToken);

            var summary = new
            {
                TotalResults = response.Results?.Length ?? 0,
                HasMore = response.HasNextPage,
                NextPageUrl = response.NextPageUrl,
                Filings = response.Results?.Select(result => new
                {
                    FormType = result.Filing,
                    FilingDate = result.FilingDate,
                    Description = result.Description,
                    DocumentsUrl = result.DocumentsUrl
                }).ToList()
            };

            return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching next page");
            return $"Error: Failed to retrieve next page of results. {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Lists common SEC form types and their purposes. Useful for understanding what type of information each filing contains.")]
    public string ListCommonFormTypes()
    {
        var forms = new
        {
            AnnualReports = new[]
            {
                new { Form = "10-K", Description = "Annual report with comprehensive financial information" },
                new { Form = "20-F", Description = "Annual report for foreign private issuers" }
            },
            QuarterlyReports = new[]
            {
                new { Form = "10-Q", Description = "Quarterly report with unaudited financial statements" },
                new { Form = "6-K", Description = "Report of foreign private issuer" }
            },
            CurrentReports = new[]
            {
                new { Form = "8-K", Description = "Current report for material events or corporate changes" }
            },
            Ownership = new[]
            {
                new { Form = "13F", Description = "Quarterly holdings report for institutional investment managers" },
                new { Form = "13D", Description = "Beneficial ownership report (>5% stake)" },
                new { Form = "13G", Description = "Beneficial ownership report (passive investors)" },
                new { Form = "4", Description = "Statement of changes in beneficial ownership" }
            },
            ProxyStatements = new[]
            {
                new { Form = "DEF 14A", Description = "Definitive proxy statement" },
                new { Form = "DEFA14A", Description = "Additional proxy soliciting materials" }
            },
            Registration = new[]
            {
                new { Form = "S-1", Description = "Registration statement for securities" },
                new { Form = "S-3", Description = "Simplified registration form" },
                new { Form = "S-4", Description = "Registration for business combinations" }
            }
        };

        return JsonSerializer.Serialize(forms, new JsonSerializerOptions { WriteIndented = true });
    }
}
