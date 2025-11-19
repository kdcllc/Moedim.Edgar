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
        var response = await ExecuteRequestAsync(url, cancellationToken).ConfigureAwait(false);
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
        HttpResponseMessage? response = null;
        int attemptCount = 0;

        while (response == null && attemptCount < _options.MaxRetryCount)
        {
            _logger.LogDebug("Preparing SEC request for URL: {Url}, Attempt: {Attempt}", url, attemptCount + 1);

            // Apply request delay
            if (attemptCount > 0 || _options.RequestDelay > TimeSpan.Zero)
            {
                await Task.Delay(_options.RequestDelay, cancellationToken).ConfigureAwait(false);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            try
            {
                var httpResponse = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                _logger.LogDebug("Received response with status code: {StatusCode}", httpResponse.StatusCode);

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    response = httpResponse;
                }
                else if (httpResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Request throttled by SEC (403). Waiting {Timeout} before retry.", _options.TimeoutDelay);
                    await Task.Delay(_options.TimeoutDelay, cancellationToken).ConfigureAwait(false);
                }
                else if (httpResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogWarning("SEC service unavailable (503). Waiting {Timeout} before retry.", _options.TimeoutDelay);
                    await Task.Delay(_options.TimeoutDelay, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogWarning("Unexpected response code: {StatusCode}. Waiting {Timeout} before retry.",
                        httpResponse.StatusCode, _options.TimeoutDelay);
                    await Task.Delay(_options.TimeoutDelay, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SEC request for URL: {Url}", url);
                throw;
            }

            attemptCount++;
        }

        if (response == null)
        {
            throw new InvalidOperationException(
                $"Unable to get data for URL '{url}'. Exceeded maximum retry count of {_options.MaxRetryCount}");
        }

        return response;
    }
}
