using Microsoft.Extensions.Logging;
using Moedim.Edgar.Client;
using Moedim.Edgar.Models.Fillings;
using Moedim.Edgar.Services.Processing;
using ReverseMarkdown;
using System.Text.RegularExpressions;

namespace Moedim.Edgar.Services.Impl;

/// <summary>
/// Service implementation for retrieving SEC filing details
/// </summary>
public class FilingDetailsService : IFilingDetailsService
{
    private readonly ISecEdgarClient _client;
    private readonly ICacheService _cache;
    private readonly ILogger<FilingDetailsService>? _logger;
    private readonly Converter _markdownConverter;

    /// <summary>
    /// Initializes a new instance of the FilingDetailsService
    /// </summary>
    /// <param name="client">The SEC EDGAR HTTP client</param>
    /// <param name="cache">The cache service</param>
    /// <param name="logger">Optional logger instance</param>
    public FilingDetailsService(
        ISecEdgarClient client,
        ICacheService cache,
        ILogger<FilingDetailsService>? logger = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger;

        // Configure markdown converter
        _markdownConverter = new Converter(new Config
        {
            GithubFlavored = true,
            RemoveComments = true,
            SmartHrefHandling = true
        });
    }

    /// <inheritdoc/>
    public async Task<EdgarFilingDetails> GetFilingDetailsAsync(string documentsUrl, CancellationToken cancellationToken = default)
    {
        ValidateDocumentsUrl(documentsUrl);

        _logger?.LogDebug("Getting filing details from: {Url}", documentsUrl);

        var html = await _client.GetAsync(documentsUrl, cancellationToken);
        return ParseFilingDetails(html);
    }

