namespace Moedim.Edgar.Models.Fillings;

/// <summary>
/// Parameters for searching SEC Edgar filings
/// </summary>
public class EdgarSearchQuery
{
    /// <summary>
    /// Stock symbol or CIK to search for
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Filing type to filter by (e.g., "10-K", "10-Q")
    /// </summary>
    public string? FilingType { get; set; }

    /// <summary>
    /// Only return filings prior to this date
    /// </summary>
    public DateTime? PriorTo { get; set; }

    /// <summary>
    /// Filter for ownership filings
    /// </summary>
    public EdgarSearchOwnershipFilter OwnershipFilter { get; set; } = EdgarSearchOwnershipFilter.Exclude;

    /// <summary>
    /// Number of results per page
    /// </summary>
    public EdgarSearchResultsPerPage ResultsPerPage { get; set; } = EdgarSearchResultsPerPage.Entries40;
}
