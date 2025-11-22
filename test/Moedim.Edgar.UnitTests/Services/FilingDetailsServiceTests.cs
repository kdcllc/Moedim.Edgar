using Microsoft.Extensions.Logging;
using Moedim.Edgar.Client;
using Moedim.Edgar.Services;
using Moedim.Edgar.Services.Impl;

namespace Moedim.Edgar.UnitTests.Services;

public class FilingDetailsServiceTests : IDisposable
{
    private readonly Mock<ISecEdgarClient> _clientMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<FilingDetailsService>> _loggerMock;
    private readonly FilingDetailsService _service;

    public FilingDetailsServiceTests()
    {
        _clientMock = new Mock<ISecEdgarClient>();
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<FilingDetailsService>>();
        _service = new FilingDetailsService(_clientMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    [Fact(DisplayName = "Constructor with null client throws ArgumentNullException")]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new FilingDetailsService(null!, _cacheMock.Object, _loggerMock.Object));

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
                <table>
                <tr><th>Seq</th><th>Description</th><th>Document</th><th>Type</th><th>Size</th></tr>
                <tr><th></th></tr>
                <tr><td>1</td><td>10-K</td><td><a href=""/Archives/edgar/data/320193/000032019323000106/aapl-20230930.htm"">aapl-20230930.htm</a></td><td>10-K</td><td>1000000</td></tr>
                </table>
                Data Files
                <table>
                <tr><th>Seq</th><th>Description</th><th>Document</th><th>Type</th><th>Size</th></tr>
                <tr><th></th></tr>
                <tr><td>2</td><td>XBRL INSTANCE DOCUMENT</td><td><a href=""/Archives/edgar/data/320193/000032019323000106/aapl-20230930.xml"">aapl-20230930.xml</a></td><td>EX-101.INS</td><td>500000</td></tr>
                </table>
            </body>
            </html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetFilingDetailsAsync("https://www.sec.gov/test.html");

        result.Should().NotBeNull();
        result.Form.Should().Be("10-K");
        result.EntityCik.Should().Be(320193);
        result.EntityName.Should().Be("Apple Inc.");
        result.AccessionNumberP1.Should().Be(320193);
        result.AccessionNumberP2.Should().Be(23);
        result.AccessionNumberP3.Should().Be(106);
        result.FilingDate.Should().Be(new DateTime(2023, 11, 3));
        result.PeriodOfReport.Should().Be(new DateTime(2023, 9, 30));
        result.Accepted.Should().Be(new DateTime(2023, 11, 3, 18, 4, 1));
        result.DocumentFormatFiles.Should().HaveCount(1);
        result.DocumentFormatFiles![0].Sequence.Should().Be(1);
        result.DocumentFormatFiles[0].Description.Should().Be("10-K");
        result.DocumentFormatFiles[0].DocumentName.Should().Be("aapl-20230930.htm");
        result.DocumentFormatFiles[0].Url.Should().Be("https://www.sec.gov//Archives/edgar/data/320193/000032019323000106/aapl-20230930.htm");
        result.DocumentFormatFiles[0].DocumentType.Should().Be("10-K");
        result.DocumentFormatFiles[0].Size.Should().Be(1000000);
        result.DataFiles.Should().HaveCount(1);
        result.DataFiles![0].Sequence.Should().Be(2);
        result.DataFiles[0].Description.Should().Be("XBRL INSTANCE DOCUMENT");
    }

