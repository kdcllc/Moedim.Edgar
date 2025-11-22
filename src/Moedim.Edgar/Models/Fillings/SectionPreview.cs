namespace Moedim.Edgar.Models.Fillings;

/// <summary>
/// Represents a preview of a filing section without full content
/// </summary>
public class SectionPreview
{
    /// <summary>
    /// The human-readable label for this section
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The anchor/target ID for this section
    /// </summary>
    public string AnchorTargetId { get; set; } = string.Empty;

    /// <summary>
    /// A brief content snippet from this section
    /// </summary>
    public string? Snippet { get; set; }
}
