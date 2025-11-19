using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moedim.Edgar.Client;
using Moedim.Edgar.Client.Impl;
using Moedim.Edgar.Options;
using Moedim.Edgar.UnitTests.Helpers;

namespace Moedim.Edgar.UnitTests.Services;

/// <summary>
/// Unit tests for SecEdgarClient
/// </summary>
public class SecEdgarClientTests : IDisposable
{
    private readonly Mock<ILogger<SecEdgarClient>> _loggerMock;
    private readonly IOptions<SecEdgarOptions> _options;
    private readonly HttpClient _httpClient;
    private readonly MockHttpMessageHandler _mockHandler;

    public SecEdgarClientTests()
    {
        _loggerMock = new Mock<ILogger<SecEdgarClient>>();
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);

        var options = new SecEdgarOptions
        {
            UserAgent = "TestApp/1.0.0 (test@example.com)",
            RequestDelay = TimeSpan.Zero,
            TimeoutDelay = TimeSpan.FromMilliseconds(10),
            MaxRetryCount = 3
        };

        _options = Microsoft.Extensions.Options.Options.Create(options);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _mockHandler?.Dispose();
    }

    [Fact(DisplayName = "Constructor with null HttpClient throws ArgumentNullException")]
    public void Constructor_NullHttpClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SecEdgarClient(null!, _options, _loggerMock.Object));

        exception.ParamName.Should().Be("httpClient");
    }

    [Fact(DisplayName = "Constructor with null options throws ArgumentNullException")]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SecEdgarClient(_httpClient, null!, _loggerMock.Object));

        exception.ParamName.Should().Be("options");
    }

    [Fact(DisplayName = "Constructor with null logger throws ArgumentNullException")]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SecEdgarClient(_httpClient, _options, null!));

        exception.ParamName.Should().Be("logger");
    }

    [Fact(DisplayName = "Constructor with valid parameters creates instance successfully")]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        // Act
        var client = new SecEdgarClient(_httpClient, _options, _loggerMock.Object);

        // Assert
        client.Should().NotBeNull();
        client.Should().BeAssignableTo<ISecEdgarClient>();
    }

    [Fact(DisplayName = "GetAsync with successful response returns content as string")]
    public async Task GetAsync_SuccessfulResponse_ReturnsContentAsString()
    {
        // Arrange
        const string expectedContent = "{\"data\": \"test\"}";
        _mockHandler.SetResponse(HttpStatusCode.OK, expectedContent);
        var client = new SecEdgarClient(_httpClient, _options, _loggerMock.Object);

        // Act
        var result = await client.GetAsync("https://test.com/api");

        // Assert
        result.Should().Be(expectedContent);
        _mockHandler.RequestCount.Should().Be(1);
    }

    [Fact(DisplayName = "GetAsync with 403 response retries and succeeds")]
    public async Task GetAsync_ForbiddenThenSuccess_RetriesAndSucceeds()
    {
        // Arrange
        const string expectedContent = "{\"data\": \"test\"}";
        _mockHandler.SetResponses(
            new HttpResponseMessage(HttpStatusCode.Forbidden),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedContent) }
        );
        var client = new SecEdgarClient(_httpClient, _options, _loggerMock.Object);

        // Act
        var result = await client.GetAsync("https://test.com/api");

        // Assert
        result.Should().Be(expectedContent);
        _mockHandler.RequestCount.Should().Be(2);
    }

    [Fact(DisplayName = "GetAsync with 503 response retries and succeeds")]
    public async Task GetAsync_ServiceUnavailableThenSuccess_RetriesAndSucceeds()
    {
        // Arrange
        const string expectedContent = "{\"data\": \"test\"}";
        _mockHandler.SetResponses(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedContent) }
        );
        var client = new SecEdgarClient(_httpClient, _options, _loggerMock.Object);

        // Act
        var result = await client.GetAsync("https://test.com/api");

        // Assert
        result.Should().Be(expectedContent);
        _mockHandler.RequestCount.Should().Be(2);
    }

    [Fact(DisplayName = "GetAsync exceeding max retries throws InvalidOperationException")]
    public async Task GetAsync_ExceedingMaxRetries_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockHandler.SetResponse(HttpStatusCode.Forbidden);
        var client = new SecEdgarClient(_httpClient, _options, _loggerMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetAsync("https://test.com/api"));

        exception.Message.Should().Contain("Exceeded maximum retry count");
        _mockHandler.RequestCount.Should().Be(3); // MaxRetryCount = 3
    }

    [Fact(DisplayName = "GetAsync with cancellation token cancels request")]
    public async Task GetAsync_WithCancellationToken_CancelsRequest()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _mockHandler.SetDelayedResponse(HttpStatusCode.OK, "{}", TimeSpan.FromSeconds(5));
        var client = new SecEdgarClient(_httpClient, _options, _loggerMock.Object);

        // Act
        cts.Cancel();

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => client.GetAsync("https://test.com/api", cts.Token));
    }

    [Fact(DisplayName = "GetStreamAsync with successful response returns stream")]
    public async Task GetStreamAsync_SuccessfulResponse_ReturnsStream()
    {
        // Arrange
        const string expectedContent = "{\"data\": \"test\"}";
        _mockHandler.SetResponse(HttpStatusCode.OK, expectedContent);
        var client = new SecEdgarClient(_httpClient, _options, _loggerMock.Object);

        // Act
        var stream = await client.GetStreamAsync("https://test.com/api");

        // Assert
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(expectedContent);
        _mockHandler.RequestCount.Should().Be(1);
    }

    [Fact(DisplayName = "GetStreamAsync with retry logic succeeds after throttling")]
    public async Task GetStreamAsync_AfterThrottling_Succeeds()
    {
        // Arrange
        const string expectedContent = "{\"data\": \"test\"}";
        _mockHandler.SetResponses(
            new HttpResponseMessage(HttpStatusCode.Forbidden),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedContent) }
        );
        var client = new SecEdgarClient(_httpClient, _options, _loggerMock.Object);

        // Act
        var stream = await client.GetStreamAsync("https://test.com/api");

        // Assert
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(expectedContent);
        _mockHandler.RequestCount.Should().Be(2);
    }

    [Fact(DisplayName = "GetStreamAsync exceeding max retries throws InvalidOperationException")]
    public async Task GetStreamAsync_ExceedingMaxRetries_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockHandler.SetResponse(HttpStatusCode.ServiceUnavailable);
        var client = new SecEdgarClient(_httpClient, _options, _loggerMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetStreamAsync("https://test.com/api"));

        exception.Message.Should().Contain("Exceeded maximum retry count");
        _mockHandler.RequestCount.Should().Be(3);
    }
}
