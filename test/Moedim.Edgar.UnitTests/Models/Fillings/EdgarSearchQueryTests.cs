using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.UnitTests.Models.Fillings;

public class EdgarSearchQueryTests
{
    [Fact(DisplayName = "EdgarSearchQuery properties can be set and retrieved")]
    public void EdgarSearchQuery_PropertiesSetAndGet()
    {
        var priorTo = new DateTime(2024, 1, 1);
        var query = new EdgarSearchQuery
        {
            Symbol = "AAPL",
            FilingType = "10-K",
            PriorTo = priorTo,
            OwnershipFilter = EdgarSearchOwnershipFilter.Only,
            ResultsPerPage = EdgarSearchResultsPerPage.Entries100
        };

        query.Symbol.Should().Be("AAPL");
        query.FilingType.Should().Be("10-K");
        query.PriorTo.Should().Be(priorTo);
        query.OwnershipFilter.Should().Be(EdgarSearchOwnershipFilter.Only);
        query.ResultsPerPage.Should().Be(EdgarSearchResultsPerPage.Entries100);
    }

    [Fact(DisplayName = "EdgarSearchQuery has correct default values")]
    public void EdgarSearchQuery_DefaultValues_AreCorrect()
    {
        var query = new EdgarSearchQuery();

        query.Symbol.Should().Be(string.Empty);
        query.FilingType.Should().BeNull();
        query.PriorTo.Should().BeNull();
        query.OwnershipFilter.Should().Be(EdgarSearchOwnershipFilter.Exclude);
        query.ResultsPerPage.Should().Be(EdgarSearchResultsPerPage.Entries40);
    }

    [Fact(DisplayName = "EdgarSearchQuery FilingType can be null")]
    public void EdgarSearchQuery_FilingTypeNull_IsAllowed()
    {
        var query = new EdgarSearchQuery
        {
            Symbol = "MSFT",
            FilingType = null
        };

        query.FilingType.Should().BeNull();
    }
}
