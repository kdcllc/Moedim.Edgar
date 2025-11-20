using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.UnitTests.Models.Fillings;

public class EdgarFilingTests
{
    [Fact(DisplayName = "EdgarFiling properties can be set and retrieved")]
    public void EdgarFiling_PropertiesSetAndGet()
    {
        var filingDate = new DateTime(2024, 2, 1);
        var filing = new EdgarFiling
        {
            Filing = "10-K",
            DocumentsUrl = "https://www.sec.gov/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193",
            Description = "Annual Report",
            FilingDate = filingDate
        };

        filing.Filing.Should().Be("10-K");
        filing.DocumentsUrl.Should().Be("https://www.sec.gov/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193");
        filing.Description.Should().Be("Annual Report");
        filing.FilingDate.Should().Be(filingDate);
    }

    [Fact(DisplayName = "EdgarFiling properties can be null")]
    public void EdgarFiling_NullableProperties_CanBeNull()
    {
        var filing = new EdgarFiling
        {
            Filing = null,
            DocumentsUrl = null,
            Description = null
        };

        filing.Filing.Should().BeNull();
        filing.DocumentsUrl.Should().BeNull();
        filing.Description.Should().BeNull();
    }

    [Fact(DisplayName = "EdgarFiling FilingDate has default value")]
    public void EdgarFiling_FilingDate_DefaultValue()
    {
        var filing = new EdgarFiling();

        filing.FilingDate.Should().Be(default(DateTime));
    }
}
