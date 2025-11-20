using Microsoft.Extensions.Logging;
using Moedim.Edgar.Client;
using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.Services.Impl;

/// <summary>
/// Service implementation for searching latest SEC Edgar filings
/// </summary>
public class EdgarLatestFilingsService(ISecEdgarClient client, ILogger<EdgarLatestFilingsService>? logger = null) : IEdgarLatestFilingsService
{
    private readonly ISecEdgarClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly ILogger<EdgarLatestFilingsService>? _logger = logger;

    /// <inheritdoc/>
    public async Task<EdgarLatestFilingResult[]> SearchAsync(EdgarLatestFilingsQuery query, CancellationToken cancellationToken = default)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));

        var url = BuildSearchUrl(query);
        _logger?.LogDebug("Searching latest Edgar filings: {Url}", url);

        var html = await _client.GetAsync(url, cancellationToken);
        return ParseLatestFilings(html);
    }

    private string BuildSearchUrl(EdgarLatestFilingsQuery query)
    {
        var url = "https://www.sec.gov/cgi-bin/browse-edgar?";

        if (!string.IsNullOrWhiteSpace(query.FormType))
        {
            url += $"&type={query.FormType}";
        }

        url += query.OwnershipFilter switch
        {
            EdgarSearchOwnershipFilter.Exclude => "&owner=exclude",
            EdgarSearchOwnershipFilter.Include => "&owner=include",
            EdgarSearchOwnershipFilter.Only => "&owner=only",
            _ => "&owner=include"
        };

        var count = query.ResultsPerPage switch
        {
            EdgarSearchResultsPerPage.Entries10 => 10,
            EdgarSearchResultsPerPage.Entries20 => 20,
            EdgarSearchResultsPerPage.Entries40 => 40,
            EdgarSearchResultsPerPage.Entries80 => 80,
            EdgarSearchResultsPerPage.Entries100 => 100,
            _ => 40
        };
        url += $"&count={count}";
        url += "&action=getcurrent";

        return url;
    }

    private EdgarLatestFilingResult[] ParseLatestFilings(string html)
    {
        // Check for no results
        if (html.ToLower().Contains("no matching filings"))
        {
            return Array.Empty<EdgarLatestFilingResult>();
        }

        var results = new List<EdgarLatestFilingResult>();

        // Find the table
        var tableStart = html.IndexOf("File/Film No");
        var tableEnd = html.IndexOf("</table>", tableStart + 1);

        if (tableStart == -1 || tableEnd == -1)
        {
            return Array.Empty<EdgarLatestFilingResult>();
        }

        var tableHtml = html.Substring(tableStart, tableEnd - tableStart);

        // Extract entity titles (they're in special rows with links)
        var titleRows = tableHtml.Split(new[] { "<tr>" }, StringSplitOptions.RemoveEmptyEntries);
        var titles = new List<string>();

        foreach (var row in titleRows)
        {
            var bgIndex = row.IndexOf("<td bg");
            if (bgIndex == -1) continue;

            var hrefIndex = row.IndexOf("href", bgIndex);
            if (hrefIndex == -1) continue;

            var textStart = row.IndexOf(">", hrefIndex) + 1;
            var textEnd = row.IndexOf("<", textStart);

            if (textStart > 0 && textEnd > textStart)
            {
                titles.Add(row.Substring(textStart, textEnd - textStart));
            }
        }

        if (titles.Count > 0)
        {
            titles.RemoveAt(0); // Remove the header
        }

        // Extract filing data
        var dataRows = tableHtml.Split(new[] { "<tr nowrap" }, StringSplitOptions.None);

        for (int i = 1; i < dataRows.Length && i <= titles.Count; i++)
        {
            var result = new EdgarLatestFilingResult();
            var rowHtml = dataRows[i];

            // Parse entity title and CIK
            var title = titles[i - 1];
            var parenIndex = title.IndexOf('(');
            if (parenIndex > 0)
            {
                result.EntityTitle = title.Substring(0, parenIndex).Trim();

                var cikStart = parenIndex + 1;
                var cikEnd = title.IndexOf(')', cikStart);
                if (cikEnd > cikStart)
                {
                    var cikStr = title.Substring(cikStart, cikEnd - cikStart).Trim();
                    if (long.TryParse(cikStr, out var cik))
                    {
                        result.EntityCik = cik;
                    }
                }
            }

            // Split into columns
            var cols = rowHtml.Split(new[] { "<td" }, StringSplitOptions.None);

            if (cols.Length > 5)
            {
                // Filing type (column 1)
                result.Filing = ExtractText(cols[1]);

                // Documents URL (column 2)
                var hrefIndex = cols[2].IndexOf("a href");
                if (hrefIndex != -1)
                {
                    var urlStart = cols[2].IndexOf('"', hrefIndex) + 1;
                    var urlEnd = cols[2].IndexOf('"', urlStart);
                    if (urlStart > 0 && urlEnd > urlStart)
                    {
                        var url = cols[2].Substring(urlStart, urlEnd - urlStart);
                        result.DocumentsUrl = "https://www.sec.gov" + url;
                    }
                }

                // Description (column 3)
                var desc = ExtractText(cols[3]);
                desc = desc.Replace("<br>", " ").Replace("&nbsp;", " ");
                result.Description = desc;

                // Filing date (column 5)
                var dateStr = ExtractText(cols[5]);
                if (DateTime.TryParse(dateStr, out var filingDate))
                {
                    result.FilingDate = filingDate;
                }

                results.Add(result);
            }
        }

        return results.ToArray();
    }

    private string ExtractText(string html)
    {
        var start = html.IndexOf('>');
        if (start == -1) return string.Empty;

        var end = html.IndexOf("</td", start + 1);
        if (end == -1) end = html.IndexOf('<', start + 1);
        if (end == -1) end = html.Length;

        return html.Substring(start + 1, end - start - 1).Trim();
    }
}
