using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moedim.Edgar.Client;

namespace Moedim.Edgar.Utils
{
    /// <summary>
    /// Service implementation for company lookups in SEC Edgar
    /// </summary>
    public class CompanyLookupService : ICompanyLookupService
    {
        private readonly ISecEdgarClient _client;
        private readonly ILogger<CompanyLookupService>? _logger;

        public CompanyLookupService(ISecEdgarClient client, ILogger<CompanyLookupService>? logger = null)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger;
        }

        public async Task<string> GetCikFromSymbolAsync(string symbol, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("Symbol is required", nameof(symbol));
            }

            var url = $"https://www.sec.gov/cgi-bin/browse-edgar?CIK={symbol}&owner=exclude";

            _logger?.LogDebug("Looking up CIK for symbol: {Symbol}", symbol);

            var html = await _client.GetAsync(url, cancellationToken);

            // Check if symbol was found
            if (html.Contains("<h1>No matching Ticker Symbol.</h1>"))
            {
                throw new InvalidOperationException($"No matching ticker symbol in the SEC database for '{symbol}'.");
            }

            // Parse CIK from response
            var cikLabel = ">CIK</acronym>";
            var labelIndex = html.IndexOf(cikLabel);

            if (labelIndex == -1)
            {
                throw new InvalidOperationException($"Unable to parse CIK from response for symbol '{symbol}'.");
            }

            var hrefIndex = html.IndexOf("href", labelIndex);
            var textStart = html.IndexOf(">", hrefIndex) + 1;
            var textEnd = html.IndexOf(" ", textStart);

            var cik = html.Substring(textStart, textEnd - textStart).Trim();

            _logger?.LogDebug("Found CIK {CIK} for symbol {Symbol}", cik, symbol);

            return cik;
        }
    }
}
