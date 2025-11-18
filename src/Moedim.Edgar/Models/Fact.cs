using Newtonsoft.Json.Linq;

namespace Moedim.Edgar.Models;

/// <summary>
/// Represents a financial fact from SEC EDGAR data
/// </summary>
public class Fact
{
    /// <summary>
    /// Gets or sets the XBRL tag for this fact
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets the human-readable label
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the description of this fact
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the array of data points for this fact
    /// </summary>
    public FactDataPoint[]? DataPoints { get; set; }

    /// <summary>
    /// Parses a JObject into a Fact instance
    /// </summary>
    /// <param name="jo">The JSON object to parse</param>
    /// <returns>A new Fact instance</returns>
    public static Fact Parse(JObject jo)
    {
        Fact ToReturn = new();

        if (jo.TryGetValue("tag", out JToken? val_tag)){ ToReturn.Tag = val_tag?.ToString(); }
        if (jo.TryGetValue("label", out JToken? val_label)) { ToReturn.Label = val_label?.ToString(); }
        if (jo.TryGetValue("description", out JToken? val_description)){ ToReturn.Description = val_description?.ToString(); }

        List<FactDataPoint> DataPoints = new();
        JProperty? prop_units = jo.Property("units");
        if (prop_units != null)
        {
            JObject units = (JObject)prop_units.Value;
            foreach (JProperty prop_unittypes in units.Properties())
            {
                JArray unittype = (JArray)prop_unittypes.Value;
                foreach (JObject factdata in unittype)
                {
                    DataPoints.Add(FactDataPoint.Parse(factdata));
                }
            }
        }
        ToReturn.DataPoints = DataPoints.ToArray();

        return ToReturn;
    }
}
