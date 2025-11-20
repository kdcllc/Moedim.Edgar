using Microsoft.Extensions.Options;
using Moedim.Edgar.Client;
using Moedim.Edgar.Models.Data;
using Moedim.Edgar.Options;
using Moedim.Edgar.Services.Impl;

namespace Moedim.Edgar.UnitTests.Services;

public class CompanyFactsServiceTests : IDisposable
{
    private readonly Mock<ISecEdgarClient> _clientMock;
    private readonly IOptions<SecEdgarOptions> _options;
    private readonly CompanyFactsService _service;

    public CompanyFactsServiceTests()
    {
        _clientMock = new Mock<ISecEdgarClient>();
        var options = new SecEdgarOptions
        {
            BaseApiUrl = "https://data.sec.gov",
            UserAgent = "TestApp/1.0.0 (test@example.com)"
        };
        _options = Microsoft.Extensions.Options.Options.Create(options);
        _service = new CompanyFactsService(_clientMock.Object, _options);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    [Fact(DisplayName = "Constructor with null client throws ArgumentNullException")]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CompanyFactsService(null!, _options));

        exception.ParamName.Should().Be("client");
    }

    [Fact(DisplayName = "Constructor with null options throws ArgumentNullException")]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CompanyFactsService(_clientMock.Object, null!));

        exception.ParamName.Should().Be("options");
    }

    [Fact(DisplayName = "QueryAsync with zero CIK throws ArgumentOutOfRangeException")]
    public async Task QueryAsync_ZeroCik_ThrowsArgumentOutOfRangeException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.QueryAsync(0));

        exception.ParamName.Should().Be("cik");
        exception.Message.Should().Contain("CIK must be positive");
    }

    [Fact(DisplayName = "QueryAsync with negative CIK throws ArgumentOutOfRangeException")]
    public async Task QueryAsync_NegativeCik_ThrowsArgumentOutOfRangeException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.QueryAsync(-1));

        exception.ParamName.Should().Be("cik");
    }

    [Fact(DisplayName = "QueryAsync formats CIK with leading zeros correctly")]
    public async Task QueryAsync_ValidInput_FormatsCikWithLeadingZeros()
    {
        var mockJson = @"{
            ""cik"": 456,
            ""entityName"": ""Test Company"",
            ""facts"": {
                ""us-gaap"": {
                    ""Assets"": {
                        ""label"": ""Assets"",
                        ""units"": {
                            ""USD"": [
                                {
                                    ""end"": ""2023-12-31"",
                                    ""val"": 1000000,
                                    ""accn"": ""0001234567-23-000001"",
                                    ""fy"": 2023,
                                    ""fp"": ""FY"",
                                    ""form"": ""10-K"",
                                    ""filed"": ""2024-02-15""
                                }
                            ]
                        }
                    }
                }
            }
        }";

        _clientMock.Setup(x => x.GetAsync(
                It.Is<string>(url => url.Contains("CIK0000000456")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockJson);

        var result = await _service.QueryAsync(456);

        result.Should().NotBeNull();
        _clientMock.Verify(x => x.GetAsync(
            "https://data.sec.gov/companyfacts/CIK0000000456.json",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "QueryAsync with valid parameters returns CompanyFactsQuery")]
    public async Task QueryAsync_ValidParameters_ReturnsCompanyFactsQuery()
    {
        var mockJson = @"{
            ""cik"": 320193,
            ""entityName"": ""Apple Inc."",
            ""facts"": {
                ""us-gaap"": {
                    ""Assets"": {
                        ""label"": ""Assets"",
                        ""units"": {
                            ""USD"": [
                                {
                                    ""end"": ""2023-09-30"",
                                    ""val"": 352755000000,
                                    ""accn"": ""0000320193-23-000106"",
                                    ""fy"": 2023,
                                    ""fp"": ""FY"",
                                    ""form"": ""10-K"",
                                    ""filed"": ""2023-11-03""
                                }
                            ]
                        }
                    }
                }
            }
        }";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockJson);

        var result = await _service.QueryAsync(320193);

        result.Should().NotBeNull();
        result.CIK.Should().Be(320193);
        result.EntityName.Should().Be("Apple Inc.");
    }

    [Fact(DisplayName = "QueryAsync respects cancellation token")]
    public async Task QueryAsync_WithCancellationToken_RespectsToken()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _service.QueryAsync(320193, cts.Token));
    }

    [Fact(DisplayName = "QueryAsync with 10-digit CIK formats correctly")]
    public async Task QueryAsync_TenDigitCik_FormatsCorrectly()
    {
        var mockJson = @"{
            ""cik"": 1234567890,
            ""entityName"": ""Test Company"",
            ""facts"": {}
        }";

        _clientMock.Setup(x => x.GetAsync(
                It.Is<string>(url => url.Contains("CIK1234567890")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockJson);

        var result = await _service.QueryAsync(1234567890);

        result.Should().NotBeNull();
        _clientMock.Verify(x => x.GetAsync(
            "https://data.sec.gov/companyfacts/CIK1234567890.json",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