    /// <inheritdoc/>
    public async Task<long> GetCikFromFilingAsync(string documentsUrl, CancellationToken cancellationToken = default)
    {
        ValidateDocumentsUrl(documentsUrl);

        var html = await _client.GetAsync(documentsUrl, cancellationToken);

        var cikLabel = "<acronym title=\"Central Index Key\">CIK</acronym>";
        var labelIndex = html.IndexOf(cikLabel);

        if (labelIndex == -1)
        {
            throw new InvalidOperationException("Unable to find CIK label in filing.");
        }

        try
        {
            var hrefIndex = html.IndexOf("a href", labelIndex);
            var textStart = html.IndexOf(">", hrefIndex) + 1;
            var textEnd = html.IndexOf(" ", textStart);
            var cikStr = html.Substring(textStart, textEnd - textStart).Trim();
            return long.Parse(cikStr);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Fatal error while trying to find CIK in filing at {documentsUrl}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<FilingDocument[]> GetDocumentFormatFilesAsync(string documentsUrl, CancellationToken cancellationToken = default)
    {
        ValidateDocumentsUrl(documentsUrl);

        var html = await _client.GetAsync(documentsUrl, cancellationToken);

        var sectionStart = html.IndexOf("Document Format Files");
        var sectionEnd = html.IndexOf("</table>", sectionStart + 1);

        if (sectionStart == -1 || sectionEnd == -1)
        {
            throw new InvalidOperationException("Unable to locate document format files.");
        }

        var tableHtml = html.Substring(sectionStart, sectionEnd - sectionStart);
        return ParseDocumentsTable(tableHtml);
    }

    /// <inheritdoc/>
    public async Task<FilingDocument[]> GetDataFilesAsync(string documentsUrl, CancellationToken cancellationToken = default)
    {
        ValidateDocumentsUrl(documentsUrl);

        var html = await _client.GetAsync(documentsUrl, cancellationToken);

        var sectionStart = html.IndexOf("Data Files");
        var sectionEnd = html.IndexOf("</table>", sectionStart + 1);

        if (sectionStart == -1 || sectionEnd == -1)
        {
            throw new InvalidOperationException("Unable to locate data files.");
        }

        var tableHtml = html.Substring(sectionStart, sectionEnd - sectionStart);
        return ParseDocumentsTable(tableHtml);
    }

    /// <inheritdoc/>
    public async Task<Stream> DownloadDocumentAsync(string documentUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentUrl))
        {
            throw new ArgumentException("Document URL is required", nameof(documentUrl));
        }

        _logger?.LogDebug("Downloading document from: {Url}", documentUrl);

        return await _client.GetStreamAsync(documentUrl, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Stream> DownloadXbrlDocumentAsync(string documentsUrl, CancellationToken cancellationToken = default)
    {
        var dataFiles = await GetDataFilesAsync(documentsUrl, cancellationToken);

        // Try to find by description
        var instanceDoc = dataFiles.FirstOrDefault(f =>
            f.Description?.Trim().ToLower().Contains("instance document") == true);

        if (instanceDoc != null && !string.IsNullOrEmpty(instanceDoc.Url))
        {
            return await DownloadDocumentAsync(instanceDoc.Url, cancellationToken);
        }

        // Try to find by document type
        instanceDoc = dataFiles.FirstOrDefault(f =>
            f.DocumentType?.ToLower().Contains("ins") == true);

        if (instanceDoc != null && !string.IsNullOrEmpty(instanceDoc.Url))
        {
            return await DownloadDocumentAsync(instanceDoc.Url, cancellationToken);
        }

        throw new InvalidOperationException("Unable to find XBRL Instance Document in this filing.");
    }

    private EdgarFilingDetails ParseFilingDetails(string html)
    {
        var details = new EdgarFilingDetails();

        // Parse accession number
        try
        {
            var secNumIndex = html.IndexOf("<div id=\"secNum\">");
            if (secNumIndex != -1)
            {
                var strongEnd = html.IndexOf("</strong>", secNumIndex);
                var textStart = html.IndexOf(">", strongEnd) + 1;
                var textEnd = html.IndexOf("<", textStart);
                var accessionNumber = html.Substring(textStart, textEnd - textStart).Trim();

                var parts = accessionNumber.Split('-');
                if (parts.Length == 3)
                {
                    details.AccessionNumberP1 = long.Parse(parts[0]);
                    details.AccessionNumberP2 = int.Parse(parts[1]);
                    details.AccessionNumberP3 = int.Parse(parts[2]);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to parse accession number");
        }

        // Parse form type
        details.Form = ExtractInfoValue(html, "Type:");

        // Parse filing date
        var filingDateStr = ExtractInfoValue(html, "Filing Date");
        if (DateTime.TryParse(filingDateStr, out var filingDate))
        {
            details.FilingDate = filingDate;
        }

        // Parse period of report
        var periodStr = ExtractInfoValue(html, "Period of Report");
        if (DateTime.TryParse(periodStr, out var periodDate))
        {
            details.PeriodOfReport = periodDate;
        }

        // Parse accepted date/time
        var acceptedStr = ExtractInfoValue(html, "Accepted");
        if (DateTime.TryParse(acceptedStr, out var acceptedDate))
        {
            details.Accepted = acceptedDate;
        }

        // Parse entity name
        try
        {
            var nameIndex = html.IndexOf("companyName");
            if (nameIndex != -1)
            {
                var textStart = html.IndexOf(">", nameIndex) + 1;
                var textEnd = html.IndexOf("<", textStart);
                var name = html.Substring(textStart, textEnd - textStart);
                details.EntityName = name.Replace("(Filer)", "").Trim();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to parse entity name");
        }

        // Parse CIK
        try
        {
            var cikLabel = "<acronym title=\"Central Index Key\">CIK</acronym>";
            var labelIndex = html.IndexOf(cikLabel);
            if (labelIndex != -1)
            {
                var hrefIndex = html.IndexOf("a href", labelIndex);
                var textStart = html.IndexOf(">", hrefIndex) + 1;
                var textEnd = html.IndexOf(" ", textStart);
                var cikStr = html.Substring(textStart, textEnd - textStart).Trim();
                details.EntityCik = long.Parse(cikStr);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to parse CIK");
        }

        // Parse document format files
        try
        {
            var sectionStart = html.IndexOf("Document Format Files");
            var sectionEnd = html.IndexOf("</table>", sectionStart + 1);
            if (sectionStart != -1 && sectionEnd > sectionStart)
            {
                var tableHtml = html.Substring(sectionStart, sectionEnd - sectionStart);
                details.DocumentFormatFiles = ParseDocumentsTable(tableHtml);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to parse document format files");
        }

        // Parse data files
        try
        {
            var sectionStart = html.IndexOf("Data Files");
            var sectionEnd = html.IndexOf("</table>", sectionStart + 1);
            if (sectionStart != -1 && sectionEnd > sectionStart)
            {
                var tableHtml = html.Substring(sectionStart, sectionEnd - sectionStart);
                details.DataFiles = ParseDocumentsTable(tableHtml);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to parse data files");
        }

        return details;
    }

    private FilingDocument[] ParseDocumentsTable(string tableHtml)
    {
        var documents = new List<FilingDocument>();
        var rows = tableHtml.Split(new[] { "<tr" }, StringSplitOptions.None);

        // Skip header rows (first 2)
        for (int i = 2; i < rows.Length; i++)
        {
            var cols = rows[i].Split(new[] { "<td" }, StringSplitOptions.None);

            if (cols.Length > 5)
            {
                var doc = new FilingDocument();

                // Sequence (column 1)
                var seqStr = ExtractText(cols[1]);
                if (int.TryParse(seqStr, out var seq))
                {
                    doc.Sequence = seq;
                }

                // Description (column 2)
                doc.Description = ExtractText(cols[2])
                    .Replace("<b>", "")
                    .Replace("</b>", "");

                // URL and filename (column 3)
                var hrefIndex = cols[3].IndexOf("href");
                if (hrefIndex != -1)
                {
                    var urlStart = cols[3].IndexOf('"', hrefIndex) + 1;
                    var urlEnd = cols[3].IndexOf('"', urlStart);
                    if (urlStart > 0 && urlEnd > urlStart)
                    {
                        doc.Url = "https://www.sec.gov/" + cols[3].Substring(urlStart, urlEnd - urlStart);
                    }

                    var nameStart = cols[3].IndexOf('>', urlEnd) + 1;
                    var nameEnd = cols[3].IndexOf('<', nameStart);
                    if (nameStart > 0 && nameEnd > nameStart)
                    {
                        doc.DocumentName = cols[3].Substring(nameStart, nameEnd - nameStart);
                    }
                }

                // Document type (column 4)
                var docType = ExtractText(cols[4]);
                doc.DocumentType = docType == "&nbsp;" ? string.Empty : docType;

                // Size (column 5)
                var sizeStr = ExtractText(cols[5]);
                if (int.TryParse(sizeStr, out var size))
                {
                    doc.Size = size;
                }

                documents.Add(doc);
            }
        }

        return documents.ToArray();
    }

    private string ExtractText(string html)
    {
        var start = html.IndexOf('>');
        if (start == -1) return string.Empty;

        var end = html.IndexOf('<', start + 1);
        if (end == -1) end = html.Length;

        return html.Substring(start + 1, end - start - 1).Trim();
    }

    private string ExtractInfoValue(string html, string label)
    {
        try
        {
            var labelIndex = html.IndexOf(label);
            if (labelIndex == -1) return string.Empty;

            var strongIndex = html.IndexOf("<strong>", labelIndex);
            var textStart = html.IndexOf(">", strongIndex) + 1;
            var textEnd = html.IndexOf("<", textStart);

            if (textStart > 0 && textEnd > textStart)
            {
                return html.Substring(textStart, textEnd - textStart).Trim();
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return string.Empty;
    }

    private void ValidateDocumentsUrl(string documentsUrl)
    {
        if (string.IsNullOrWhiteSpace(documentsUrl))
        {
            throw new ArgumentException("Documents URL is required", nameof(documentsUrl));
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GetFilingDocumentAsync(
        string accessionNumber,
        string? tickerOrCik = null,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        format = format?.ToLowerInvariant() ?? "markdown";
        var cacheKey = $"{accessionNumber}_document_{format}";

        // Check cache first
        var cached = await _cache.GetAsync<string>(cacheKey);
        if (cached != null) return cached;

        try
        {
            // Clean accession number
            var cleanAccession = Regex.Replace(
                accessionNumber,
                @"(-index)?(\.htm)?$",
                "",
                RegexOptions.IgnoreCase);

            // Try to get document URL
            var documentUrl = await FindPrimaryDocumentUrlAsync(cleanAccession, tickerOrCik, cancellationToken);

            if (documentUrl == null)
            {
                _logger?.LogWarning("Could not find primary document for accession: {Accession}", cleanAccession);
                return null;
            }

            // Download the document
            var htmlContent = await _client.GetAsync(documentUrl, cancellationToken);

            // Convert to markdown if requested
            string result = format == "markdown"
                ? _markdownConverter.Convert(htmlContent)
                : htmlContent;

            // Cache the result
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(24));

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting filing document for accession: {Accession}", accessionNumber);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<FilingSectionsResult?> GetFilingSectionsAsync(
        string accessionNumber,
        FilingSectionsRequest request,
        string? tickerOrCik = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cleanAccession = Regex.Replace(
                accessionNumber,
                @"(-index)?(\.htm)?$",
                "",
                RegexOptions.IgnoreCase);

            // Get the filing document
            var filingDocument = await GetFilingDocumentAsync(cleanAccession, tickerOrCik, "html", cancellationToken);

            if (string.IsNullOrEmpty(filingDocument))
            {
                return null;
            }

            // Check if we have cached sections
            var sectionCacheKey = $"{cleanAccession}_sections";
            var slices = await _cache.GetAsync<List<HtmlSlice>>(sectionCacheKey);

            if (slices == null)
            {
                slices = await FilingProcessor.ProcessFilingContentAsync(filingDocument);
                await _cache.SetAsync(sectionCacheKey, slices, TimeSpan.FromHours(24));
            }

            var result = new FilingSectionsResult();

            // For preview, return only metadata
            if (request.PreviewOnly)
            {
                result.Preview = slices.Select(s => new SectionPreview
                {
                    Label = s.Label,
                    AnchorTargetId = s.AnchorTargetId,
                    Snippet = s.Content.Length > 200
                        ? s.Content.Substring(0, 200) + "..."
                        : s.Content
                }).ToList();

                return result;
            }

            // Filter by anchor_ids if provided
            var filteredSlices = slices;
            if (request.AnchorIds != null && request.AnchorIds.Count > 0)
            {
                filteredSlices = slices
                    .Where(s => request.AnchorIds.Contains(s.AnchorTargetId, StringComparer.OrdinalIgnoreCase))
                    .ToList();
            }

            // Convert to markdown by default
            var format = request.Format?.ToLowerInvariant() ?? "markdown";
            if (format == "markdown")
            {
                filteredSlices = FilingProcessor.ConvertToMarkdown(filteredSlices);
            }

            // Return sections
            result.Sections = filteredSlices;

            // Merge if requested
            if (request.Merge)
            {
                result.MergedContent = string.Join("\n\n",
                    filteredSlices.Where(s => !string.IsNullOrWhiteSpace(s.Content))
                                  .Select(s => s.Content));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting filing sections for accession: {Accession}", accessionNumber);
            return null;
        }
    }

    private async Task<string?> FindPrimaryDocumentUrlAsync(
        string cleanAccession,
        string? tickerOrCik,
        CancellationToken cancellationToken)
    {
        try
        {
            // Build the documents URL
            var parts = cleanAccession.Split('-');
            if (parts.Length != 3) return null;

            var accessionNoHyphens = cleanAccession.Replace("-", "");

            // We need the CIK - try to extract from accession or use provided ticker/CIK
            string? cik = null;

            if (!string.IsNullOrEmpty(tickerOrCik))
            {
                // If it's numeric, assume it's CIK
                if (long.TryParse(tickerOrCik, out var parsedCik))
                {
                    cik = parsedCik.ToString("D10");
                }
                else
                {
                    // It's a ticker, we'd need to look it up
                    // For now, just use the filer CIK from accession
                    cik = parts[0];
                }
            }
            else
            {
                cik = parts[0];
            }

            // Try to get the filing details to find primary document
            var documentsUrl = $"https://www.sec.gov/cgi-bin/viewer?action=view&cik={cik}&accession_number={accessionNoHyphens}";

            try
            {
                var details = await GetFilingDetailsAsync(documentsUrl, cancellationToken);

                // Find the primary document (usually sequence 1)
                var primaryDoc = details.DocumentFormatFiles?
                    .OrderBy(d => d.Sequence)
                    .FirstOrDefault(d => d.DocumentType?.Contains("10-") == true ||
                                        d.DocumentType?.Contains("8-") == true ||
                                        d.Sequence == 1);

                return primaryDoc?.Url;
            }
            catch
            {
                // If that fails, try constructing URL directly
                return $"https://www.sec.gov/Archives/edgar/data/{cik}/{accessionNoHyphens}/{cleanAccession}.htm";
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error finding primary document URL for: {Accession}", cleanAccession);
            return null;
        }
    }
}
