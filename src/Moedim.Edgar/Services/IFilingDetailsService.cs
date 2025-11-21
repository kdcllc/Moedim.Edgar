using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.Services;

/// <summary>
/// Interface for retrieving detailed information about SEC filings
/// </summary>
public interface IFilingDetailsService
{
    /// <summary>
    /// Gets detailed information about a filing from its documents URL
    /// </summary>
    /// <param name="documentsUrl">The URL to the filing documents page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed filing information including metadata and document lists</returns>
    Task<EdgarFilingDetails> GetFilingDetailsAsync(string documentsUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the CIK from a filing documents URL
    /// </summary>
    /// <param name="documentsUrl">The URL to the filing documents page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The CIK as a long integer</returns>
    Task<long> GetCikFromFilingAsync(string documentsUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets document format files from a filing
    /// </summary>
    /// <param name="documentsUrl">The URL to the filing documents page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An array of document format files</returns>
    Task<FilingDocument[]> GetDocumentFormatFilesAsync(string documentsUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets data files from a filing
    /// </summary>
    /// <param name="documentsUrl">The URL to the filing documents page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An array of data files</returns>
    Task<FilingDocument[]> GetDataFilesAsync(string documentsUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a specific filing document
    /// </summary>
    /// <param name="documentUrl">The URL to the specific document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A stream containing the document content</returns>
    Task<Stream> DownloadDocumentAsync(string documentUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the XBRL instance document from a filing
    /// </summary>
    /// <param name="documentsUrl">The URL to the filing documents page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A stream containing the XBRL instance document</returns>
    Task<Stream> DownloadXbrlDocumentAsync(string documentsUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the complete content of a filing document in the specified format
    /// </summary>
    /// <param name="accessionNumber">The filing accession number</param>
    /// <param name="tickerOrCik">Optional ticker symbol or CIK to help locate the filing</param>
    /// <param name="format">Output format: 'markdown' (default) or 'html'</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The complete filing content in the requested format</returns>
    Task<string?> GetFilingDocumentAsync(string accessionNumber, string? tickerOrCik = null, string? format = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sections from a filing based on the request parameters
    /// </summary>
    /// <param name="accessionNumber">The filing accession number</param>
    /// <param name="request">Section request parameters</param>
    /// <param name="tickerOrCik">Optional ticker symbol or CIK to help locate the filing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Filing sections result with preview or full content</returns>
    Task<FilingSectionsResult?> GetFilingSectionsAsync(string accessionNumber, FilingSectionsRequest request, string? tickerOrCik = null, CancellationToken cancellationToken = default);
}
