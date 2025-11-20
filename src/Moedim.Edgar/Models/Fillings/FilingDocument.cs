namespace Moedim.Edgar.Models.Fillings;

/// <summary>
/// Represents a document within an SEC filing
/// </summary>
public class FilingDocument
{
    /// <summary>
    /// Sequence number of the document
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// Description of the document
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Name of the document file
    /// </summary>
    public string? DocumentName { get; set; }

    /// <summary>
    /// URL to access the document
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Type of the document
    /// </summary>
    public string? DocumentType { get; set; }

    /// <summary>
    /// Size of the document in bytes
    /// </summary>
    public int Size { get; set; }
}
