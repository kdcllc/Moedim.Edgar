using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.UnitTests.Models.Fillings;

public class EdgarFilingDetailsTests
{
    [Fact(DisplayName = "EdgarFilingDetails properties can be set and retrieved")]
    public void EdgarFilingDetails_PropertiesSetAndGet()
    {
        var filingDate = new DateTime(2024, 2, 1);
        var periodOfReport = new DateTime(2023, 12, 31);
        var accepted = new DateTime(2024, 2, 1, 16, 30, 0);
        var documentFiles = new FilingDocument[] { new FilingDocument { DocumentName = "doc1.html" } };
        var dataFiles = new FilingDocument[] { new FilingDocument { DocumentName = "data1.xml" } };

        var details = new EdgarFilingDetails
        {
            AccessionNumberP1 = 1234567890,
            AccessionNumberP2 = 12,
            AccessionNumberP3 = 123456,
            Form = "10-K",
            FilingDate = filingDate,
            PeriodOfReport = periodOfReport,
            Accepted = accepted,
            DocumentFormatFiles = documentFiles,
            DataFiles = dataFiles,
            EntityName = "Apple Inc.",
            EntityCik = 320193
        };

        details.AccessionNumberP1.Should().Be(1234567890);
        details.AccessionNumberP2.Should().Be(12);
        details.AccessionNumberP3.Should().Be(123456);
        details.Form.Should().Be("10-K");
        details.FilingDate.Should().Be(filingDate);
        details.PeriodOfReport.Should().Be(periodOfReport);
        details.Accepted.Should().Be(accepted);
        details.DocumentFormatFiles.Should().HaveCount(1);
        details.DataFiles.Should().HaveCount(1);
        details.EntityName.Should().Be("Apple Inc.");
        details.EntityCik.Should().Be(320193);
    }

    [Fact(DisplayName = "EdgarFilingDetails nullable properties can be null")]
    public void EdgarFilingDetails_NullableProperties_CanBeNull()
    {
        var details = new EdgarFilingDetails
        {
            Form = null,
            DocumentFormatFiles = null,
            DataFiles = null,
            EntityName = null
        };

        details.Form.Should().BeNull();
        details.DocumentFormatFiles.Should().BeNull();
        details.DataFiles.Should().BeNull();
        details.EntityName.Should().BeNull();
    }

    [Fact(DisplayName = "EdgarFilingDetails numeric properties have default values")]
    public void EdgarFilingDetails_NumericProperties_DefaultValues()
    {
        var details = new EdgarFilingDetails();

        details.AccessionNumberP1.Should().Be(0);
        details.AccessionNumberP2.Should().Be(0);
        details.AccessionNumberP3.Should().Be(0);
        details.EntityCik.Should().Be(0);
    }

    [Fact(DisplayName = "EdgarFilingDetails DateTime properties have default values")]
    public void EdgarFilingDetails_DateTimeProperties_DefaultValues()
    {
        var details = new EdgarFilingDetails();

        details.FilingDate.Should().Be(default(DateTime));
        details.PeriodOfReport.Should().Be(default(DateTime));
        details.Accepted.Should().Be(default(DateTime));
    }
}
