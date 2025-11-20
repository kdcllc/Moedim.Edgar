namespace Moedim.Edgar.Models.Fillings;

/// <summary>
/// Response from an SEC Edgar search
/// </summary>
public class EdgarSearchResponse
{
    /// <summary>
    /// Search results
    /// </summary>
    public EdgarSearchResult[] Results { get; set; } = Array.Empty<EdgarSearchResult>();

    /// <summary>
    /// URL for the next page of results (if available)
    /// </summary>
    public string? NextPageUrl { get; set; }

    /// <summary>
    /// Whether there is a next page available
    /// </summary>
    public bool HasNextPage => !string.IsNullOrEmpty(NextPageUrl);
}
