using Newtonsoft.Json.Linq;

namespace Moedim.Edgar.Models.Data;

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
    public decimal Value { get; set; }

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
    /// <exception cref="ArgumentNullException">Thrown when jo is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when parsing fails</exception>
    public static FactDataPoint Parse(JObject jo)
    {
        ArgumentNullException.ThrowIfNull(jo);

        try
        {
            var dataPoint = new FactDataPoint();

            JProperty? prop_start = jo.Property("start");
            if (prop_start != null && prop_start.Value.Type != JTokenType.Null)
            {
                dataPoint.Start = DateTime.Parse(prop_start.Value.ToString()!, System.Globalization.CultureInfo.InvariantCulture);
            }

            JProperty? prop_end = jo.Property("end");
            if (prop_end != null)
            {
                dataPoint.End = DateTime.Parse(prop_end.Value.ToString()!, System.Globalization.CultureInfo.InvariantCulture);
            }

            JProperty? prop_val = jo.Property("val");
            if (prop_val != null && prop_val.Value.Type != JTokenType.Null)
            {
                dataPoint.Value = Convert.ToDecimal(prop_val.Value.ToString()!);
            }

            JProperty? prop_fy = jo.Property("fy");
            if (prop_fy != null && prop_fy.Value.Type != JTokenType.Null)
            {
                dataPoint.FiscalYear = Convert.ToInt32(prop_fy.Value.ToString()!);
            }

            JProperty? prop_fp = jo.Property("fp");
            if (prop_fp != null && prop_fp.Value.Type != JTokenType.Null)
            {
                string fp = prop_fp.Value.ToString()!;
                dataPoint.Period = fp switch
                {
                    var p when p.Equals("fy", StringComparison.OrdinalIgnoreCase) => FiscalPeriod.FiscalYear,
                    var p when p.Equals("q1", StringComparison.OrdinalIgnoreCase) => FiscalPeriod.Q1,
                    var p when p.Equals("q2", StringComparison.OrdinalIgnoreCase) => FiscalPeriod.Q2,
                    var p when p.Equals("q3", StringComparison.OrdinalIgnoreCase) => FiscalPeriod.Q3,
                    var p when p.Equals("q4", StringComparison.OrdinalIgnoreCase) => FiscalPeriod.Q4,
                    _ => FiscalPeriod.FiscalYear
                };
            }

            if (jo.TryGetValue("form", out var val_form)) { dataPoint.FromForm = val_form.ToString(); }
            if (jo.TryGetValue("filed", out var val_filed))
            {
                dataPoint.Filed = DateTime.Parse(val_filed.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            }

            return dataPoint;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse fact data point", ex);
        }
    }
}
