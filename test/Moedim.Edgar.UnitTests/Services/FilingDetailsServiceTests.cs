using Microsoft.Extensions.Logging;
using Moedim.Edgar.Client;
using Moedim.Edgar.Services.Impl;

namespace Moedim.Edgar.UnitTests.Services;

public class FilingDetailsServiceTests : IDisposable
{
    private readonly Mock<ISecEdgarClient> _clientMock;
    private readonly Mock<ILogger<FilingDetailsService>> _loggerMock;
    private readonly FilingDetailsService _service;

    public FilingDetailsServiceTests()
    {
        _clientMock = new Mock<ISecEdgarClient>();
        _loggerMock = new Mock<ILogger<FilingDetailsService>>();
        _service = new FilingDetailsService(_clientMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    [Fact(DisplayName = "Constructor with null client throws ArgumentNullException")]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new FilingDetailsService(null!, _loggerMock.Object));

        exception.ParamName.Should().Be("client");
    }

    [Fact(DisplayName = "GetFilingDetailsAsync with null URL throws ArgumentException")]
    public async Task GetFilingDetailsAsync_NullUrl_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetFilingDetailsAsync(null!));

        exception.ParamName.Should().Be("documentsUrl");
    }

    [Fact(DisplayName = "GetFilingDetailsAsync with empty URL throws ArgumentException")]
    public async Task GetFilingDetailsAsync_EmptyUrl_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetFilingDetailsAsync(""));

        exception.ParamName.Should().Be("documentsUrl");
    }

    [Fact(DisplayName = "GetCikFromFilingAsync with valid URL returns CIK")]
    public async Task GetCikFromFilingAsync_ValidUrl_ReturnsCik()
    {
        var mockHtml = @"
            <html>
            <body>
                <div>
                    <acronym title=""Central Index Key"">CIK</acronym>
                    <a href='/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193'>0000320193 (Apple Inc.)</a>
                </div>
            </body>
            </html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetCikFromFilingAsync("https://www.sec.gov/Archives/edgar/data/320193/000032019323000106/index.html");

        result.Should().Be(320193);
    }

    [Fact(DisplayName = "GetCikFromFilingAsync with missing CIK throws InvalidOperationException")]
    public async Task GetCikFromFilingAsync_MissingCik_ThrowsInvalidOperationException()
    {
        var mockHtml = "<html><body>No CIK found</body></html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetCikFromFilingAsync("https://www.sec.gov/test.html"));

        exception.Message.Should().Contain("Unable to find CIK");
    }

    [Fact(DisplayName = "GetDocumentFormatFilesAsync with missing section throws InvalidOperationException")]
    public async Task GetDocumentFormatFilesAsync_MissingSection_ThrowsInvalidOperationException()
    {
        var mockHtml = "<html><body>No document format files section</body></html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetDocumentFormatFilesAsync("https://www.sec.gov/test.html"));

        exception.Message.Should().Contain("Unable to locate document format files");
    }

    [Fact(DisplayName = "GetDataFilesAsync with missing section throws InvalidOperationException")]
    public async Task GetDataFilesAsync_MissingSection_ThrowsInvalidOperationException()
    {
        var mockHtml = "<html><body>No data files section</body></html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetDataFilesAsync("https://www.sec.gov/test.html"));

        exception.Message.Should().Contain("Unable to locate data files");
    }

    [Fact(DisplayName = "DownloadDocumentAsync with null URL throws ArgumentException")]
    public async Task DownloadDocumentAsync_NullUrl_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.DownloadDocumentAsync(null!));

        exception.ParamName.Should().Be("documentUrl");
    }

    [Fact(DisplayName = "DownloadDocumentAsync with valid URL returns stream")]
    public async Task DownloadDocumentAsync_ValidUrl_ReturnsStream()
    {
        var mockStream = new MemoryStream();
        _clientMock.Setup(x => x.GetStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockStream);

        var result = await _service.DownloadDocumentAsync("https://www.sec.gov/document.xml");

        result.Should().NotBeNull();
        result.Should().BeSameAs(mockStream);
    }

    [Fact(DisplayName = "GetFilingDetailsAsync parses complete filing details")]
    public async Task GetFilingDetailsAsync_ValidHtml_ParsesAllDetails()
    {
        var mockHtml = @"
            <html>
            <body>
                <div id=""secNum""><strong>Accession Number:</strong> 0000320193-23-000106</div>
                <div class=""infoHead"">Type:</div><div><strong>10-K</strong></div>
                <div class=""infoHead"">Filing Date</div><div><strong>2023-11-03</strong></div>
                <div class=""infoHead"">Period of Report</div><div><strong>2023-09-30</strong></div>
                <div class=""infoHead"">Accepted</div><div><strong>2023-11-03 18:04:01</strong></div>
                <div class=""companyName"">Apple Inc. (Filer)
                    <acronym title=""Central Index Key"">CIK</acronym>
                    <a href='/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193'>0000320193 </a>
                </div>
                Document Format Files
                <table></table>
                Data Files
                <table></table>
            </body>
            </html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetFilingDetailsAsync("https://www.sec.gov/test.html");

        result.Should().NotBeNull();
        result.Form.Should().Be("10-K");
        result.EntityCik.Should().Be(320193);
        result.EntityName.Should().Be("Apple Inc.");
    }
}
