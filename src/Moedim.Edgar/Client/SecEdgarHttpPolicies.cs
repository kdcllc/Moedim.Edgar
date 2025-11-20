using System.Net;
using Microsoft.Extensions.Logging;
using Moedim.Edgar.Options;
using Polly;
using Polly.Extensions.Http;

namespace Moedim.Edgar.Client;

/// <summary>
/// Factory for SEC EDGAR HTTP resilience policies.
/// </summary>
internal static class SecEdgarHttpPolicies
{
    /// <summary>
    /// Creates a retry policy tailored for SEC EDGAR throttling guidance.
    /// </summary>
    internal static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(
        SecEdgarOptions options,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        var effectiveRetryLimit = options.RetryCountOverride ?? options.MaxRetryCount;
        var retryCount = Math.Max(0, effectiveRetryLimit - 1);
        if (retryCount == 0)
        {
            return Policy.NoOpAsync<HttpResponseMessage>();
        }

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(result => result.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount,
                (attempt, outcome, context) => ResolveDelay(attempt, outcome, options),
                (outcome, timespan, attempt, context) =>
                {
                    var statusCode = outcome.Result?.StatusCode;
                    logger.LogWarning(
                        "Retrying SEC request after status code {StatusCode}. Attempt {Attempt} of {MaxAttempts} waiting {Delay}.",
                        statusCode,
                        attempt + 1,
                        effectiveRetryLimit,
                        timespan);
                    return Task.CompletedTask;
                });
    }

    /// <summary>
    /// Determines the delay between retry attempts for a given outcome.
    /// </summary>
    internal static TimeSpan ResolveDelay(
        int attempt,
        DelegateResult<HttpResponseMessage> outcome,
        SecEdgarOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (attempt < 1)
        {
            attempt = 1;
        }

        if (outcome.Result is { Headers.RetryAfter: { } retryAfter })
        {
            if (retryAfter.Delta is { } delta && delta > TimeSpan.Zero)
            {
                return delta;
            }

            if (retryAfter.Date is { } date)
            {
                var delay = date - DateTimeOffset.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    return delay;
                }
            }
        }

        var requestDelay = options.RequestDelay > TimeSpan.Zero ? options.RequestDelay : TimeSpan.Zero;
        var baseDelay = options.RetryDelay
                         ?? (requestDelay > TimeSpan.Zero ? requestDelay : options.TimeoutDelay);
        if (baseDelay <= TimeSpan.Zero)
        {
            baseDelay = TimeSpan.FromMilliseconds(1);
        }

        if (!options.UseExponentialBackoff || attempt == 1)
        {
            return baseDelay;
        }

        var multiplier = Math.Pow(options.RetryBackoffMultiplier, attempt - 1);
        if (double.IsNaN(multiplier) || double.IsInfinity(multiplier) || multiplier <= 0)
        {
            return baseDelay;
        }

        var computedDelayMs = baseDelay.TotalMilliseconds * multiplier;
        if (computedDelayMs <= 0 || double.IsInfinity(computedDelayMs) || double.IsNaN(computedDelayMs))
        {
            return baseDelay;
        }

        return TimeSpan.FromMilliseconds(computedDelayMs);
    }
}
