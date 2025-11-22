namespace Moedim.Edgar.Models.Fillings;

/// <summary>
/// Request parameters for retrieving filing sections
/// </summary>
public class FilingSectionsRequest
{
    /// <summary>
    /// If true, only return section previews without full content
    /// </summary>
    public bool PreviewOnly { get; set; }

    /// <summary>
    /// List of anchor IDs to retrieve. If null or empty, returns all sections or preview.
    /// </summary>
    public IReadOnlyList<string>? AnchorIds { get; set; }

    /// <summary>
    /// Output format: 'markdown' (default) or 'html'
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// If true, merge all requested sections into a single content block
    /// </summary>
    public bool Merge { get; set; }
}
