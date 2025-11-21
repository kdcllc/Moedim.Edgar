using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Moedim.Edgar.Services;

namespace Moedim.Edgar.Mcp.Tools;

/// <summary>
/// MCP tools for querying company financial data from SEC EDGAR.
/// Provides access to company facts and specific financial concepts.
/// </summary>
internal class CompanyDataTools
{
    private readonly ICompanyFactsService _companyFactsService;
    private readonly ICompanyConceptService _companyConceptService;
    private readonly ICompanyLookupService _companyLookupService;
    private readonly ILogger<CompanyDataTools> _logger;

    public CompanyDataTools(
        ICompanyFactsService companyFactsService,
        ICompanyConceptService companyConceptService,
        ICompanyLookupService companyLookupService,
        ILogger<CompanyDataTools> logger)
    {
        _companyFactsService = companyFactsService;
        _companyConceptService = companyConceptService;
        _companyLookupService = companyLookupService;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Looks up a company's CIK (Central Index Key) from its trading symbol. The CIK is required for most other EDGAR operations.")]
    public async Task<string> GetCikFromSymbol(
        [Description("The stock trading symbol (e.g., 'AAPL', 'MSFT', 'TSLA')")] string symbol,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Looking up CIK for symbol: {Symbol}", symbol);
            var cik = await _companyLookupService.GetCikFromSymbolAsync(symbol, cancellationToken);
            return $"CIK for {symbol}: {cik}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error looking up CIK for symbol: {Symbol}", symbol);
            return $"Error: Failed to lookup CIK for {symbol}. {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Retrieves all financial facts for a company from SEC EDGAR. Returns comprehensive financial data including US-GAAP concepts, custom company facts, and historical data points.")]
    public async Task<string> GetCompanyFacts(
        [Description("The Central Index Key (CIK) of the company. Can be obtained using GetCikFromSymbol.")] int cik,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Querying company facts for CIK: {CIK}", cik);
            var facts = await _companyFactsService.QueryAsync(cik, cancellationToken);

            var summary = new
            {
                CompanyName = facts.EntityName,
                CIK = facts.CIK,
                FactCount = facts.Facts?.Length ?? 0,
                Facts = facts.Facts?.Take(10).Select(f => new
                {
                    f.Tag,
                    f.Label,
                    f.Description,
                    DataPointCount = f.DataPoints?.Length ?? 0
                }).ToList()
            };

            return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying company facts for CIK: {CIK}", cik);
            return $"Error: Failed to retrieve company facts for CIK {cik}. {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Retrieves a specific financial concept (like Revenues, Assets, NetIncome) for a company. Returns historical data points for the requested XBRL tag.")]
    public async Task<string> GetCompanyConcept(
        [Description("The Central Index Key (CIK) of the company")] int cik,
        [Description("The XBRL tag name (e.g., 'Revenues', 'Assets', 'NetIncomeLoss', 'EarningsPerShareBasic')")] string tag,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Querying concept {Tag} for CIK: {CIK}", tag, cik);
            var concept = await _companyConceptService.QueryAsync(cik, tag, cancellationToken);

            var summary = new
            {
                CompanyName = concept.EntityName,
                CIK = concept.CIK,
                Tag = concept.Result?.Tag,
                Label = concept.Result?.Label,
                Description = concept.Result?.Description,
                DataPointCount = concept.Result?.DataPoints?.Length ?? 0,
                RecentDataPoints = concept.Result?.DataPoints?.OrderByDescending(dp => dp.Filed)
                    .Take(5)
                    .Select(dp => new
                    {
                        dp.Value,
                        dp.FiscalYear,
                        Period = dp.Period.ToString(),
                        dp.Filed,
                        Form = dp.FromForm
                    }).ToList()
            };

            return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying concept {Tag} for CIK: {CIK}", tag, cik);
            return $"Error: Failed to retrieve concept '{tag}' for CIK {cik}. {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Lists common XBRL financial concepts that can be queried. Useful for discovering available financial metrics.")]
    public string ListCommonConcepts()
    {
        var concepts = new
        {
            IncomeStatement = new[]
            {
                "Revenues",
                "CostOfRevenue",
                "GrossProfit",
                "OperatingIncomeLoss",
                "NetIncomeLoss",
                "EarningsPerShareBasic",
                "EarningsPerShareDiluted"
            },
            BalanceSheet = new[]
            {
                "Assets",
                "AssetsCurrent",
                "Liabilities",
                "LiabilitiesCurrent",
                "StockholdersEquity",
                "Cash",
                "AccountsReceivable",
                "Inventory",
                "PropertyPlantAndEquipmentNet"
            },
            CashFlow = new[]
            {
                "NetCashProvidedByUsedInOperatingActivities",
                "NetCashProvidedByUsedInInvestingActivities",
                "NetCashProvidedByUsedInFinancingActivities",
                "CashAndCashEquivalentsAtCarryingValue"
            }
        };

        return JsonSerializer.Serialize(concepts, new JsonSerializerOptions { WriteIndented = true });
    }
}
