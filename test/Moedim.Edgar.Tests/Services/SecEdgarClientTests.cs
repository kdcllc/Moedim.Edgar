using Microsoft.Extensions.Logging;
using Moedim.Edgar.Client.Impl;
using Moedim.Edgar.Options;

namespace Moedim.Edgar.Tests.Services;

/// <summary>
/// Unit tests for SecEdgarClient
/// </summary>
public class SecEdgarClientTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<SecEdgarClient>> _loggerMock;
    private readonly SecEdgarOptions _options;

    public SecEdgarClientTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<SecEdgarClient>>();

        _options = new SecEdgarOptions
        {
            UserAgent = "Test App test@example.com",
            RequestDelay = TimeSpan.Zero,
            TimeoutDelay = TimeSpan.FromMilliseconds(10),
            MaxRetryCount = 3
        };
    }

    [Fact]
    public void Constructor_WithNullHttpClientFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SecEdgarClient(null!, _options, _loggerMock.Object));

        exception.ParamName.Should().Be("httpClientFactory");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SecEdgarClient(_httpClientFactoryMock.Object, null!, _loggerMock.Object));

        exception.ParamName.Should().Be("options");
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var client = new SecEdgarClient(_httpClientFactoryMock.Object, _options, _loggerMock.Object);

        // Assert
        client.Should().NotBeNull();
    }
}
