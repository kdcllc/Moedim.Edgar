using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.UnitTests.Models.Fillings;

public class FilingDocumentTests
{
    [Fact(DisplayName = "FilingDocument properties can be set and retrieved")]
    public void FilingDocument_PropertiesSetAndGet()
    {
        var document = new FilingDocument
        {
            Sequence = 1,
            Description = "Complete submission text file",
            DocumentName = "aapl-20231231.htm",
            Url = "https://www.sec.gov/Archives/edgar/data/320193/000032019324000006/aapl-20231231.htm",
            DocumentType = "10-K",
            Size = 1024567
        };

        document.Sequence.Should().Be(1);
        document.Description.Should().Be("Complete submission text file");
        document.DocumentName.Should().Be("aapl-20231231.htm");
        document.Url.Should().Be("https://www.sec.gov/Archives/edgar/data/320193/000032019324000006/aapl-20231231.htm");
        document.DocumentType.Should().Be("10-K");
        document.Size.Should().Be(1024567);
    }

    [Fact(DisplayName = "FilingDocument nullable properties can be null")]
    public void FilingDocument_NullableProperties_CanBeNull()
    {
        var document = new FilingDocument
        {
            Description = null,
            DocumentName = null,
            Url = null,
            DocumentType = null
        };

        document.Description.Should().BeNull();
        document.DocumentName.Should().BeNull();
        document.Url.Should().BeNull();
        document.DocumentType.Should().BeNull();
    }

    [Fact(DisplayName = "FilingDocument numeric properties have default values")]
    public void FilingDocument_NumericProperties_DefaultValues()
    {
        var document = new FilingDocument();

        document.Sequence.Should().Be(0);
        document.Size.Should().Be(0);
    }
}
