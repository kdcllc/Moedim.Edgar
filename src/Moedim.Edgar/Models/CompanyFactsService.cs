using Moedim.Edgar.Services;
using Newtonsoft.Json.Linq;

namespace Moedim.Edgar.Models;

/// <summary>
/// Service for querying all facts for a company from SEC EDGAR
/// </summary>
/// <remarks>
/// Initializes a new instance of the CompanyFactsService class
/// </remarks>
/// <param name="client">The SEC EDGAR client</param>
public class CompanyFactsService(ISecEdgarClient client) : ICompanyFactsService
{
    private readonly ISecEdgarClient _client = client ?? throw new ArgumentNullException(nameof(client));

    /// <summary>
    /// Queries all facts for a company
    /// </summary>
    /// <param name="cik">The Central Index Key of the company</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A CompanyFactsQuery containing all facts</returns>
    public async Task<CompanyFactsQuery> QueryAsync(int cik, CancellationToken cancellationToken = default)
    {
        string cikPortion = cik.ToString("0000000000");
        string url = $"https://data.sec.gov/api/xbrl/companyfacts/CIK{cikPortion}.json";

        string content = await _client.GetAsync(url, cancellationToken);

        JObject jo = JObject.Parse(content);
        return CompanyFactsQuery.Parse(jo);
    }
}
