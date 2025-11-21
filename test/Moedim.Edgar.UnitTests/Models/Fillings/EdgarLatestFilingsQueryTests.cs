using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.UnitTests.Models.Fillings;

public class EdgarLatestFilingsQueryTests
{
    [Fact(DisplayName = "EdgarLatestFilingsQuery properties can be set and retrieved")]
    public void EdgarLatestFilingsQuery_PropertiesSetAndGet()
    {
        var query = new EdgarLatestFilingsQuery
        {
            FormType = "8-K",
            OwnershipFilter = EdgarSearchOwnershipFilter.Exclude,
            ResultsPerPage = EdgarSearchResultsPerPage.Entries80
        };

        query.FormType.Should().Be("8-K");
        query.OwnershipFilter.Should().Be(EdgarSearchOwnershipFilter.Exclude);
        query.ResultsPerPage.Should().Be(EdgarSearchResultsPerPage.Entries80);
    }

    [Fact(DisplayName = "EdgarLatestFilingsQuery has correct default values")]
    public void EdgarLatestFilingsQuery_DefaultValues_AreCorrect()
    {
        var query = new EdgarLatestFilingsQuery();

        query.FormType.Should().BeNull();
        query.OwnershipFilter.Should().Be(EdgarSearchOwnershipFilter.Include);
        query.ResultsPerPage.Should().Be(EdgarSearchResultsPerPage.Entries40);
    }

    [Fact(DisplayName = "EdgarLatestFilingsQuery FormType can be null")]
    public void EdgarLatestFilingsQuery_FormTypeNull_IsAllowed()
    {
        var query = new EdgarLatestFilingsQuery
        {
            FormType = null
        };

        query.FormType.Should().BeNull();
    }
}
