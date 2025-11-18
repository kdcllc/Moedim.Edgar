using Moedim.Edgar.Services;
using Newtonsoft.Json.Linq;

namespace Moedim.Edgar.Models;

/// <summary>
/// Service for querying specific company concepts from SEC EDGAR
/// </summary>
/// <remarks>
/// Initializes a new instance of the CompanyConceptService class
/// </remarks>
/// <param name="client">The SEC EDGAR client</param>
public class CompanyConceptService(ISecEdgarClient client) : ICompanyConceptService
{
    private readonly ISecEdgarClient _client = client ?? throw new ArgumentNullException(nameof(client));

    /// <summary>
    /// Queries a specific concept for a company
    /// </summary>
    /// <param name="cik">The Central Index Key of the company</param>
    /// <param name="tag">The XBRL tag to query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A CompanyConceptQuery containing the results</returns>
    public async Task<CompanyConceptQuery> QueryAsync(int cik, string tag, CancellationToken cancellationToken = default)
    {
        string cikPortion = cik.ToString("0000000000");
        string url = $"https://data.sec.gov/api/xbrl/companyconcept/CIK{cikPortion}/us-gaap/{tag}.json";

        string content = await _client.GetAsync(url, cancellationToken);

        JObject jo = JObject.Parse(content);
        return CompanyConceptQuery.Parse(jo);
    }
}
