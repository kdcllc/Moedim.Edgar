namespace Moedim.Edgar.Client;

/// <summary>
/// Interface for SEC EDGAR API client
/// </summary>
public interface ISecEdgarClient
{
    /// <summary>
    /// Gets the content from the specified URL as a string
    /// </summary>
    /// <param name="url">The URL to fetch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response content as a string</returns>
    Task<string> GetAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the content from the specified URL as a stream
    /// </summary>
    /// <param name="url">The URL to fetch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response content as a stream. Caller is responsible for disposing the stream.</returns>
    /// <remarks>The returned stream must be disposed by the caller.</remarks>
    Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default);
}
