namespace Moedim.Edgar.Models.Fillings;

/// <summary>
/// Represents a result from a latest filings search
/// </summary>
public class EdgarLatestFilingResult : EdgarFiling
{
    /// <summary>
    /// Title/name of the entity
    /// </summary>
    public string? EntityTitle { get; set; }

    /// <summary>
    /// Central Index Key (CIK) of the entity
    /// </summary>
    public long EntityCik { get; set; }
}
