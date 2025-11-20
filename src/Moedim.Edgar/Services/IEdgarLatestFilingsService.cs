using System.Threading;
using System.Threading.Tasks;
using Moedim.Edgar.Models;

namespace Moedim.Edgar.Filings
{
    /// <summary>
    /// Service for searching latest SEC Edgar filings across all companies
    /// </summary>
    public interface IEdgarLatestFilingsService
    {
        /// <summary>
        /// Search for latest filings across all companies
        /// </summary>
        Task<EdgarLatestFilingResult[]> SearchAsync(EdgarLatestFilingsQuery query, CancellationToken cancellationToken = default);
    }
}
