using System.Threading;
using System.Threading.Tasks;
using Moedim.Edgar.Models;

namespace Moedim.Edgar.Filings
{
    /// <summary>
    /// Service for searching SEC Edgar filings by company
    /// </summary>
    public interface IEdgarSearchService
    {
        /// <summary>
        /// Search for filings by company symbol or CIK
        /// </summary>
        Task<EdgarSearchResponse> SearchAsync(EdgarSearchQuery query, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get the next page of search results
        /// </summary>
        Task<EdgarSearchResponse> NextPageAsync(string nextPageUrl, CancellationToken cancellationToken = default);
    }
}
