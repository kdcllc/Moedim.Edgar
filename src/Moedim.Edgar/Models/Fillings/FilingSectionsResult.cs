namespace Moedim.Edgar.Models.Fillings;

/// <summary>
/// Result of a filing sections query
/// </summary>
public class FilingSectionsResult
{
    /// <summary>
    /// Section previews (when PreviewOnly is true)
    /// </summary>
    public IReadOnlyList<SectionPreview>? Preview { get; set; }

    /// <summary>
    /// Full section content (when PreviewOnly is false)
    /// </summary>
    public IReadOnlyList<HtmlSlice>? Sections { get; set; }

    /// <summary>
    /// Merged content (when Merge is true)
    /// </summary>
    public string? MergedContent { get; set; }
}
