using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.UnitTests.Models.Fillings;

public class EdgarSearchResultTests
{
    [Fact(DisplayName = "EdgarSearchResult inherits from EdgarFiling")]
    public void EdgarSearchResult_InheritsFromEdgarFiling()
    {
        var result = new EdgarSearchResult();

        result.Should().BeAssignableTo<EdgarFiling>();
    }

    [Fact(DisplayName = "EdgarSearchResult properties can be set and retrieved")]
    public void EdgarSearchResult_PropertiesSetAndGet()
    {
        var filingDate = new DateTime(2024, 1, 15);
        var result = new EdgarSearchResult
        {
            Filing = "10-Q",
            DocumentsUrl = "https://www.sec.gov/documents",
            Description = "Quarterly Report",
            FilingDate = filingDate,
            InteractiveDataUrl = "https://www.sec.gov/interactive-data"
        };

        result.Filing.Should().Be("10-Q");
        result.DocumentsUrl.Should().Be("https://www.sec.gov/documents");
        result.Description.Should().Be("Quarterly Report");
        result.FilingDate.Should().Be(filingDate);
        result.InteractiveDataUrl.Should().Be("https://www.sec.gov/interactive-data");

        // Explicit access to ensure coverage tool picks it up
        var url = result.InteractiveDataUrl;
        url.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "EdgarSearchResult InteractiveDataUrl can be null")]
    public void EdgarSearchResult_InteractiveDataUrl_CanBeNull()
    {
        var result = new EdgarSearchResult
        {
            InteractiveDataUrl = null
        };

        result.InteractiveDataUrl.Should().BeNull();
    }
}
