using Microsoft.Extensions.Logging;
using Moedim.Edgar.Client;
using Moedim.Edgar.Services.Impl;

namespace Moedim.Edgar.UnitTests.Services;

public class CompanyLookupServiceTests : IDisposable
{
    private readonly Mock<ISecEdgarClient> _clientMock;
    private readonly Mock<ILogger<CompanyLookupService>> _loggerMock;
    private readonly CompanyLookupService _service;

    public CompanyLookupServiceTests()
    {
        _clientMock = new Mock<ISecEdgarClient>();
        _loggerMock = new Mock<ILogger<CompanyLookupService>>();
        _service = new CompanyLookupService(_clientMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    [Fact(DisplayName = "Constructor with null client throws ArgumentNullException")]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CompanyLookupService(null!, _loggerMock.Object));

        exception.ParamName.Should().Be("client");
    }

    [Fact(DisplayName = "GetCikFromSymbolAsync with null symbol throws ArgumentException")]
    public async Task GetCikFromSymbolAsync_NullSymbol_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetCikFromSymbolAsync(null!));

        exception.ParamName.Should().Be("symbol");
        exception.Message.Should().Contain("Symbol is required");
    }

    [Fact(DisplayName = "GetCikFromSymbolAsync with empty symbol throws ArgumentException")]
    public async Task GetCikFromSymbolAsync_EmptySymbol_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetCikFromSymbolAsync(""));

        exception.ParamName.Should().Be("symbol");
    }

    [Fact(DisplayName = "GetCikFromSymbolAsync with whitespace symbol throws ArgumentException")]
    public async Task GetCikFromSymbolAsync_WhitespaceSymbol_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetCikFromSymbolAsync("   "));

        exception.ParamName.Should().Be("symbol");
    }

    [Fact(DisplayName = "GetCikFromSymbolAsync with no matching ticker throws InvalidOperationException")]
    public async Task GetCikFromSymbolAsync_NoMatchingTicker_ThrowsInvalidOperationException()
    {
        var mockHtml = "<h1>No matching Ticker Symbol.</h1>";
        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetCikFromSymbolAsync("INVALID"));

        exception.Message.Should().Contain("No matching ticker symbol");
        exception.Message.Should().Contain("INVALID");
    }

    [Fact(DisplayName = "GetCikFromSymbolAsync unable to parse CIK throws InvalidOperationException")]
    public async Task GetCikFromSymbolAsync_UnableToParseoCik_ThrowsInvalidOperationException()
    {
        var mockHtml = "<html><body>Some content without proper CIK format</body></html>";
        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetCikFromSymbolAsync("AAPL"));

        exception.Message.Should().Contain("Unable to parse CIK");
        exception.Message.Should().Contain("AAPL");
    }

    [Fact(DisplayName = "GetCikFromSymbolAsync with valid symbol returns CIK")]
    public async Task GetCikFromSymbolAsync_ValidSymbol_ReturnsCik()
    {
        var mockHtml = @"
            <html>
            <body>
                <div>
                    <acronym title='Central Index Key'>CIK</acronym>
                    <a href='/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193'>0000320193 </a>
                </div>
            </body>
            </html>";

        _clientMock.Setup(x => x.GetAsync(
                It.Is<string>(url => url.Contains("CIK=AAPL")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetCikFromSymbolAsync("AAPL");

        result.Should().Be("0000320193");
        _clientMock.Verify(x => x.GetAsync(
            "https://www.sec.gov/cgi-bin/browse-edgar?CIK=AAPL&owner=exclude",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GetCikFromSymbolAsync respects cancellation token")]
    public async Task GetCikFromSymbolAsync_WithCancellationToken_RespectsToken()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _service.GetCikFromSymbolAsync("AAPL", cts.Token));
    }

    [Fact(DisplayName = "GetCikFromSymbolAsync constructs correct URL")]
    public async Task GetCikFromSymbolAsync_ConstructsCorrectUrl()
    {
        var mockHtml = @"
            <html>
            <body>
                <div>
                    <acronym title='Central Index Key'>CIK</acronym>
                    <a href='/cgi-bin/browse-edgar?action=getcompany&CIK=0001234567'>0001234567 </a>
                </div>
            </body>
            </html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        await _service.GetCikFromSymbolAsync("MSFT");

        _clientMock.Verify(x => x.GetAsync(
            "https://www.sec.gov/cgi-bin/browse-edgar?CIK=MSFT&owner=exclude",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
