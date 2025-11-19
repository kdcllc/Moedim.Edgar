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
    /// <exception cref="ArgumentNullException">Thrown when jo is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when parsing fails</exception>
    public static CompanyConceptQuery Parse(JObject jo)
    {
        ArgumentNullException.ThrowIfNull(jo);

        try
        {
            var query = new CompanyConceptQuery();

            JProperty? prop_cik = jo.Property("cik");
            if (prop_cik != null)
            {
                query.CIK = Convert.ToInt32(prop_cik.Value.ToString()!);
            }

            JProperty? prop_entityName = jo.Property("entityName");
            if (prop_entityName != null)
            {
                query.EntityName = prop_entityName.Value.ToString();
            }

            query.Result = Fact.Parse(jo);

            return query;
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new InvalidOperationException("Failed to parse company concept data", ex);
        }
    }
}
