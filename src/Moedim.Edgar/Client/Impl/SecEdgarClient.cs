using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moedim.Edgar.Options;

namespace Moedim.Edgar.Client.Impl;

/// <summary>
/// SEC EDGAR API client implementation using HttpClient
/// </summary>
/// <remarks>
/// Initializes a new instance of the SecEdgarClient class
/// </remarks>
/// <param name="httpClient">The HTTP client</param>
/// <param name="options">The SEC EDGAR options</param>
/// <param name="logger">Optional logger</param>
public class SecEdgarClient(
    HttpClient httpClient,
    IOptions<SecEdgarOptions> options,
    ILogger<SecEdgarClient> logger) : ISecEdgarClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<SecEdgarClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly SecEdgarOptions _options = (options ?? throw new ArgumentNullException(nameof(options))).Value
        ?? throw new ArgumentNullException(nameof(options), "Options value cannot be null");

    /// <inheritdoc />
    public async Task<string> GetAsync(string url, CancellationToken cancellationToken = default)
    {
        using var response = await ExecuteRequestAsync(url, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default)
    {
        var response = await ExecuteRequestAsync(url, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> ExecuteRequestAsync(string url, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Preparing SEC request for URL: {Url}", url);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SEC request for URL: {Url}", url);
            throw;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogError(
                "SEC request for URL {Url} failed with status code {StatusCode} after {MaxRetryCount} attempts.",
                url,
                response.StatusCode,
                _options.MaxRetryCount);
            response.Dispose();
            throw new InvalidOperationException(
                $"Unable to get data for URL '{url}'. Exceeded maximum retry count of {_options.MaxRetryCount}");
        }

        _logger.LogDebug("SEC request for URL: {Url} succeeded", url);
        return response;
    }
}
