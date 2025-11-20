using System.Threading;
using System.Threading.Tasks;

namespace Moedim.Edgar.Utils
{
    /// <summary>
    /// Service for looking up company information in SEC Edgar
    /// </summary>
    public interface ICompanyLookupService
    {
        /// <summary>
        /// Get the CIK (Central Index Key) for a company from its trading symbol
        /// </summary>
        Task<string> GetCikFromSymbolAsync(string symbol, CancellationToken cancellationToken = default);
    }
}
