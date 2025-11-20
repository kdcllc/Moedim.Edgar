using System.Net;
using Microsoft.Extensions.Logging;
using Moedim.Edgar.Client;
using Moedim.Edgar.Options;
using Polly;

namespace Moedim.Edgar.UnitTests.Client;

public class SecEdgarHttpPoliciesTests
{
    private readonly SecEdgarOptions _options;
    private readonly Mock<ILogger> _loggerMock;

    public SecEdgarHttpPoliciesTests()
    {
        _options = new SecEdgarOptions
        {
            TimeoutDelay = TimeSpan.FromMilliseconds(1),
            RequestDelay = TimeSpan.Zero,
            MaxRetryCount = 3,
            UserAgent = "TestApp/1.0.0 (test@example.com)"
        };

        _loggerMock = new Mock<ILogger>();
    }

    [Fact(DisplayName = "Retry policy retries Forbidden responses and succeeds")]
    public async Task CreateRetryPolicy_ForbiddenThenSuccess_RetriesAndSucceeds()
    {
        var policy = SecEdgarHttpPolicies.CreateRetryPolicy(_options, _loggerMock.Object);
        var attempt = 0;

        using var response = await policy.ExecuteAsync(() =>
        {
            attempt++;
            var statusCode = attempt < 3 ? HttpStatusCode.Forbidden : HttpStatusCode.OK;
            return Task.FromResult(new HttpResponseMessage(statusCode));
        });

        attempt.Should().Be(3);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Retry policy returns no-op when max retries disabled")]
    public async Task CreateRetryPolicy_NoRetriesConfigured_DoesNotRetry()
    {
        var options = new SecEdgarOptions
        {
            MaxRetryCount = 0,
            TimeoutDelay = TimeSpan.Zero,
            UserAgent = _options.UserAgent
        };

        var policy = SecEdgarHttpPolicies.CreateRetryPolicy(options, _loggerMock.Object);
        var attempt = 0;

        using var response = await policy.ExecuteAsync(() =>
        {
            attempt++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        });

        attempt.Should().Be(1);
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact(DisplayName = "ResolveDelay honors Retry-After delta value")]
    public void ResolveDelay_WithRetryAfterDelta_UsesHeader()
    {
        using var retryAfterResponse = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        retryAfterResponse.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(5));
        var outcome = new DelegateResult<HttpResponseMessage>(retryAfterResponse);

        var delay = SecEdgarHttpPolicies.ResolveDelay(1, outcome, _options);

        delay.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "ResolveDelay defaults to timeout when no header present")]
    public void ResolveDelay_WithoutRetryAfter_UsesTimeoutDelay()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        var delay = SecEdgarHttpPolicies.ResolveDelay(1, outcome, _options);

        delay.Should().Be(_options.TimeoutDelay);
    }

    [Fact(DisplayName = "ResolveDelay uses RequestDelay fallback when configured")]
    public void ResolveDelay_WithRequestDelay_UsesRequestDelay()
    {
        var options = new SecEdgarOptions
        {
            RequestDelay = TimeSpan.FromMilliseconds(500),
            TimeoutDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryCount = 2,
            UserAgent = _options.UserAgent
        };

        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        var delay = SecEdgarHttpPolicies.ResolveDelay(1, outcome, options);

        delay.Should().Be(TimeSpan.FromMilliseconds(500));
    }

    [Fact(DisplayName = "Retry count override limits total attempts")]
    public async Task CreateRetryPolicy_RetryCountOverrideLimitsAttempts()
    {
        var options = new SecEdgarOptions
        {
            MaxRetryCount = 5,
            RetryCountOverride = 2,
            TimeoutDelay = TimeSpan.FromMilliseconds(1),
            UserAgent = _options.UserAgent
        };

        var policy = SecEdgarHttpPolicies.CreateRetryPolicy(options, _loggerMock.Object);
        var attempt = 0;

        using var response = await policy.ExecuteAsync(() =>
        {
            attempt++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        });

        attempt.Should().Be(2);
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact(DisplayName = "ResolveDelay applies exponential backoff when enabled")]
    public void ResolveDelay_WithExponentialBackoff_ScalesDelay()
    {
        var options = new SecEdgarOptions
        {
            TimeoutDelay = TimeSpan.FromMilliseconds(100),
            RetryDelay = TimeSpan.FromMilliseconds(200),
            UseExponentialBackoff = true,
            RetryBackoffMultiplier = 3,
            MaxRetryCount = 4,
            UserAgent = _options.UserAgent
        };

        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        var delay = SecEdgarHttpPolicies.ResolveDelay(3, outcome, options);

        delay.Should().Be(TimeSpan.FromMilliseconds(1800));
    }
}
