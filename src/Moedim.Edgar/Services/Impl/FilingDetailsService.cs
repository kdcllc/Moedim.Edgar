using Microsoft.Extensions.Logging;
using Moedim.Edgar.Client;
using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.Services.Impl;

/// <summary>
/// Service implementation for retrieving SEC filing details
/// </summary>
public class FilingDetailsService(ISecEdgarClient client, ILogger<FilingDetailsService>? logger = null) : IFilingDetailsService
{
    private readonly ISecEdgarClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly ILogger<FilingDetailsService>? _logger = logger;

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
}
