namespace Moedim.Edgar.Models.Fillings;

/// <summary>
/// Filter options for ownership in SEC Edgar searches
/// </summary>
public enum EdgarSearchOwnershipFilter
{
    /// <summary>Include ownership filings</summary>
    Include,

    /// <summary>Exclude ownership filings</summary>
    Exclude,

    /// <summary>Only ownership filings</summary>
    Only
}

/// <summary>
/// Number of results per page in SEC Edgar searches
/// </summary>
public enum EdgarSearchResultsPerPage
{
    /// <summary>10 entries per page</summary>
    Entries10,

    /// <summary>20 entries per page</summary>
    Entries20,

    /// <summary>40 entries per page</summary>
    Entries40,

    /// <summary>80 entries per page</summary>
    Entries80,

    /// <summary>100 entries per page</summary>
    Entries100
}
