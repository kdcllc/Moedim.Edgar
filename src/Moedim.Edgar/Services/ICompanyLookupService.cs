namespace Moedim.Edgar.Services;

/// <summary>
/// Interface for looking up company information in SEC EDGAR
/// </summary>
public interface ICompanyLookupService
{
    /// <summary>
    /// Gets the CIK (Central Index Key) for a company from its trading symbol
    /// </summary>
    /// <param name="symbol">The trading symbol of the company</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The CIK as a string</returns>
    Task<string> GetCikFromSymbolAsync(string symbol, CancellationToken cancellationToken = default);
}
