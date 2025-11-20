using Moedim.Edgar.Models.Data;
using Newtonsoft.Json.Linq;

namespace Moedim.Edgar.UnitTests.Models.Data;

public class FactDataPointTests
{
    [Fact(DisplayName = "FactDataPoint properties can be set and retrieved")]
    public void FactDataPoint_PropertiesSetAndGet()
    {
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2023, 12, 31);
        var filed = new DateTime(2024, 2, 1);

        var dataPoint = new FactDataPoint
        {
            Start = start,
            End = end,
            Value = 1500000.50m,
            FiscalYear = 2023,
            Period = FiscalPeriod.FiscalYear,
            FromForm = "10-K",
            Filed = filed
        };

        dataPoint.Start.Should().Be(start);
        dataPoint.Start.Should().NotBeNull(); // Explicitly test Start getter
        dataPoint.End.Should().Be(end);
        dataPoint.Value.Should().Be(1500000.50m);
        dataPoint.FiscalYear.Should().Be(2023);
        dataPoint.Period.Should().Be(FiscalPeriod.FiscalYear);
        dataPoint.FromForm.Should().Be("10-K");
        dataPoint.Filed.Should().Be(filed);
    }

    [Fact(DisplayName = "Parse returns valid FactDataPoint from JObject")]
    public void Parse_ValidJObject_ReturnsFactDataPoint()
    {
        var json = @"{
            ""start"": ""2023-01-01"",
            ""end"": ""2023-12-31"",
            ""val"": 1000000.25,
            ""fy"": 2023,
            ""fp"": ""FY"",
            ""form"": ""10-K"",
            ""filed"": ""2024-02-01""
        }";
        var jObject = JObject.Parse(json);

        var result = FactDataPoint.Parse(jObject);

        result.Should().NotBeNull();
        result.Start.Should().Be(new DateTime(2023, 1, 1));
        result.End.Should().Be(new DateTime(2023, 12, 31));
        result.Value.Should().Be(1000000.25m);
        result.FiscalYear.Should().Be(2023);
        result.Period.Should().Be(FiscalPeriod.FiscalYear);
        result.FromForm.Should().Be("10-K");
        result.Filed.Should().Be(new DateTime(2024, 2, 1));
    }

    [Fact(DisplayName = "Parse throws ArgumentNullException when JObject is null")]
    public void Parse_NullJObject_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            FactDataPoint.Parse(null!));

        exception.ParamName.Should().Be("jo");
    }

    [Fact(DisplayName = "Parse handles null start date")]
    public void Parse_NullStartDate_ReturnsNullStart()
    {
        var json = @"{
            ""start"": null,
            ""end"": ""2023-12-31"",
            ""val"": 1000,
            ""filed"": ""2024-01-01""
        }";
        var jObject = JObject.Parse(json);

        var result = FactDataPoint.Parse(jObject);

        result.Start.Should().BeNull();
        result.End.Should().Be(new DateTime(2023, 12, 31));
    }

    [Fact(DisplayName = "Parse handles non-null start date")]
    public void Parse_NonNullStartDate_ReturnsValidStart()
    {
        var json = @"{
            ""start"": ""2023-01-01"",
            ""end"": ""2023-12-31"",
            ""val"": 1000,
            ""filed"": ""2024-01-01""
        }";
        var jObject = JObject.Parse(json);

        var result = FactDataPoint.Parse(jObject);

        result.Start.Should().Be(new DateTime(2023, 1, 1));
        result.End.Should().Be(new DateTime(2023, 12, 31));
    }

    [Fact(DisplayName = "Parse handles null value")]
    public void Parse_NullValue_ReturnsZeroValue()
    {
        var json = @"{
            ""end"": ""2023-12-31"",
            ""val"": null,
            ""filed"": ""2024-01-01""
        }";
        var jObject = JObject.Parse(json);

        var result = FactDataPoint.Parse(jObject);

        result.Value.Should().Be(0m);
    }

    [Fact(DisplayName = "Parse handles null fiscal year")]
    public void Parse_NullFiscalYear_ReturnsNullFiscalYear()
    {
        var json = @"{
            ""end"": ""2023-12-31"",
            ""val"": 1000,
            ""fy"": null,
            ""filed"": ""2024-01-01""
        }";
        var jObject = JObject.Parse(json);

        var result = FactDataPoint.Parse(jObject);

        result.FiscalYear.Should().BeNull();
    }

    [Theory(DisplayName = "Parse correctly maps fiscal period values")]
    [InlineData("FY", FiscalPeriod.FiscalYear)]
    [InlineData("fy", FiscalPeriod.FiscalYear)]
    [InlineData("Q1", FiscalPeriod.Q1)]
    [InlineData("q1", FiscalPeriod.Q1)]
    [InlineData("Q2", FiscalPeriod.Q2)]
    [InlineData("q2", FiscalPeriod.Q2)]
    [InlineData("Q3", FiscalPeriod.Q3)]
    [InlineData("q3", FiscalPeriod.Q3)]
    [InlineData("Q4", FiscalPeriod.Q4)]
    [InlineData("q4", FiscalPeriod.Q4)]
    [InlineData("unknown", FiscalPeriod.FiscalYear)]
    public void Parse_FiscalPeriodValues_MapsCorrectly(string fpValue, FiscalPeriod expectedPeriod)
    {
        var json = $@"{{
            ""end"": ""2023-12-31"",
            ""val"": 1000,
            ""fp"": ""{fpValue}"",
            ""filed"": ""2024-01-01""
        }}";
        var jObject = JObject.Parse(json);

        var result = FactDataPoint.Parse(jObject);

        result.Period.Should().Be(expectedPeriod);
    }

    [Fact(DisplayName = "Parse handles null fiscal period")]
    public void Parse_NullFiscalPeriod_DefaultsToFiscalYear()
    {
        var json = @"{
            ""end"": ""2023-12-31"",
            ""val"": 1000,
            ""fp"": null,
            ""filed"": ""2024-01-01""
        }";
        var jObject = JObject.Parse(json);

        var result = FactDataPoint.Parse(jObject);

        result.Period.Should().Be(FiscalPeriod.FiscalYear);
    }

    [Fact(DisplayName = "Parse handles missing form property using TryGetValue")]
    public void Parse_MissingForm_ReturnsNullForm()
    {
        var json = @"{
            ""end"": ""2023-12-31"",
            ""val"": 1000,
            ""filed"": ""2024-01-01""
        }";
        var jObject = JObject.Parse(json);

        var result = FactDataPoint.Parse(jObject);

        result.FromForm.Should().BeNull();
    }

    [Fact(DisplayName = "Parse handles scientific notation in value")]
    public void Parse_ScientificNotationValue_ParsesCorrectly()
    {
        var json = @"{
            ""end"": ""2023-12-31"",
            ""val"": 1.5e6,
            ""filed"": ""2024-01-01""
        }";
        var jObject = JObject.Parse(json);

        var result = FactDataPoint.Parse(jObject);

        result.Value.Should().Be(1500000m);
    }

    [Fact(DisplayName = "Parse throws InvalidOperationException for invalid end date")]
    public void Parse_InvalidEndDate_ThrowsInvalidOperationException()
    {
        var json = @"{
            ""end"": ""not-a-date"",
            ""val"": 1000,
            ""filed"": ""2024-01-01""
        }";
        var jObject = JObject.Parse(json);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            FactDataPoint.Parse(jObject));

        exception.Message.Should().Contain("Failed to parse fact data point");
    }

    [Fact(DisplayName = "Parse throws InvalidOperationException for invalid filed date")]
    public void Parse_InvalidFiledDate_ThrowsInvalidOperationException()
    {
        var json = @"{
            ""end"": ""2023-12-31"",
            ""val"": 1000,
            ""filed"": ""invalid-date""
        }";
        var jObject = JObject.Parse(json);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            FactDataPoint.Parse(jObject));

        exception.Message.Should().Contain("Failed to parse fact data point");
    }

    [Fact(DisplayName = "Parse throws InvalidOperationException for invalid value format")]
    public void Parse_InvalidValueFormat_ThrowsInvalidOperationException()
    {
        var json = @"{
            ""end"": ""2023-12-31"",
            ""val"": ""not-a-number"",
            ""filed"": ""2024-01-01""
        }";
        var jObject = JObject.Parse(json);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            FactDataPoint.Parse(jObject));

        exception.Message.Should().Contain("Failed to parse fact data point");
    }
}
