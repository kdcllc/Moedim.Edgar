using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.UnitTests.Models.Fillings;

public class EdgarSearchResponseTests
{
    [Fact(DisplayName = "EdgarSearchResponse properties can be set and retrieved")]
    public void EdgarSearchResponse_PropertiesSetAndGet()
    {
        var results = new EdgarSearchResult[]
        {
            new EdgarSearchResult { Filing = "10-K" },
            new EdgarSearchResult { Filing = "10-Q" }
        };

        var response = new EdgarSearchResponse
        {
            Results = results,
            NextPageUrl = "https://www.sec.gov/next-page"
        };

        response.Results.Should().HaveCount(2);
        response.Results[0].Filing.Should().Be("10-K");
        response.NextPageUrl.Should().Be("https://www.sec.gov/next-page");
    }

    [Fact(DisplayName = "EdgarSearchResponse has default empty array for Results")]
    public void EdgarSearchResponse_DefaultResults_IsEmptyArray()
    {
        var response = new EdgarSearchResponse();

        response.Results.Should().NotBeNull();
        response.Results.Should().BeEmpty();
    }

    [Fact(DisplayName = "EdgarSearchResponse HasNextPage returns true when NextPageUrl is set")]
    public void EdgarSearchResponse_HasNextPage_TrueWhenUrlSet()
    {
        var response = new EdgarSearchResponse
        {
            NextPageUrl = "https://www.sec.gov/next"
        };

        response.HasNextPage.Should().BeTrue();
    }

    [Fact(DisplayName = "EdgarSearchResponse HasNextPage returns false when NextPageUrl is null")]
    public void EdgarSearchResponse_HasNextPage_FalseWhenUrlNull()
    {
        var response = new EdgarSearchResponse
        {
            NextPageUrl = null
        };

        response.HasNextPage.Should().BeFalse();
    }

    [Fact(DisplayName = "EdgarSearchResponse HasNextPage returns false when NextPageUrl is empty")]
    public void EdgarSearchResponse_HasNextPage_FalseWhenUrlEmpty()
    {
        var response = new EdgarSearchResponse
        {
            NextPageUrl = string.Empty
        };

        response.HasNextPage.Should().BeFalse();
    }

    [Fact(DisplayName = "EdgarSearchResponse HasNextPage returns true when NextPageUrl is whitespace")]
    public void EdgarSearchResponse_HasNextPage_TrueWhenUrlWhitespace()
    {
        var response = new EdgarSearchResponse
        {
            NextPageUrl = "   "
        };

        // Note: IsNullOrEmpty doesn't check for whitespace, so this returns true
        response.HasNextPage.Should().BeTrue();
    }
}
