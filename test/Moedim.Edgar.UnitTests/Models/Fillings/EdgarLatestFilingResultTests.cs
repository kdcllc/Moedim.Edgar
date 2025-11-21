using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.UnitTests.Models.Fillings;

public class EdgarLatestFilingResultTests
{
    [Fact(DisplayName = "EdgarLatestFilingResult inherits from EdgarFiling")]
    public void EdgarLatestFilingResult_InheritsFromEdgarFiling()
    {
        var result = new EdgarLatestFilingResult();

        result.Should().BeAssignableTo<EdgarFiling>();
    }

    [Fact(DisplayName = "EdgarLatestFilingResult properties can be set and retrieved")]
    public void EdgarLatestFilingResult_PropertiesSetAndGet()
    {
        var filingDate = new DateTime(2024, 1, 20);
        var result = new EdgarLatestFilingResult
        {
            Filing = "8-K",
            DocumentsUrl = "https://www.sec.gov/documents",
            Description = "Current Report",
            FilingDate = filingDate,
            EntityTitle = "Apple Inc.",
            EntityCik = 320193
        };

        result.Filing.Should().Be("8-K");
        result.DocumentsUrl.Should().Be("https://www.sec.gov/documents");
        result.Description.Should().Be("Current Report");
        result.FilingDate.Should().Be(filingDate);
        result.EntityTitle.Should().Be("Apple Inc.");
        result.EntityCik.Should().Be(320193);

        // Explicit access to ensure coverage tool picks it up
        var title = result.EntityTitle;
        var cik = result.EntityCik;
        title.Should().NotBeNullOrEmpty();
        cik.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "EdgarLatestFilingResult EntityTitle can be null")]
    public void EdgarLatestFilingResult_EntityTitle_CanBeNull()
    {
        var result = new EdgarLatestFilingResult
        {
            EntityTitle = null
        };

        result.EntityTitle.Should().BeNull();
    }

    [Fact(DisplayName = "EdgarLatestFilingResult EntityCik has default value")]
    public void EdgarLatestFilingResult_EntityCik_DefaultValue()
    {
        var result = new EdgarLatestFilingResult();

        result.EntityCik.Should().Be(0);
    }
}
