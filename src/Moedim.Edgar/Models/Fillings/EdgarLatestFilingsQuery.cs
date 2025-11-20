namespace Moedim.Edgar.Models.Fillings;

/// <summary>
/// Parameters for searching latest SEC Edgar filings
/// </summary>
public class EdgarLatestFilingsQuery
{
    /// <summary>
    /// Form type to filter by (e.g., "10-K", "8-K")
    /// </summary>
    public string? FormType { get; set; }

    /// <summary>
    /// Filter for ownership filings
    /// </summary>
    public EdgarSearchOwnershipFilter OwnershipFilter { get; set; } = EdgarSearchOwnershipFilter.Include;

    /// <summary>
    /// Number of results per page
    /// </summary>
    public EdgarSearchResultsPerPage ResultsPerPage { get; set; } = EdgarSearchResultsPerPage.Entries40;
}
