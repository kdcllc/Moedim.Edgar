using Microsoft.Extensions.Options;
using Moedim.Edgar.Client;
using Moedim.Edgar.Models.Data;
using Moedim.Edgar.Options;
using Newtonsoft.Json.Linq;

namespace Moedim.Edgar.Services.Impl;

/// <summary>
/// Service for querying specific company concepts from SEC EDGAR
/// </summary>
/// <remarks>
/// Initializes a new instance of the CompanyConceptService class
/// </remarks>
/// <param name="client">The SEC EDGAR client</param>
/// <param name="options">The SEC EDGAR options</param>
public class CompanyConceptService(
    ISecEdgarClient client,
    IOptions<SecEdgarOptions> options) : ICompanyConceptService
{
    private readonly ISecEdgarClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly SecEdgarOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <summary>
    /// Queries a specific concept for a company
    /// </summary>
    /// <param name="cik">The Central Index Key of the company</param>
    /// <param name="tag">The XBRL tag to query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A CompanyConceptQuery containing the results</returns>
    public async Task<CompanyConceptQuery> QueryAsync(int cik, string tag, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tag);
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be empty", nameof(tag));
        if (cik <= 0)
            throw new ArgumentOutOfRangeException(nameof(cik), "CIK must be positive");

        string cikPortion = cik.ToString("0000000000");
        string url = $"{_options.BaseApiUrl}/companyconcept/CIK{cikPortion}/us-gaap/{tag}.json";

        string content = await _client.GetAsync(url, cancellationToken).ConfigureAwait(false);

        JObject jo = JObject.Parse(content);
        return CompanyConceptQuery.Parse(jo);
    }
}
