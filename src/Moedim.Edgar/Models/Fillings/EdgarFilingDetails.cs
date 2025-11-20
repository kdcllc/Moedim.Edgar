namespace Moedim.Edgar.Models.Fillings;

/// <summary>
/// Detailed information about an SEC Edgar filing
/// </summary>
public class EdgarFilingDetails
{
    /// <summary>
    /// First part of the accession number
    /// </summary>
    public long AccessionNumberP1 { get; set; }

    /// <summary>
    /// Second part of the accession number
    /// </summary>
    public int AccessionNumberP2 { get; set; }

    /// <summary>
    /// Third part of the accession number
    /// </summary>
    public int AccessionNumberP3 { get; set; }

    /// <summary>
    /// Form type (e.g., 10-K, 10-Q)
    /// </summary>
    public string? Form { get; set; }

    /// <summary>
    /// Date the filing was submitted
    /// </summary>
    public DateTime FilingDate { get; set; }

    /// <summary>
    /// Period covered by the report
    /// </summary>
    public DateTime PeriodOfReport { get; set; }

    /// <summary>
    /// Date/time the filing was accepted
    /// </summary>
    public DateTime Accepted { get; set; }

    /// <summary>
    /// Document format files included in the filing
    /// </summary>
    public FilingDocument[]? DocumentFormatFiles { get; set; }

    /// <summary>
    /// Data files included in the filing
    /// </summary>
    public FilingDocument[]? DataFiles { get; set; }

    /// <summary>
    /// Name of the entity (company)
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// Central Index Key (CIK) of the entity
    /// </summary>
    public long EntityCik { get; set; }
}
