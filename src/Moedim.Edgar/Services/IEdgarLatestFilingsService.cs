using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.Services;

/// <summary>
/// Interface for searching latest SEC EDGAR filings across all companies
/// </summary>
public interface IEdgarLatestFilingsService
{
    /// <summary>
    /// Searches for latest filings across all companies
    /// </summary>
    /// <param name="query">The search query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An array of latest filing results</returns>
    Task<EdgarLatestFilingResult[]> SearchAsync(EdgarLatestFilingsQuery query, CancellationToken cancellationToken = default);
}
