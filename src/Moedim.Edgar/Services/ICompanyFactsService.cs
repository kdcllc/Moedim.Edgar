using Moedim.Edgar.Models;

namespace Moedim.Edgar.Services;

/// <summary>
/// Interface for querying all facts for a company from SEC EDGAR
/// </summary>
public interface ICompanyFactsService
{
    /// <summary>
    /// Queries all facts for a company
    /// </summary>
    /// <param name="cik">The Central Index Key of the company</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A CompanyFactsQuery containing all facts</returns>
    Task<CompanyFactsQuery> QueryAsync(int cik, CancellationToken cancellationToken = default);
}
