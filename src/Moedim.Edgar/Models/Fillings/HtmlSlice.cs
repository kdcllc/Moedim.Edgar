namespace Moedim.Edgar.Models.Fillings;

/// <summary>
/// Represents a parsed section of a SEC filing HTML document
/// </summary>
public class HtmlSlice
{
    /// <summary>
    /// The HTML content of this section
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The human-readable label for this section (e.g., "PART I", "ITEM 1A. RISK FACTORS")
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The generated anchor/target ID for this section used for navigation
    /// </summary>
    public string AnchorTargetId { get; set; } = string.Empty;

    /// <summary>
    /// The starting position of this section in the original document
    /// </summary>
    public int StartOffset { get; set; }

    /// <summary>
    /// The length of this section in the original document
    /// </summary>
    public int Length { get; set; }
}
