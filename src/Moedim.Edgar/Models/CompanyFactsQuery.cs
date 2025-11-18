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
    public static CompanyFactsQuery Parse(JObject jo)
    {
        CompanyFactsQuery ToReturn = new();

        if (jo.TryGetValue("cik", out JToken? val_cik)) { ToReturn.CIK = Convert.ToInt32(val_cik?.ToString() ?? "0"); }
        if (jo.TryGetValue("entityName", out JToken? val_entityName)) { ToReturn.EntityName = val_entityName?.ToString(); }

        List<Fact> Facts = new();
        JProperty? prop_facts = jo.Property("facts");
        if (prop_facts != null)
        {
            JObject facts = (JObject)prop_facts.Value;
            foreach (JProperty prop_facttype in facts.Properties())
            {
                JObject facttype = (JObject)prop_facttype.Value;
                foreach (JProperty prop_fact in facttype.Properties())
                {
                    JObject fact = (JObject)prop_fact.Value;
                    Fact ThisFact = Fact.Parse(fact);
                    ThisFact.Tag = prop_fact.Name;
                    Facts.Add(ThisFact);
                }
            }
        }
        ToReturn.Facts = Facts.ToArray();

        return ToReturn;
    }
}
