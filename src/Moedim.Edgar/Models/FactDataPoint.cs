using Newtonsoft.Json.Linq;

namespace Moedim.Edgar.Models;

/// <summary>
/// Represents a single data point for a financial fact
/// </summary>
public class FactDataPoint
{
    /// <summary>
    /// Gets or sets the start date of the reporting period
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// Gets or sets the end date of the reporting period
    /// </summary>
    public DateTime End { get; set; }

    /// <summary>
    /// Gets or sets the numerical value of this data point
    /// </summary>
    public float Value { get; set; }

    /// <summary>
    /// Gets or sets the fiscal year
    /// </summary>
    public int? FiscalYear { get; set; }

    /// <summary>
    /// Gets or sets the fiscal period (Q1, Q2, Q3, Q4, or FY)
    /// </summary>
    public FiscalPeriod Period { get; set; }

    /// <summary>
    /// Gets or sets the SEC form type this data came from (e.g., 10-K, 10-Q)
    /// </summary>
    public string? FromForm { get; set; }

    /// <summary>
    /// Gets or sets the date this data was filed with the SEC
    /// </summary>
    public DateTime Filed { get; set; }

    /// <summary>
    /// Parses a JObject into a FactDataPoint instance
    /// </summary>
    /// <param name="jo">The JSON object to parse</param>
    /// <returns>A new FactDataPoint instance</returns>
    public static FactDataPoint Parse(JObject jo)
    {
        FactDataPoint ToReturn = new();

        JProperty? prop_start = jo.Property("start");
        if (prop_start != null)
        {
            if (prop_start.Value.Type != JTokenType.Null)
            {
                ToReturn.Start = DateTime.Parse(prop_start.Value.ToString()!);
            }
        }

        JProperty? prop_end = jo.Property("end");
        if (prop_end != null)
        {
            ToReturn.End = DateTime.Parse(prop_end.Value.ToString()!);
        }

        JProperty? prop_val = jo.Property("val");
        if (prop_val != null)
        {
            if (prop_val.Value.Type != JTokenType.Null)
            {
                ToReturn.Value = Convert.ToSingle(prop_val.Value.ToString()!);
            }
        }

        JProperty? prop_fy = jo.Property("fy");
        if (prop_fy != null)
        {
            if (prop_fy.Value.Type != JTokenType.Null)
            {
                ToReturn.FiscalYear = Convert.ToInt32(prop_fy.Value.ToString()!);
            }
        }

        JProperty? prop_fp = jo.Property("fp");
        if (prop_fp != null)
        {
            if (prop_fp.Value.Type != JTokenType.Null)
            {
                string fp = prop_fp.Value.ToString()!;
                if (fp.ToLower() == "fy")
                {
                    ToReturn.Period = FiscalPeriod.FiscalYear;
                }
                else if (fp.ToLower() == "q1")
                {
                    ToReturn.Period = FiscalPeriod.Q1;
                }
                else if (fp.ToLower() == "q2")
                {
                    ToReturn.Period = FiscalPeriod.Q2;
                }
                else if (fp.ToLower() == "q3")
                {
                    ToReturn.Period = FiscalPeriod.Q3;
                }
                else if (fp.ToLower() == "q4")
                {
                    ToReturn.Period = FiscalPeriod.Q4;
                }
            }
        }

        if (jo.TryGetValue("form", out var val_form)) { ToReturn.FromForm = val_form.ToString(); }
        if (jo.TryGetValue("filed", out var val_filed)) { ToReturn.Filed = DateTime.Parse(val_filed.ToString()); }

        return ToReturn;
    }
}
