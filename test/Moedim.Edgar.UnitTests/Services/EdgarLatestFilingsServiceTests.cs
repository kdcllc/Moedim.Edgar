using Microsoft.Extensions.Logging;
using Moedim.Edgar.Client;
using Moedim.Edgar.Models.Fillings;
using Moedim.Edgar.Services.Impl;

namespace Moedim.Edgar.UnitTests.Services;

public class EdgarLatestFilingsServiceTests : IDisposable
{
    private readonly Mock<ISecEdgarClient> _clientMock;
    private readonly Mock<ILogger<EdgarLatestFilingsService>> _loggerMock;
    private readonly EdgarLatestFilingsService _service;

    public EdgarLatestFilingsServiceTests()
    {
        _clientMock = new Mock<ISecEdgarClient>();
        _loggerMock = new Mock<ILogger<EdgarLatestFilingsService>>();
        _service = new EdgarLatestFilingsService(_clientMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    [Fact(DisplayName = "Constructor with null client throws ArgumentNullException")]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new EdgarLatestFilingsService(null!, _loggerMock.Object));

        exception.ParamName.Should().Be("client");
    }

    [Fact(DisplayName = "SearchAsync with null query throws ArgumentNullException")]
    public async Task SearchAsync_NullQuery_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.SearchAsync(null!));
    }

    [Fact(DisplayName = "SearchAsync with no matching filings returns empty array")]
    public async Task SearchAsync_NoMatchingFilings_ReturnsEmptyArray()
    {
        var query = new EdgarLatestFilingsQuery
        {
            FormType = "8-K"
        };

        var mockHtml = "<html>no matching filings</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.SearchAsync(query);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact(DisplayName = "SearchAsync constructs URL correctly with form type")]
    public async Task SearchAsync_ConstructsUrlWithFormType()
    {
        var query = new EdgarLatestFilingsQuery
        {
            FormType = "10-Q",
            OwnershipFilter = EdgarSearchOwnershipFilter.Exclude,
            ResultsPerPage = EdgarSearchResultsPerPage.Entries20
        };

        var mockHtml = "<html>no matching filings</html>";
        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        await _service.SearchAsync(query);

        _clientMock.Verify(x => x.GetAsync(
            It.Is<string>(url =>
                url.Contains("type=10-Q") &&
                url.Contains("owner=exclude") &&
                url.Contains("count=20") &&
                url.Contains("action=getcurrent")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SearchAsync constructs URL with ownership filter only")]
    public async Task SearchAsync_ConstructsUrlWithOwnershipFilterOnly()
    {
        var query = new EdgarLatestFilingsQuery
        {
            OwnershipFilter = EdgarSearchOwnershipFilter.Only,
            ResultsPerPage = EdgarSearchResultsPerPage.Entries100
        };

        var mockHtml = "<html>no matching filings</html>";
        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        await _service.SearchAsync(query);

        _clientMock.Verify(x => x.GetAsync(
            It.Is<string>(url =>
                url.Contains("owner=only") &&
                url.Contains("count=100")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SearchAsync handles missing table gracefully")]
    public async Task SearchAsync_MissingTable_ReturnsEmptyArray()
    {
        var query = new EdgarLatestFilingsQuery();
        var mockHtml = "<html><body>Some content without table</body></html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.SearchAsync(query);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact(DisplayName = "SearchAsync respects cancellation token")]
    public async Task SearchAsync_WithCancellationToken_RespectsToken()
    {
        var query = new EdgarLatestFilingsQuery();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _service.SearchAsync(query, cts.Token));
    }

    [Fact(DisplayName = "SearchAsync with default results per page uses 40")]
    public async Task SearchAsync_DefaultResultsPerPage_Uses40()
    {
        var query = new EdgarLatestFilingsQuery();
        var mockHtml = "<html>no matching filings</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        await _service.SearchAsync(query);

        _clientMock.Verify(x => x.GetAsync(
            It.Is<string>(url => url.Contains("count=40")),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
