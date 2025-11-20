using Microsoft.Extensions.Options;
using Moedim.Edgar.Client;
using Moedim.Edgar.Models.Data;
using Moedim.Edgar.Options;
using Moedim.Edgar.Services.Impl;

namespace Moedim.Edgar.UnitTests.Services;

public class CompanyConceptServiceTests : IDisposable
{
    private readonly Mock<ISecEdgarClient> _clientMock;
    private readonly IOptions<SecEdgarOptions> _options;
    private readonly CompanyConceptService _service;

    public CompanyConceptServiceTests()
    {
        _clientMock = new Mock<ISecEdgarClient>();
        var options = new SecEdgarOptions
        {
            BaseApiUrl = "https://data.sec.gov",
            UserAgent = "TestApp/1.0.0 (test@example.com)"
        };
        _options = Microsoft.Extensions.Options.Options.Create(options);
        _service = new CompanyConceptService(_clientMock.Object, _options);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    [Fact(DisplayName = "Constructor with null client throws ArgumentNullException")]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CompanyConceptService(null!, _options));

        exception.ParamName.Should().Be("client");
    }

    [Fact(DisplayName = "Constructor with null options throws ArgumentNullException")]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CompanyConceptService(_clientMock.Object, null!));

        exception.ParamName.Should().Be("options");
    }

    [Fact(DisplayName = "QueryAsync with null tag throws ArgumentNullException")]
    public async Task QueryAsync_NullTag_ThrowsArgumentNullException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.QueryAsync(123456, null!));

        exception.ParamName.Should().Be("tag");
    }

    [Fact(DisplayName = "QueryAsync with empty tag throws ArgumentException")]
    public async Task QueryAsync_EmptyTag_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.QueryAsync(123456, ""));

        exception.ParamName.Should().Be("tag");
        exception.Message.Should().Contain("Tag cannot be empty");
    }

    [Fact(DisplayName = "QueryAsync with whitespace tag throws ArgumentException")]
    public async Task QueryAsync_WhitespaceTag_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.QueryAsync(123456, "   "));

        exception.ParamName.Should().Be("tag");
    }

    [Fact(DisplayName = "QueryAsync with zero CIK throws ArgumentOutOfRangeException")]
    public async Task QueryAsync_ZeroCik_ThrowsArgumentOutOfRangeException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.QueryAsync(0, "Assets"));

        exception.ParamName.Should().Be("cik");
        exception.Message.Should().Contain("CIK must be positive");
    }

    [Fact(DisplayName = "QueryAsync with negative CIK throws ArgumentOutOfRangeException")]
    public async Task QueryAsync_NegativeCik_ThrowsArgumentOutOfRangeException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.QueryAsync(-1, "Assets"));

        exception.ParamName.Should().Be("cik");
    }

    [Fact(DisplayName = "QueryAsync formats CIK with leading zeros correctly")]
    public async Task QueryAsync_ValidInput_FormatsCikWithLeadingZeros()
    {
        var mockJson = @"{
            ""cik"": 123,
            ""taxonomy"": ""us-gaap"",
            ""tag"": ""Assets"",
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
        }";

        _clientMock.Setup(x => x.GetAsync(
                It.Is<string>(url => url.Contains("CIK0000000123")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockJson);

        var result = await _service.QueryAsync(123, "Assets");

        result.Should().NotBeNull();
        _clientMock.Verify(x => x.GetAsync(
            "https://data.sec.gov/companyconcept/CIK0000000123/us-gaap/Assets.json",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "QueryAsync with valid parameters returns CompanyConceptQuery")]
    public async Task QueryAsync_ValidParameters_ReturnsCompanyConceptQuery()
    {
        var mockJson = @"{
            ""cik"": 320193,
            ""taxonomy"": ""us-gaap"",
            ""tag"": ""Assets"",
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
        }";

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockJson);

        var result = await _service.QueryAsync(320193, "Assets");

        result.Should().NotBeNull();
        result.CIK.Should().Be(320193);
        result.Result.Should().NotBeNull();
    }

    [Fact(DisplayName = "QueryAsync respects cancellation token")]
    public async Task QueryAsync_WithCancellationToken_RespectsToken()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _clientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _service.QueryAsync(320193, "Assets", cts.Token));
    }
}