    [Fact(DisplayName = "GetDocumentFormatFilesAsync parses documents correctly")]
    public async Task GetDocumentFormatFilesAsync_ValidHtml_ParsesDocuments()
    {
        var mockHtml = @"
            Document Format Files
            <table>
            <tr><th>Seq</th><th>Description</th><th>Document</th><th>Type</th><th>Size</th></tr>
            <tr><th></th></tr>
            <tr><td>1</td><td>10-K</td><td><a href=""/Archives/edgar/data/320193/test.htm"">test.htm</a></td><td>10-K</td><td>1000</td></tr>
            <tr><td>2</td><td>Exhibit</td><td><a href=""/test2.htm"">test2.htm</a></td><td>&nbsp;</td><td>2000</td></tr>
            </table>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetDocumentFormatFilesAsync("https://www.sec.gov/test.html");

        result.Should().HaveCount(2);
        result[0].Sequence.Should().Be(1);
        result[0].Description.Should().Be("10-K");
        result[0].DocumentType.Should().Be("10-K");
        result[1].Sequence.Should().Be(2);
        result[1].DocumentType.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetDataFilesAsync parses data files correctly")]
    public async Task GetDataFilesAsync_ValidHtml_ParsesDataFiles()
    {
        var mockHtml = @"
            Data Files
            <table>
            <tr><th>Header</th></tr>
            <tr><th>Header2</th></tr>
            <tr><td>1</td><td>XBRL Instance</td><td><a href=""/data.xml"">data.xml</a></td><td>EX-101.INS</td><td>5000</td></tr>
            </table>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetDataFilesAsync("https://www.sec.gov/test.html");

        result.Should().HaveCount(1);
        result[0].DocumentType.Should().Be("EX-101.INS");
    }

    [Fact(DisplayName = "DownloadXbrlDocumentAsync finds instance document by description")]
    public async Task DownloadXbrlDocumentAsync_FindsByDescription_ReturnsStream()
    {
        var mockHtml = @"
            Data Files
            <table>
            <tr><th>Header</th></tr>
            <tr><th>Header2</th></tr>
            <tr><td>1</td><td>XBRL INSTANCE DOCUMENT</td><td><a href=""/data.xml"">data.xml</a></td><td>EX-101.INS</td><td>5000</td></tr>
            </table>";

        var mockStream = new MemoryStream();
        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);
        _clientMock.Setup(x => x.GetStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockStream);

        var result = await _service.DownloadXbrlDocumentAsync("https://www.sec.gov/test.html");

        result!.Should().BeSameAs(mockStream);
    }

    [Fact(DisplayName = "DownloadXbrlDocumentAsync finds instance document by type")]
    public async Task DownloadXbrlDocumentAsync_FindsByType_ReturnsStream()
    {
        var mockHtml = @"
            Data Files
            <table>
            <tr><th>Header</th></tr>
            <tr><th>Header2</th></tr>
            <tr><td>1</td><td>Schema</td><td><a href=""/schema.xsd"">schema.xsd</a></td><td>EX-101.SCH</td><td>3000</td></tr>
            <tr><td>2</td><td>Data File</td><td><a href=""/data.xml"">data.xml</a></td><td>EX-101.INS</td><td>5000</td></tr>
            </table>";

        var mockStream = new MemoryStream();
        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);
        _clientMock.Setup(x => x.GetStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockStream);

        var result = await _service.DownloadXbrlDocumentAsync("https://www.sec.gov/test.html");

