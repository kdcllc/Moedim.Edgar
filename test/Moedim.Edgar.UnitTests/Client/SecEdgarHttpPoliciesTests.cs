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

    [Fact(DisplayName = "CreateRetryPolicy with null options throws ArgumentNullException")]
    public void CreateRetryPolicy_NullOptions_ThrowsArgumentNullException()
    {
        SecEdgarOptions? nullOptions = null;
        var exception = Assert.Throws<ArgumentNullException>(() =>
            SecEdgarHttpPolicies.CreateRetryPolicy(nullOptions!, _loggerMock.Object));

        exception.ParamName.Should().Be("options");
    }

    [Fact(DisplayName = "ResolveDelay with null options throws ArgumentNullException")]
    public void ResolveDelay_NullOptions_ThrowsArgumentNullException()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        SecEdgarOptions? nullOptions = null;
        var exception = Assert.Throws<ArgumentNullException>(() =>
            SecEdgarHttpPolicies.ResolveDelay(1, outcome, nullOptions!));

        exception.ParamName.Should().Be("options");
    }

    [Fact(DisplayName = "ResolveDelay with attempt zero uses attempt one")]
    public void ResolveDelay_AttemptZero_UsesAttemptOne()
    {
        var options = new SecEdgarOptions
        {
            TimeoutDelay = TimeSpan.FromMilliseconds(100),
            RetryDelay = TimeSpan.FromMilliseconds(200),
            UseExponentialBackoff = true,
            RetryBackoffMultiplier = 2,
            MaxRetryCount = 3,
            UserAgent = _options.UserAgent
        };

        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        var delay = SecEdgarHttpPolicies.ResolveDelay(0, outcome, options);

        delay.Should().Be(TimeSpan.FromMilliseconds(200)); // Should use attempt 1
    }

    [Fact(DisplayName = "ResolveDelay with negative attempt uses attempt one")]
    public void ResolveDelay_NegativeAttempt_UsesAttemptOne()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        var delay = SecEdgarHttpPolicies.ResolveDelay(-5, outcome, _options);

        delay.Should().Be(_options.TimeoutDelay);
    }

    [Fact(DisplayName = "ResolveDelay with Retry-After date format")]
    public void ResolveDelay_RetryAfterDate_CalculatesCorrectDelay()
    {
        var futureDate = DateTimeOffset.UtcNow.AddSeconds(5);
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(futureDate);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        var delay = SecEdgarHttpPolicies.ResolveDelay(1, outcome, _options);

        delay.Should().BeGreaterThan(TimeSpan.FromSeconds(4));
        delay.Should().BeLessThan(TimeSpan.FromSeconds(6));
    }

    [Fact(DisplayName = "ResolveDelay with past Retry-After date uses fallback")]
    public void ResolveDelay_RetryAfterPastDate_UsesFallback()
    {
        var pastDate = DateTimeOffset.UtcNow.AddSeconds(-5);
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(pastDate);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        var delay = SecEdgarHttpPolicies.ResolveDelay(1, outcome, _options);

        delay.Should().Be(_options.TimeoutDelay);
    }

    [Fact(DisplayName = "ResolveDelay with infinity multiplier returns base delay")]
    public void ResolveDelay_InfinityMultiplier_ReturnsBaseDelay()
    {
        var options = new SecEdgarOptions
        {
            TimeoutDelay = TimeSpan.FromMilliseconds(100),
            RetryDelay = TimeSpan.FromMilliseconds(200),
            UseExponentialBackoff = true,
            RetryBackoffMultiplier = double.PositiveInfinity,
            MaxRetryCount = 10,
            UserAgent = _options.UserAgent
        };

        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        var delay = SecEdgarHttpPolicies.ResolveDelay(5, outcome, options);

        // When multiplier is infinity, the code detects it and returns baseDelay
        delay.Should().Be(TimeSpan.FromMilliseconds(200));
    }

    [Fact(DisplayName = "ResolveDelay without exponential backoff uses constant delay")]
    public void ResolveDelay_NoExponentialBackoff_UsesConstantDelay()
    {
        var options = new SecEdgarOptions
        {
            TimeoutDelay = TimeSpan.FromMilliseconds(100),
            RetryDelay = TimeSpan.FromMilliseconds(200),
            UseExponentialBackoff = false,
            MaxRetryCount = 3,
            UserAgent = _options.UserAgent
        };

        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        var delay1 = SecEdgarHttpPolicies.ResolveDelay(1, outcome, options);
        var delay2 = SecEdgarHttpPolicies.ResolveDelay(2, outcome, options);
        var delay3 = SecEdgarHttpPolicies.ResolveDelay(3, outcome, options);

        delay1.Should().Be(TimeSpan.FromMilliseconds(200));
        delay2.Should().Be(TimeSpan.FromMilliseconds(200));
        delay3.Should().Be(TimeSpan.FromMilliseconds(200));
    }

    [Fact(DisplayName = "ResolveDelay with all zero delays uses minimal 1ms delay")]
    public void ResolveDelay_AllZeroDelays_UsesMinimal1ms()
    {
        var options = new SecEdgarOptions
        {
            TimeoutDelay = TimeSpan.Zero,
            RequestDelay = TimeSpan.Zero,
            RetryDelay = TimeSpan.Zero,
            MaxRetryCount = 3,
            UserAgent = _options.UserAgent
        };

        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        var delay = SecEdgarHttpPolicies.ResolveDelay(1, outcome, options);

        // When all delays are zero or negative, code defaults to 1ms
        delay.Should().Be(TimeSpan.FromMilliseconds(1));
    }

    [Fact(DisplayName = "ResolveDelay with zero multiplier uses base delay")]
    public void ResolveDelay_ZeroMultiplier_UsesBaseDelay()
    {
        var options = new SecEdgarOptions
        {
            TimeoutDelay = TimeSpan.FromMilliseconds(100),
            RetryDelay = TimeSpan.FromMilliseconds(200),
            UseExponentialBackoff = true,
            RetryBackoffMultiplier = 0,
            MaxRetryCount = 3,
            UserAgent = _options.UserAgent
        };

        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        var delay = SecEdgarHttpPolicies.ResolveDelay(2, outcome, options);

        delay.Should().Be(TimeSpan.FromMilliseconds(200));
    }

    [Fact(DisplayName = "ResolveDelay with multiplier of one uses linear scaling")]
    public void ResolveDelay_MultiplierOne_UsesLinearScaling()
    {
        var options = new SecEdgarOptions
        {
            TimeoutDelay = TimeSpan.FromMilliseconds(100),
            RetryDelay = TimeSpan.FromMilliseconds(200),
            UseExponentialBackoff = true,
            RetryBackoffMultiplier = 1,
            MaxRetryCount = 5,
            UserAgent = _options.UserAgent
        };

        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        var delay1 = SecEdgarHttpPolicies.ResolveDelay(1, outcome, options);
        var delay2 = SecEdgarHttpPolicies.ResolveDelay(2, outcome, options);
        var delay3 = SecEdgarHttpPolicies.ResolveDelay(3, outcome, options);

        delay1.Should().Be(TimeSpan.FromMilliseconds(200)); // 200 * 1
        delay2.Should().Be(TimeSpan.FromMilliseconds(200)); // 200 * 1
        delay3.Should().Be(TimeSpan.FromMilliseconds(200)); // 200 * 1
    }

    [Fact(DisplayName = "ResolveDelay with first attempt has minimal backoff")]
    public void ResolveDelay_FirstAttempt_MinimalBackoff()
    {
        var options = new SecEdgarOptions
        {
            TimeoutDelay = TimeSpan.FromMilliseconds(100),
            RetryDelay = TimeSpan.FromMilliseconds(200),
            UseExponentialBackoff = true,
            RetryBackoffMultiplier = 2,
            MaxRetryCount = 5,
            UserAgent = _options.UserAgent
        };

        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        var outcome = new DelegateResult<HttpResponseMessage>(response);

        var delay = SecEdgarHttpPolicies.ResolveDelay(1, outcome, options);

        delay.Should().Be(TimeSpan.FromMilliseconds(200)); // First attempt, no backoff multiplication
    }
}
