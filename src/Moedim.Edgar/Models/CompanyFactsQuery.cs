using Newtonsoft.Json.Linq;

namespace Moedim.Edgar.Models;

/// <summary>
/// Represents a query result containing all facts for a company
/// </summary>
public class CompanyFactsQuery
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
    /// Gets or sets the array of facts for this company
    /// </summary>
    public Fact[]? Facts { get; set; }

    /// <summary>
    /// Parses a JObject into a CompanyFactsQuery instance
    /// </summary>
    /// <param name="jo">The JSON object to parse</param>
    /// <returns>A new CompanyFactsQuery instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when jo is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when parsing fails</exception>
    public static CompanyFactsQuery Parse(JObject jo)
    {
        ArgumentNullException.ThrowIfNull(jo);

        try
        {
            var query = new CompanyFactsQuery();

            if (jo.TryGetValue("cik", out JToken? val_cik)) { query.CIK = Convert.ToInt32(val_cik?.ToString() ?? "0"); }
            if (jo.TryGetValue("entityName", out JToken? val_entityName)) { query.EntityName = val_entityName?.ToString(); }

            var facts = new List<Fact>();
            JProperty? prop_facts = jo.Property("facts");
            if (prop_facts != null)
            {
                JObject factsObject = (JObject)prop_facts.Value;
                foreach (JProperty prop_facttype in factsObject.Properties())
                {
                    JObject facttype = (JObject)prop_facttype.Value;
                    foreach (JProperty prop_fact in facttype.Properties())
                    {
                        JObject fact = (JObject)prop_fact.Value;
                        Fact thisFact = Fact.Parse(fact);
                        thisFact.Tag = prop_fact.Name;
                        facts.Add(thisFact);
                    }
                }
            }
            query.Facts = facts.ToArray();

            return query;
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new InvalidOperationException("Failed to parse company facts data", ex);
        }
    }
}
