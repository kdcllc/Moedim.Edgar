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

    [McpServerTool]
    [Description("Downloads the full content of a SEC filing document. Returns clean markdown by default for LLM processing, or HTML if format='html'. Note: Filings can be very large (10-50MB). Consider using PreviewFilingSections for targeted analysis.")]
    public async Task<string> GetFilingDocument(
        [Description("The filing accession number (format: 0000000000-00-000000)")] string accessionNumber,
        [Description("Optional: Company ticker symbol or CIK to help locate the filing")] string? tickerOrCik = null,
        [Description("Output format: 'markdown' (default, clean text) or 'html' (raw HTML)")] string? format = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching filing document: {Accession}, Format: {Format}", accessionNumber, format ?? "markdown");

            var content = await _filingDetailsService.GetFilingDocumentAsync(accessionNumber, tickerOrCik, format, cancellationToken);

            if (content == null)
            {
                return $"Error: Filing {accessionNumber} not found";
            }

            // Truncate if too large (over 100KB)
            if (content.Length > 100000)
            {
                return content.Substring(0, 100000) + $"\n\n[Content truncated - total length: {content.Length:N0} characters. Use PreviewFilingSections and GetFilingSections for targeted extraction.]";
            }

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching filing document: {Accession}", accessionNumber);
            return $"Error: Failed to retrieve filing document. {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Preview available sections in a SEC filing WITHOUT full content. Returns section IDs, titles, and brief snippets. Use this FIRST to discover sections, then use GetFilingSections with the IDs to get actual content.")]
    public async Task<string> PreviewFilingSections(
        [Description("The filing accession number")] string accessionNumber,
        [Description("Optional: Company ticker symbol or CIK")] string? tickerOrCik = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Previewing filing sections: {Accession}", accessionNumber);

            var request = new Moedim.Edgar.Models.Fillings.FilingSectionsRequest
            {
                PreviewOnly = true
            };

            var result = await _filingDetailsService.GetFilingSectionsAsync(accessionNumber, request, tickerOrCik, cancellationToken);

            if (result?.Preview == null || result.Preview.Count == 0)
            {
                return "No sections found in this filing.";
            }

            var summary = new
            {
                TotalSections = result.Preview.Count,
                Sections = result.Preview.Select(s => new
                {
                    AnchorId = s.AnchorTargetId,
                    Title = s.Label,
                    Preview = s.Snippet
                }).ToList()
            };

            return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing filing sections: {Accession}", accessionNumber);
            return $"Error: Failed to preview sections. {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Retrieve FULL CONTENT of specific filing sections by anchor IDs as clean markdown by default. You must call PreviewFilingSections first to get the anchor IDs. Pass the IDs you want to extract.")]
    public async Task<string> GetFilingSections(
        [Description("The filing accession number")] string accessionNumber,
        [Description("List of section anchor IDs to retrieve (from PreviewFilingSections)")] string[] anchorIds,
        [Description("Optional: Company ticker symbol or CIK")] string? tickerOrCik = null,
        [Description("If true, merge all sections into single content block")] bool merge = false,
        [Description("Output format: 'markdown' (default) or 'html'")] string? format = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching filing sections: {Accession}, Sections: {Count}", accessionNumber, anchorIds?.Length ?? 0);

            var request = new Moedim.Edgar.Models.Fillings.FilingSectionsRequest
            {
                PreviewOnly = false,
                AnchorIds = anchorIds,
                Merge = merge,
                Format = format
            };

            var result = await _filingDetailsService.GetFilingSectionsAsync(accessionNumber, request, tickerOrCik, cancellationToken);

            if (result == null)
            {
                return "Error: Filing not found";
            }

            if (merge && !string.IsNullOrEmpty(result.MergedContent))
            {
                return result.MergedContent;
            }

            if (result.Sections != null && result.Sections.Count > 0)
            {
                var sections = result.Sections.Select(s => new
                {
                    AnchorId = s.AnchorTargetId,
                    Title = s.Label,
                    Content = s.Content
                }).ToList();

                return JsonSerializer.Serialize(sections, new JsonSerializerOptions { WriteIndented = true });
            }

            return "No sections found matching the provided anchor IDs.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching filing sections: {Accession}", accessionNumber);
            return $"Error: Failed to retrieve sections. {ex.Message}";
        }
    }
}
