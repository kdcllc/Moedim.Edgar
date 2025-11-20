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

    [Fact(DisplayName = "SearchAsync with ownership filter include uses correct parameter")]
    public async Task SearchAsync_OwnershipFilterInclude_UsesCorrectParameter()
    {
        var query = new EdgarLatestFilingsQuery
        {
            OwnershipFilter = EdgarSearchOwnershipFilter.Include
        };
        var mockHtml = "<html>no matching filings</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        await _service.SearchAsync(query);

        _clientMock.Verify(x => x.GetAsync(
            It.Is<string>(url => url.Contains("owner=include")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = "SearchAsync constructs URL with all results per page options")]
    [InlineData(EdgarSearchResultsPerPage.Entries10, 10)]
    [InlineData(EdgarSearchResultsPerPage.Entries20, 20)]
    [InlineData(EdgarSearchResultsPerPage.Entries40, 40)]
    [InlineData(EdgarSearchResultsPerPage.Entries80, 80)]
    [InlineData(EdgarSearchResultsPerPage.Entries100, 100)]
    public async Task SearchAsync_AllResultsPerPageOptions_ConstructsCorrectUrl(
        EdgarSearchResultsPerPage resultsPerPage, int expectedCount)
    {
        var query = new EdgarLatestFilingsQuery
        {
            ResultsPerPage = resultsPerPage
        };
        var mockHtml = "<html>no matching filings</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        await _service.SearchAsync(query);

        _clientMock.Verify(x => x.GetAsync(
            It.Is<string>(url => url.Contains($"count={expectedCount}")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SearchAsync with whitespace form type does not add type parameter")]
    public async Task SearchAsync_WhitespaceFormType_DoesNotAddTypeParameter()
    {
        var query = new EdgarLatestFilingsQuery
        {
            FormType = "   "
        };
        var mockHtml = "<html>no matching filings</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        await _service.SearchAsync(query);

        _clientMock.Verify(x => x.GetAsync(
            It.Is<string>(url => !url.Contains("type=")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SearchAsync parses single filing result successfully")]
    public async Task SearchAsync_ValidHtml_ParsesSingleFilingSuccessfully()
    {
        var query = new EdgarLatestFilingsQuery();
        var mockHtml = @"
<html>
<body>
File/Film No
<table>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar"">Company CIK</a></td></tr>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193"">Apple Inc (0000320193)</a></td></tr>
<tr nowrap><td>10-Q</td><td><a href=""/Archives/edgar/data/320193/000032019323000077/0000320193-23-000077-index.htm"">Documents</a></td><td>Quarterly Report</td><td></td><td>2023-11-03</td></tr>
</table>
</body>
</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.SearchAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].EntityTitle.Should().Be("Apple Inc");
        result[0].EntityCik.Should().Be(320193);
        result[0].Filing.Should().Be("10-Q");
        result[0].DocumentsUrl.Should().Be("https://www.sec.gov/Archives/edgar/data/320193/000032019323000077/0000320193-23-000077-index.htm");
        result[0].Description.Should().Be("Quarterly Report");
        result[0].FilingDate.Should().Be(new DateTime(2023, 11, 3));
    }

    [Fact(DisplayName = "SearchAsync parses multiple filing results successfully")]
    public async Task SearchAsync_ValidHtmlMultipleResults_ParsesAllFilingsSuccessfully()
    {
        var query = new EdgarLatestFilingsQuery();
        var mockHtml = @"
<html>
<body>
File/Film No
<table>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar"">Company CIK</a></td></tr>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar?action=getcompany&CIK=0000789019"">Microsoft Corp (0000789019)</a></td></tr>
<tr nowrap><td>8-K</td><td><a href=""/Archives/edgar/data/789019/000119312523123456/d123456d8k.htm"">Documents</a></td><td>Current Report</td><td></td><td>2023-05-15</td></tr>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar?action=getcompany&CIK=0001018724"">Amazon.com Inc (0001018724)</a></td></tr>
<tr nowrap><td>10-K</td><td><a href=""/Archives/edgar/data/1018724/000101872423000001/amzn-20221231.htm"">Documents</a></td><td>Annual Report<br>&nbsp;</td><td></td><td>2023-02-03</td></tr>
</table>
</body>
</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.SearchAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        result[0].EntityTitle.Should().Be("Microsoft Corp");
        result[0].EntityCik.Should().Be(789019);
        result[0].Filing.Should().Be("8-K");
        result[0].Description.Should().Be("Current Report");
        result[0].FilingDate.Should().Be(new DateTime(2023, 5, 15));

        result[1].EntityTitle.Should().Be("Amazon.com Inc");
        result[1].EntityCik.Should().Be(1018724);
        result[1].Filing.Should().Be("10-K");
        result[1].Description.Should().Be("Annual Report  "); // <br> and &nbsp; replaced with spaces
        result[1].FilingDate.Should().Be(new DateTime(2023, 2, 3));
    }

    [Fact(DisplayName = "SearchAsync handles entity title without CIK parentheses")]
    public async Task SearchAsync_EntityTitleWithoutCik_ParsesCorrectly()
    {
        var query = new EdgarLatestFilingsQuery();
        var mockHtml = @"
<html>
<body>
File/Film No
<table>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar"">Company CIK</a></td></tr>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193"">Apple Inc</a></td></tr>
<tr nowrap><td>10-Q</td><td><a href=""/Archives/edgar/data/320193/000032019323000077/0000320193-23-000077-index.htm"">Documents</a></td><td>Quarterly Report</td><td></td><td>2023-11-03</td></tr>
</table>
</body>
</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.SearchAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].EntityCik.Should().Be(0);
    }

    [Fact(DisplayName = "SearchAsync handles invalid CIK in parentheses")]
    public async Task SearchAsync_InvalidCikInParentheses_DoesNotParseCik()
    {
        var query = new EdgarLatestFilingsQuery();
        var mockHtml = @"
<html>
<body>
File/Film No
<table>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar"">Company CIK</a></td></tr>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193"">Apple Inc (INVALID)</a></td></tr>
<tr nowrap><td>10-Q</td><td><a href=""/Archives/edgar/data/320193/000032019323000077/0000320193-23-000077-index.htm"">Documents</a></td><td>Quarterly Report</td><td></td><td>2023-11-03</td></tr>
</table>
</body>
</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.SearchAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].EntityTitle.Should().Be("Apple Inc");
        result[0].EntityCik.Should().Be(0);
    }

    [Fact(DisplayName = "SearchAsync handles missing documents URL")]
    public async Task SearchAsync_MissingDocumentsUrl_ParsesOtherFieldsCorrectly()
    {
        var query = new EdgarLatestFilingsQuery();
        var mockHtml = @"
<html>
<body>
File/Film No
<table>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar"">Company CIK</a></td></tr>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193"">Apple Inc (0000320193)</a></td></tr>
<tr nowrap><td>10-Q</td><td>No Link</td><td>Quarterly Report</td><td></td><td>2023-11-03</td></tr>
</table>
</body>
</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.SearchAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].DocumentsUrl.Should().BeNull();
        result[0].Filing.Should().Be("10-Q");
    }

    [Fact(DisplayName = "SearchAsync handles invalid filing date")]
    public async Task SearchAsync_InvalidFilingDate_DoesNotSetDate()
    {
        var query = new EdgarLatestFilingsQuery();
        var mockHtml = @"
<html>
<body>
File/Film No
<table>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar"">Company CIK</a></td></tr>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193"">Apple Inc (0000320193)</a></td></tr>
<tr nowrap><td>10-Q</td><td><a href=""/Archives/edgar/data/320193/000032019323000077/0000320193-23-000077-index.htm"">Documents</a></td><td>Quarterly Report</td><td></td><td>INVALID_DATE</td></tr>
</table>
</body>
</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.SearchAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].FilingDate.Should().Be(default(DateTime));
    }

    [Fact(DisplayName = "SearchAsync handles insufficient columns in row")]
    public async Task SearchAsync_InsufficientColumns_SkipsRow()
    {
        var query = new EdgarLatestFilingsQuery();
        var mockHtml = @"
<html>
<body>
File/Film No
<table>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193"">Apple Inc (0000320193)</a></td></tr>
<tr nowrap>
<td>10-Q</td>
<td>Only Two Columns</td>
</tr>
</table>
</body>
</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.SearchAsync(query);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact(DisplayName = "SearchAsync handles table without entity titles")]
    public async Task SearchAsync_TableWithoutEntityTitles_ReturnsEmptyArray()
    {
        var query = new EdgarLatestFilingsQuery();
        var mockHtml = @"
<html>
<body>
File/Film No
<table>
<tr nowrap>
<td>10-Q</td>
<td><a href=""/Archives/edgar/data/320193/000032019323000077/0000320193-23-000077-index.htm"">Documents</a></td>
<td>Quarterly Report</td>
<td></td>
<td></td>
<td>2023-11-03</td>
</tr>
</table>
</body>
</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.SearchAsync(query);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact(DisplayName = "SearchAsync handles description with HTML tags")]
    public async Task SearchAsync_DescriptionWithHtmlTags_CleansTags()
    {
        var query = new EdgarLatestFilingsQuery();
        var mockHtml = @"
<html>
<body>
File/Film No
<table>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar"">Company CIK</a></td></tr>
<tr><td bgcolor=""white""><a href=""/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193"">Apple Inc (0000320193)</a></td></tr>
<tr nowrap><td>10-Q</td><td><a href=""/Archives/edgar/data/320193/000032019323000077/0000320193-23-000077-index.htm"">Documents</a></td><td>Quarterly<br>Report&nbsp;Filed</td><td></td><td>2023-11-03</td></tr>
</table>
</body>
</html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.SearchAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Description.Should().Contain("Quarterly Report Filed");
    }

    [Fact(DisplayName = "Constructor with null logger works correctly")]
    public void Constructor_NullLogger_WorksCorrectly()
    {
        var service = new EdgarLatestFilingsService(_clientMock.Object, null);

        service.Should().NotBeNull();
    }
}
