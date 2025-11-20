using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Moedim.Edgar.Services;

namespace Moedim.Edgar.Mcp.Tools;

/// <summary>
/// MCP tools for retrieving detailed filing information and documents.
/// Provides access to filing metadata, document lists, and document downloads.
/// </summary>
internal class FilingDetailsTools
{
    private readonly IFilingDetailsService _filingDetailsService;
    private readonly ILogger<FilingDetailsTools> _logger;

    public FilingDetailsTools(
        IFilingDetailsService filingDetailsService,
        ILogger<FilingDetailsTools> logger)
    {
        _filingDetailsService = filingDetailsService;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Retrieves detailed information about a specific SEC filing including metadata and available documents.")]
    public async Task<string> GetFilingDetails(
        [Description("The documents URL for the filing (obtained from search results)")] string documentsUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching filing details from: {Url}", documentsUrl);

            var details = await _filingDetailsService.GetFilingDetailsAsync(documentsUrl, cancellationToken);

            var summary = new
            {
                EntityName = details.EntityName,
                CIK = details.EntityCik,
                FormType = details.Form,
                FilingDate = details.FilingDate,
                PeriodOfReport = details.PeriodOfReport,
                Accepted = details.Accepted,
                AccessionNumber = $"{details.AccessionNumberP1}-{details.AccessionNumberP2}-{details.AccessionNumberP3}",
                DocumentFormatFiles = details.DocumentFormatFiles?.Select(d => new
                {
                    Sequence = d.Sequence,
                    d.Description,
                    d.DocumentType,
                    DocumentName = d.DocumentName,
                    Url = d.Url,
                    SizeKB = d.Size / 1024
                }).ToList(),
                DataFiles = details.DataFiles?.Select(d => new
                {
                    Sequence = d.Sequence,
                    d.Description,
                    d.DocumentType,
                    DocumentName = d.DocumentName,
                    Url = d.Url,
                    SizeKB = d.Size / 1024
                }).ToList()
            };

            return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching filing details from: {Url}", documentsUrl);
            return $"Error: Failed to retrieve filing details. {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Retrieves the CIK (Central Index Key) from a filing documents URL.")]
    public async Task<string> GetCikFromFiling(
        [Description("The documents URL for the filing")] string documentsUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Extracting CIK from filing URL: {Url}", documentsUrl);

            var cik = await _filingDetailsService.GetCikFromFilingAsync(documentsUrl, cancellationToken);

            return $"CIK: {cik}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting CIK from: {Url}", documentsUrl);
            return $"Error: Failed to extract CIK from filing URL. {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Lists all document format files (HTML, PDF, etc.) available in a filing.")]
    public async Task<string> GetDocumentFormatFiles(
        [Description("The documents URL for the filing")] string documentsUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching document format files from: {Url}", documentsUrl);

            var files = await _filingDetailsService.GetDocumentFormatFilesAsync(documentsUrl, cancellationToken);

            var summary = new
            {
                TotalFiles = files?.Length ?? 0,
                Files = files?.Select(f => new
                {
                    Sequence = f.Sequence,
                    f.Description,
                    f.DocumentType,
                    DocumentName = f.DocumentName,
                    Url = f.Url,
                    SizeKB = f.Size / 1024
                }).ToList()
            };

            return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching document format files from: {Url}", documentsUrl);
            return $"Error: Failed to retrieve document format files. {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Lists all data files (XML, XBRL, JSON, etc.) available in a filing.")]
    public async Task<string> GetDataFiles(
        [Description("The documents URL for the filing")] string documentsUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching data files from: {Url}", documentsUrl);

            var files = await _filingDetailsService.GetDataFilesAsync(documentsUrl, cancellationToken);

            var summary = new
            {
                TotalFiles = files?.Length ?? 0,
                Files = files?.Select(f => new
                {
                    Sequence = f.Sequence,
                    f.Description,
                    f.DocumentType,
                    DocumentName = f.DocumentName,
                    Url = f.Url,
                    SizeKB = f.Size / 1024
                }).ToList()
            };

            return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data files from: {Url}", documentsUrl);
            return $"Error: Failed to retrieve data files. {ex.Message}";
        }
    }
}
