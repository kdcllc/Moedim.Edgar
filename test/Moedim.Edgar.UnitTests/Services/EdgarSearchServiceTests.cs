using Microsoft.Extensions.Logging;
using Moedim.Edgar.Client;
using Moedim.Edgar.Models.Fillings;
using Moedim.Edgar.Services.Impl;

namespace Moedim.Edgar.UnitTests.Services;

public class EdgarSearchServiceTests : IDisposable
{
    private readonly Mock<ISecEdgarClient> _clientMock;
    private readonly Mock<ILogger<EdgarSearchService>> _loggerMock;
    private readonly EdgarSearchService _service;

    public EdgarSearchServiceTests()
    {
        _clientMock = new Mock<ISecEdgarClient>();
        _loggerMock = new Mock<ILogger<EdgarSearchService>>();
        _service = new EdgarSearchService(_clientMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    [Fact(DisplayName = "Constructor with null client throws ArgumentNullException")]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new EdgarSearchService(null!, _loggerMock.Object));

        exception.ParamName.Should().Be("client");
    }

    [Fact(DisplayName = "SearchAsync with null query throws ArgumentNullException")]
    public async Task SearchAsync_NullQuery_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.SearchAsync(null!));
    }

    [Fact(DisplayName = "SearchAsync with null symbol throws ArgumentException")]
    public async Task SearchAsync_NullSymbol_ThrowsArgumentException()
    {
        var query = new EdgarSearchQuery { Symbol = null! };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SearchAsync(query));

        exception.ParamName.Should().Be("query");
        exception.Message.Should().Contain("Symbol is required");
    }

    [Fact(DisplayName = "SearchAsync with empty symbol throws ArgumentException")]
    public async Task SearchAsync_EmptySymbol_ThrowsArgumentException()
    {
        var query = new EdgarSearchQuery { Symbol = "" };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SearchAsync(query));

        exception.ParamName.Should().Be("query");
    }

    [Fact(DisplayName = "SearchAsync with no matching ticker throws InvalidOperationException")]
    public async Task SearchAsync_NoMatchingTicker_ThrowsInvalidOperationException()
    {
        var query = new EdgarSearchQuery { Symbol = "INVALID" };
        var mockHtml = "<html>No matching Ticker Symbol.</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SearchAsync(query));

        exception.Message.Should().Contain("No matching Ticker Symbol");
    }

    [Fact(DisplayName = "SearchAsync with valid query returns results")]
    public async Task SearchAsync_ValidQuery_ReturnsResults()
    {
        var query = new EdgarSearchQuery
        {
            Symbol = "AAPL",
            FilingType = "10-K"
        };

        var mockHtml = @"
            <table id='tableFile2'>
                <tr><th>Filings</th></tr>
                <tr><th>Format</th></tr>
                <tr>
                    <td>10-K</td>
                    <td><a id='documentsbutton' href='/Archives/edgar/data/320193/000032019323000001/index.html'>Documents</a>
                        <a id='interactiveDataBtn' href='/cgi-bin/viewer?action=view&amp;cik=320193'>Interactive Data</a>
                    </td>
                    <td>Annual Report</td>
                    <td>2023-11-03</td>
                </tr>
            </table>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.SearchAsync(query);

        result.Should().NotBeNull();
        result.Results.Should().NotBeNull();
        result.Results.Should().HaveCountGreaterThan(0);
    }

    [Fact(DisplayName = "SearchAsync constructs URL correctly with all parameters")]
    public async Task SearchAsync_ConstructsUrlWithAllParameters()
    {
        var query = new EdgarSearchQuery
        {
            Symbol = "MSFT",
            FilingType = "10-Q",
            PriorTo = new DateTime(2023, 12, 31),
            OwnershipFilter = EdgarSearchOwnershipFilter.Include,
            ResultsPerPage = EdgarSearchResultsPerPage.Entries80
        };

        var mockHtml = "<table id='tableFile2'></table>";
        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        await _service.SearchAsync(query);

        _clientMock.Verify(x => x.GetAsync(
            It.Is<string>(url =>
                url.Contains("CIK=MSFT") &&
                url.Contains("type=10-Q") &&
                url.Contains("dateb=20231231") &&
                url.Contains("owner=include") &&
                url.Contains("count=80")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SearchAsync parses next page URL correctly")]
    public async Task SearchAsync_ParsesNextPageUrl()
    {
        var query = new EdgarSearchQuery { Symbol = "AAPL" };

        var mockHtml = @"
            <table id='tableFile2'></table>
            <input type=""button"" value=""Next 40"" onclick=""window.location='/cgi-bin/browse-edgar?action=getcompany&CIK=AAPL&start=40'"">";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.SearchAsync(query);

        result.NextPageUrl.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "NextPageAsync with null URL throws ArgumentException")]
    public async Task NextPageAsync_NullUrl_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.NextPageAsync(null!));

        exception.ParamName.Should().Be("nextPageUrl");
        exception.Message.Should().Contain("Next page URL is required");
    }

    [Fact(DisplayName = "NextPageAsync with empty URL throws ArgumentException")]
    public async Task NextPageAsync_EmptyUrl_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.NextPageAsync(""));

        exception.ParamName.Should().Be("nextPageUrl");
    }

    [Fact(DisplayName = "NextPageAsync with valid URL returns results")]
    public async Task NextPageAsync_ValidUrl_ReturnsResults()
    {
        var mockHtml = @"
            <table id='tableFile2'>
                <tr><th>Filings</th></tr>
                <tr><th>Format</th></tr>
                <tr>
                    <td>10-Q</td>
                    <td><a id='documentsbutton' href='/Archives/edgar/data/320193/000032019323000002/index.html'>Documents</a></td>
                    <td>Quarterly Report</td>
                    <td>2023-08-04</td>
                </tr>
            </table>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.NextPageAsync("https://www.sec.gov/cgi-bin/browse-edgar?start=40");

        result.Should().NotBeNull();
        result.Results.Should().NotBeNull();
    }

    [Fact(DisplayName = "SearchAsync respects cancellation token")]
    public async Task SearchAsync_WithCancellationToken_RespectsToken()
    {
        var query = new EdgarSearchQuery { Symbol = "AAPL" };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _service.SearchAsync(query, cts.Token));
    }

    [Fact(DisplayName = "NextPageAsync respects cancellation token")]
    public async Task NextPageAsync_WithCancellationToken_RespectsToken()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _service.NextPageAsync("https://www.sec.gov/cgi-bin/browse-edgar?start=40", cts.Token));
    }
}
