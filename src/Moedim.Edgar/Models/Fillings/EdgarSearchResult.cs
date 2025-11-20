namespace Moedim.Edgar.Models.Fillings;

/// <summary>
/// Represents a single result from an SEC Edgar search
/// </summary>
public class EdgarSearchResult : EdgarFiling
{
    /// <summary>
    /// URL to interactive data viewer (if available)
    /// </summary>
    public string? InteractiveDataUrl { get; set; }
}
