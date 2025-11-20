using Microsoft.Extensions.Options;
using Moedim.Edgar.Client;
using Moedim.Edgar.Models.Data;
using Moedim.Edgar.Options;
using Newtonsoft.Json.Linq;

namespace Moedim.Edgar.Services.Impl;

/// <summary>
/// Service for querying all facts for a company from SEC EDGAR
/// </summary>
/// <remarks>
/// Initializes a new instance of the CompanyFactsService class
/// </remarks>
/// <param name="client">The SEC EDGAR client</param>
/// <param name="options">The SEC EDGAR options</param>
public class CompanyFactsService(
    ISecEdgarClient client,
    IOptions<SecEdgarOptions> options) : ICompanyFactsService
{
    private readonly ISecEdgarClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly SecEdgarOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <summary>
    /// Queries all facts for a company
    /// </summary>
    /// <param name="cik">The Central Index Key of the company</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A CompanyFactsQuery containing all facts</returns>
    public async Task<CompanyFactsQuery> QueryAsync(int cik, CancellationToken cancellationToken = default)
    {
        if (cik <= 0)
            throw new ArgumentOutOfRangeException(nameof(cik), "CIK must be positive");

        string cikPortion = cik.ToString("0000000000");
        string url = $"{_options.BaseApiUrl}/companyfacts/CIK{cikPortion}.json";

        string content = await _client.GetAsync(url, cancellationToken).ConfigureAwait(false);

        JObject jo = JObject.Parse(content);
        return CompanyFactsQuery.Parse(jo);
    }
}
