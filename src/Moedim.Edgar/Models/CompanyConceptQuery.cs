using Newtonsoft.Json.Linq;

namespace Moedim.Edgar.Models;

/// <summary>
/// Represents a query result for a specific company concept
/// </summary>
public class CompanyConceptQuery
{
    /// <summary>
    /// Gets or sets the Central Index Key (CIK) of the company
    /// </summary>
    public int CIK { get; set; }

    /// <summary>
    /// Gets or sets the entity name
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// Gets or sets the fact result for this concept
    /// </summary>
    public Fact? Result { get; set; }

    /// <summary>
    /// Parses a JObject into a CompanyConceptQuery instance
    /// </summary>
    /// <param name="jo">The JSON object to parse</param>
    /// <returns>A new CompanyConceptQuery instance</returns>
    public static CompanyConceptQuery Parse(JObject jo)
    {
        CompanyConceptQuery ToReturn = new();

        JProperty? prop_cik = jo.Property("cik");
        if (prop_cik != null)
        {
            ToReturn.CIK = Convert.ToInt32(prop_cik.Value.ToString()!);
        }

        JProperty? prop_entityName = jo.Property("entityName");
        if (prop_entityName != null)
        {
            ToReturn.EntityName = prop_entityName.Value.ToString();
        }

        ToReturn.Result = Fact.Parse(jo);

        return ToReturn;
    }
}
