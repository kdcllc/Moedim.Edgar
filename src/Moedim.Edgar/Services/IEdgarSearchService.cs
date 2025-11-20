using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.Services;

/// <summary>
/// Interface for searching SEC EDGAR filings by company
/// </summary>
public interface IEdgarSearchService
{
    /// <summary>
    /// Searches for filings by company symbol or CIK
    /// </summary>
    /// <param name="query">The search query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A search response containing filing results and pagination information</returns>
    Task<EdgarSearchResponse> SearchAsync(EdgarSearchQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next page of search results
    /// </summary>
    /// <param name="nextPageUrl">The URL for the next page of results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A search response containing the next page of filing results</returns>
    Task<EdgarSearchResponse> NextPageAsync(string nextPageUrl, CancellationToken cancellationToken = default);
}
