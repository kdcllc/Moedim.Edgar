using Microsoft.Extensions.Logging;
using Moedim.Edgar.Client;
using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.Services.Impl;

/// <summary>
/// Service implementation for searching SEC Edgar filings
/// </summary>
public class EdgarSearchService(ISecEdgarClient client, ILogger<EdgarSearchService>? logger = null) : IEdgarSearchService
{
    private readonly ISecEdgarClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly ILogger<EdgarSearchService>? _logger = logger;

    /// <inheritdoc/>
    public async Task<EdgarSearchResponse> SearchAsync(EdgarSearchQuery query, CancellationToken cancellationToken = default)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (string.IsNullOrWhiteSpace(query.Symbol))
            throw new ArgumentException("Symbol is required", nameof(query));

        var url = BuildSearchUrl(query);
        _logger?.LogDebug("Searching Edgar filings: {Url}", url);

        var html = await _client.GetAsync(url, cancellationToken);
        return ParseSearchResponse(html);
    }

    /// <inheritdoc/>
    public async Task<EdgarSearchResponse> NextPageAsync(string nextPageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nextPageUrl))
            throw new ArgumentException("Next page URL is required", nameof(nextPageUrl));

        _logger?.LogDebug("Getting next page of Edgar search results: {Url}", nextPageUrl);

        var html = await _client.GetAsync(nextPageUrl, cancellationToken);
        return ParseSearchResponse(html);
    }

    private string BuildSearchUrl(EdgarSearchQuery query)
    {
        var url = "https://www.sec.gov/cgi-bin/browse-edgar?action=getcompany";
        url += $"&CIK={query.Symbol}";

        if (!string.IsNullOrWhiteSpace(query.FilingType))
        {
            url += $"&type={query.FilingType}";
        }

        if (query.PriorTo.HasValue)
        {
            var date = query.PriorTo.Value;
            var dateStr = $"{date.Year:0000}{date.Month:00}{date.Day:00}";
            url += $"&dateb={dateStr}";
        }

        url += query.OwnershipFilter switch
        {
            EdgarSearchOwnershipFilter.Exclude => "&owner=exclude",
            EdgarSearchOwnershipFilter.Include => "&owner=include",
            EdgarSearchOwnershipFilter.Only => "&owner=only",
            _ => "&owner=exclude"
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

        return url;
    }

    private EdgarSearchResponse ParseSearchResponse(string html)
    {
        var response = new EdgarSearchResponse();

        // Check for errors
        if (html.Contains("No matching Ticker Symbol."))
        {
            throw new InvalidOperationException("No matching Ticker Symbol.");
        }

        // Find the results table
        var tableStart = html.IndexOf("tableFile2");
        var tableEnd = html.IndexOf("</table>", tableStart + 1);

        if (tableStart == -1 || tableEnd == -1)
        {
            // No results found
            return response;
        }

        var tableHtml = html.Substring(tableStart, tableEnd - tableStart);
        var rows = tableHtml.Split(new[] { "<tr" }, StringSplitOptions.None);

        var results = new List<EdgarSearchResult>();

        // Skip first 2 rows (header rows)
        for (int i = 2; i < rows.Length; i++)
        {
            var cols = rows[i].Split(new[] { "<td" }, StringSplitOptions.None);

            if (cols.Length > 4)
            {
                var result = new EdgarSearchResult();

                // Filing type (column 1)
                result.Filing = ExtractText(cols[1]);

                // Documents URL (column 2)
                var documentsLink = ExtractAttribute(cols[2], "href", "documentsbutton");
                if (!string.IsNullOrEmpty(documentsLink))
                {
                    result.DocumentsUrl = "https://www.sec.gov" + documentsLink;
                }

                // Interactive data URL (column 2)
                var interactiveLink = ExtractAttribute(cols[2], "href", "interactiveDataBtn");
                if (!string.IsNullOrEmpty(interactiveLink))
                {
                    result.InteractiveDataUrl = "https://www.sec.gov" + interactiveLink.Replace("&amp;", "&");
                }

                // Description (column 3)
                result.Description = ExtractText(cols[3]);

                // Filing date (column 4)
                var dateStr = ExtractText(cols[4]);
                if (DateTime.TryParse(dateStr, out var filingDate))
                {
                    result.FilingDate = filingDate;
                }

                results.Add(result);
            }
        }

        response.Results = results.ToArray();

        // Check for next page
        var nextButtonText = "<input type=\"button\" value=\"Next ";
        var nextButtonIndex = html.IndexOf(nextButtonText);
        if (nextButtonIndex != -1)
        {
            var locationIndex = html.IndexOf(".location", nextButtonIndex);
            if (locationIndex != -1)
            {
                var urlStart = html.IndexOf("'", locationIndex) + 1;
                var urlEnd = html.IndexOf("'", urlStart);
                var nextUrl = html.Substring(urlStart, urlEnd - urlStart);
                response.NextPageUrl = "https://www.sec.gov" + nextUrl;
            }
        }

        return response;
    }

    private string ExtractText(string html)
    {
        var start = html.IndexOf('>');
        if (start == -1) return string.Empty;

        var end = html.IndexOf('<', start + 1);
        if (end == -1) end = html.Length;

        return html.Substring(start + 1, end - start - 1).Trim();
    }

    private string ExtractAttribute(string html, string attribute, string searchText)
    {
        var searchIndex = html.IndexOf(searchText);
        if (searchIndex == -1) return string.Empty;

        var attrIndex = html.LastIndexOf(attribute, searchIndex);
        if (attrIndex == -1) return string.Empty;

        var valueStart = html.IndexOf('"', attrIndex) + 1;
        var valueEnd = html.IndexOf('"', valueStart);

        if (valueStart == 0 || valueEnd == -1) return string.Empty;

        return html.Substring(valueStart, valueEnd - valueStart);
    }
}
