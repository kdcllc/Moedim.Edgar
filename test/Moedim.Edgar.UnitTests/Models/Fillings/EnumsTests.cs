using Moedim.Edgar.Models.Fillings;

namespace Moedim.Edgar.UnitTests.Models.Fillings;

public class EnumsTests
{
    [Theory(DisplayName = "EdgarSearchOwnershipFilter enum has correct values")]
    [InlineData(EdgarSearchOwnershipFilter.Include, 0)]
    [InlineData(EdgarSearchOwnershipFilter.Exclude, 1)]
    [InlineData(EdgarSearchOwnershipFilter.Only, 2)]
    public void EdgarSearchOwnershipFilter_EnumValues_AreCorrect(EdgarSearchOwnershipFilter filter, int expectedValue)
    {
        ((int)filter).Should().Be(expectedValue);
    }

    [Theory(DisplayName = "EdgarSearchResultsPerPage enum has correct values")]
    [InlineData(EdgarSearchResultsPerPage.Entries10, 0)]
    [InlineData(EdgarSearchResultsPerPage.Entries20, 1)]
    [InlineData(EdgarSearchResultsPerPage.Entries40, 2)]
    [InlineData(EdgarSearchResultsPerPage.Entries80, 3)]
    [InlineData(EdgarSearchResultsPerPage.Entries100, 4)]
    public void EdgarSearchResultsPerPage_EnumValues_AreCorrect(EdgarSearchResultsPerPage perPage, int expectedValue)
    {
        ((int)perPage).Should().Be(expectedValue);
    }

    [Fact(DisplayName = "EdgarSearchOwnershipFilter enum can be cast from int")]
    public void EdgarSearchOwnershipFilter_CastFromInt_ReturnsCorrectValue()
    {
        EdgarSearchOwnershipFilter filter = (EdgarSearchOwnershipFilter)1;

        filter.Should().Be(EdgarSearchOwnershipFilter.Exclude);
    }

    [Fact(DisplayName = "EdgarSearchResultsPerPage enum can be cast from int")]
    public void EdgarSearchResultsPerPage_CastFromInt_ReturnsCorrectValue()
    {
        EdgarSearchResultsPerPage perPage = (EdgarSearchResultsPerPage)2;

        perPage.Should().Be(EdgarSearchResultsPerPage.Entries40);
    }

    [Fact(DisplayName = "EdgarSearchOwnershipFilter ToString returns name")]
    public void EdgarSearchOwnershipFilter_ToString_ReturnsEnumName()
    {
        var filter = EdgarSearchOwnershipFilter.Only;

        filter.ToString().Should().Be("Only");
    }

    [Fact(DisplayName = "EdgarSearchResultsPerPage ToString returns name")]
    public void EdgarSearchResultsPerPage_ToString_ReturnsEnumName()
    {
        var perPage = EdgarSearchResultsPerPage.Entries100;

        perPage.ToString().Should().Be("Entries100");
    }

    [Fact(DisplayName = "EdgarSearchOwnershipFilter default value is Include")]
    public void EdgarSearchOwnershipFilter_DefaultValue_IsInclude()
    {
        EdgarSearchOwnershipFilter defaultFilter = default;

        defaultFilter.Should().Be(EdgarSearchOwnershipFilter.Include);
    }

    [Fact(DisplayName = "EdgarSearchResultsPerPage default value is Entries10")]
    public void EdgarSearchResultsPerPage_DefaultValue_IsEntries10()
    {
        EdgarSearchResultsPerPage defaultPerPage = default;

        defaultPerPage.Should().Be(EdgarSearchResultsPerPage.Entries10);
    }
}
