using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moedim.Edgar.Models;

namespace Moedim.Edgar.Filings
{
    /// <summary>
    /// Service for retrieving detailed information about SEC filings
    /// </summary>
    public interface IFilingDetailsService
    {
        /// <summary>
        /// Get detailed information about a filing from its documents URL
        /// </summary>
        Task<EdgarFilingDetails> GetFilingDetailsAsync(string documentsUrl, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get the CIK from a filing documents URL
        /// </summary>
        Task<long> GetCikFromFilingAsync(string documentsUrl, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get document format files from a filing
        /// </summary>
        Task<FilingDocument[]> GetDocumentFormatFilesAsync(string documentsUrl, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get data files from a filing
        /// </summary>
        Task<FilingDocument[]> GetDataFilesAsync(string documentsUrl, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Download a specific filing document
        /// </summary>
        Task<Stream> DownloadDocumentAsync(string documentUrl, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Download the XBRL instance document from a filing
        /// </summary>
        Task<Stream> DownloadXbrlDocumentAsync(string documentsUrl, CancellationToken cancellationToken = default);
    }
}