        result!.Should().BeSameAs(mockStream);
    }

    [Fact(DisplayName = "DownloadXbrlDocumentAsync throws when instance document not found")]
    public async Task DownloadXbrlDocumentAsync_NoInstanceDocument_ThrowsInvalidOperationException()
    {
        var mockHtml = @"
            Data Files
            <table>
            <tr><th>Header</th></tr>
            <tr><th>Header2</th></tr>
            <tr><td>1</td><td>Schema</td><td><a href=""/schema.xsd"">schema.xsd</a></td><td>EX-101.SCH</td><td>3000</td></tr>
            </table>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DownloadXbrlDocumentAsync("https://www.sec.gov/test.html"));

        exception.Message.Should().Contain("Unable to find XBRL Instance Document");
    }

    [Fact(DisplayName = "GetCikFromFilingAsync handles parsing errors")]
    public async Task GetCikFromFilingAsync_ParsingError_ThrowsInvalidOperationException()
    {
        var mockHtml = @"
            <html>
            <body>
                <acronym title=""Central Index Key"">CIK</acronym>
                Invalid structure
            </body>
            </html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetCikFromFilingAsync("https://www.sec.gov/test.html"));

        exception.Message.Should().Contain("Fatal error while trying to find CIK");
    }

    [Fact(DisplayName = "GetFilingDetailsAsync handles missing optional fields gracefully")]
    public async Task GetFilingDetailsAsync_MissingFields_ReturnsPartialDetails()
    {
        var mockHtml = @"
            <html>
            <body>
                <div>Some content</div>
            </body>
            </html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetFilingDetailsAsync("https://www.sec.gov/test.html");

        result.Should().NotBeNull();
        result.Form.Should().BeEmpty();
        result.EntityCik.Should().Be(0);
    }

    [Fact(DisplayName = "GetFilingDetailsAsync handles invalid accession number format")]
    public async Task GetFilingDetailsAsync_InvalidAccessionNumber_HandlesGracefully()
    {
        var mockHtml = @"
            <html>
            <body>
                <div id=""secNum""><strong>Accession Number:</strong> INVALID</div>
            </body>
            </html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetFilingDetailsAsync("https://www.sec.gov/test.html");

        result.Should().NotBeNull();
        result.AccessionNumberP1.Should().Be(0);
    }

    [Fact(DisplayName = "GetFilingDetailsAsync handles invalid date formats")]
    public async Task GetFilingDetailsAsync_InvalidDates_UsesDefaultValues()
    {
        var mockHtml = @"
            <html>
            <body>
                <div class=""infoHead"">Filing Date</div><div><strong>INVALID</strong></div>
                <div class=""infoHead"">Period of Report</div><div><strong>INVALID</strong></div>
                <div class=""infoHead"">Accepted</div><div><strong>INVALID</strong></div>
            </body>
            </html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetFilingDetailsAsync("https://www.sec.gov/test.html");

        result.Should().NotBeNull();
        result.FilingDate.Should().Be(default(DateTime));
        result.PeriodOfReport.Should().Be(default(DateTime));
        result.Accepted.Should().Be(default(DateTime));
    }

    [Fact(DisplayName = "GetFilingDetailsAsync handles malformed tables")]
    public async Task GetFilingDetailsAsync_MalformedTables_HandlesGracefully()
    {
        var mockHtml = @"
            <html>
            <body>
                Document Format Files
                <table>
                <tr><td>incomplete</td></tr>
                </table>
                Data Files
                <table>
                </table>
            </body>
            </html>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetFilingDetailsAsync("https://www.sec.gov/test.html");

        result.Should().NotBeNull();
        result.DocumentFormatFiles.Should().BeEmpty();
        result.DataFiles.Should().BeEmpty();
    }

    [Fact(DisplayName = "Constructor with null logger works correctly")]
    public void Constructor_NullLogger_WorksCorrectly()
    {
        var service = new FilingDetailsService(_clientMock.Object, _cacheMock.Object, null);

        service.Should().NotBeNull();
    }

    [Fact(DisplayName = "GetFilingDetailsAsync respects cancellation token")]
    public async Task GetFilingDetailsAsync_WithCancellationToken_RespectsToken()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _service.GetFilingDetailsAsync("https://www.sec.gov/test.html", cts.Token));
    }

    [Fact(DisplayName = "ParseDocumentsTable handles rows with insufficient columns")]
    public async Task GetDocumentFormatFilesAsync_InsufficientColumns_SkipsRow()
    {
        var mockHtml = @"
            Document Format Files
            <table>
            <tr><th>Header</th></tr>
            <tr><th>Header2</th></tr>
            <tr><td>1</td><td>Only</td><td>Three</td></tr>
            <tr><td>2</td><td>Valid</td><td><a href=""/test.htm"">test.htm</a></td><td>10-K</td><td>1000</td><td>Extra</td></tr>
            </table>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetDocumentFormatFilesAsync("https://www.sec.gov/test.html");

        result.Should().HaveCount(1);
        result[0].Sequence.Should().Be(2);
    }

    [Fact(DisplayName = "ParseDocumentsTable handles invalid sequence numbers")]
    public async Task GetDocumentFormatFilesAsync_InvalidSequence_UsesDefault()
    {
        var mockHtml = @"
            Document Format Files
            <table>
            <tr><th>Header</th></tr>
            <tr><th>Header2</th></tr>
            <tr><td>INVALID</td><td>Test</td><td><a href=""/test.htm"">test.htm</a></td><td>10-K</td><td>1000</td></tr>
            </table>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetDocumentFormatFilesAsync("https://www.sec.gov/test.html");

        result.Should().HaveCount(1);
        result[0].Sequence.Should().Be(0);
    }

    [Fact(DisplayName = "ParseDocumentsTable handles invalid size")]
    public async Task GetDocumentFormatFilesAsync_InvalidSize_UsesDefault()
    {
        var mockHtml = @"
            Document Format Files
            <table>
            <tr><th>Header</th></tr>
            <tr><th>Header2</th></tr>
            <tr><td>1</td><td>Test</td><td><a href=""/test.htm"">test.htm</a></td><td>10-K</td><td>INVALID</td></tr>
            </table>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetDocumentFormatFilesAsync("https://www.sec.gov/test.html");

        result.Should().HaveCount(1);
        result[0].Size.Should().Be(0);
    }

    [Fact(DisplayName = "ParseDocumentsTable handles missing href")]
    public async Task GetDocumentFormatFilesAsync_MissingHref_HandlesGracefully()
    {
        var mockHtml = @"
            Document Format Files
            <table>
            <tr><th>Header</th></tr>
            <tr><th>Header2</th></tr>
            <tr><td>1</td><td>Test</td><td>No link here</td><td>10-K</td><td>1000</td></tr>
            </table>";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHtml);

        var result = await _service.GetDocumentFormatFilesAsync("https://www.sec.gov/test.html");

        result.Should().HaveCount(1);
        result[0].Url.Should().BeNull();
        result[0].DocumentName.Should().BeNull();
    }
}
