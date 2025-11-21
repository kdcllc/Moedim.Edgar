namespace Moedim.Edgar.Models.Fillings;

/// <summary>
/// Base class representing an SEC Edgar filing
/// </summary>
public class EdgarFiling
{
    /// <summary>
    /// Filing type (e.g., 10-K, 10-Q, 8-K)
    /// </summary>
    public string? Filing { get; set; }

    /// <summary>
    /// URL to the documents page for this filing
    /// </summary>
    public string? DocumentsUrl { get; set; }

    /// <summary>
    /// Description of the filing
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Date the filing was submitted
    /// </summary>
    public DateTime FilingDate { get; set; }
}
