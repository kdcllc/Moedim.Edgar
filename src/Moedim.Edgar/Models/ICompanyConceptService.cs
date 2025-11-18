namespace Moedim.Edgar.Models;

/// <summary>
/// Interface for querying specific company concepts from SEC EDGAR
/// </summary>
public interface ICompanyConceptService
{
    /// <summary>
    /// Queries a specific concept for a company
    /// </summary>
    /// <param name="cik">The Central Index Key of the company</param>
    /// <param name="tag">The XBRL tag to query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A CompanyConceptQuery containing the results</returns>
    Task<CompanyConceptQuery> QueryAsync(int cik, string tag, CancellationToken cancellationToken = default);
}
