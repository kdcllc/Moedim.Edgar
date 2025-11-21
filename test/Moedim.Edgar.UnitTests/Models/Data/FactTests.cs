using Moedim.Edgar.Models.Data;
using Newtonsoft.Json.Linq;

namespace Moedim.Edgar.UnitTests.Models.Data;

public class FactTests
{
    [Fact(DisplayName = "Fact properties can be set and retrieved")]
    public void Fact_PropertiesSetAndGet()
    {
        var dataPoints = new FactDataPoint[] { new FactDataPoint { Value = 100m } };
        var fact = new Fact
        {
            Tag = "Revenue",
            Label = "Total Revenue",
            Description = "Total revenue from all sources",
            DataPoints = dataPoints
        };

        fact.Tag.Should().Be("Revenue");
        fact.Label.Should().Be("Total Revenue");
        fact.Description.Should().Be("Total revenue from all sources");
        fact.DataPoints.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Parse returns valid Fact from JObject")]
    public void Parse_ValidJObject_ReturnsFact()
    {
        var json = @"{
            ""tag"": ""AccountsPayable"",
            ""label"": ""Accounts Payable, Current"",
            ""description"": ""Carrying value as of the balance sheet date"",
            ""units"": {
                ""USD"": [
                    {
                        ""end"": ""2023-12-31"",
                        ""val"": 1000000,
                        ""filed"": ""2024-02-01""
                    }
                ]
            }
        }";
        var jObject = JObject.Parse(json);

        var result = Fact.Parse(jObject);

        result.Should().NotBeNull();
        result.Tag.Should().Be("AccountsPayable");
        result.Label.Should().Be("Accounts Payable, Current");
        result.Description.Should().Be("Carrying value as of the balance sheet date");
        result.DataPoints.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Parse throws ArgumentNullException when JObject is null")]
    public void Parse_NullJObject_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Fact.Parse(null!));

        exception.ParamName.Should().Be("jo");
    }

    [Fact(DisplayName = "Parse handles multiple unit types")]
    public void Parse_MultipleUnitTypes_ParsesAllDataPoints()
    {
        var json = @"{
            ""tag"": ""Revenue"",
            ""label"": ""Revenue"",
            ""units"": {
                ""USD"": [
                    { ""end"": ""2023-12-31"", ""val"": 1000, ""filed"": ""2024-01-01"" }
                ],
                ""EUR"": [
                    { ""end"": ""2023-12-31"", ""val"": 900, ""filed"": ""2024-01-01"" }
                ]
            }
        }";
        var jObject = JObject.Parse(json);

        var result = Fact.Parse(jObject);

        result.DataPoints.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Parse handles missing optional properties")]
    public void Parse_MissingOptionalProperties_ReturnsFactWithNulls()
    {
        var json = @"{
            ""units"": {}
        }";
        var jObject = JObject.Parse(json);

        var result = Fact.Parse(jObject);

        result.Tag.Should().BeNull();
        result.Label.Should().BeNull();
        result.Description.Should().BeNull();
        result.DataPoints.Should().BeEmpty();
    }

    [Fact(DisplayName = "Parse handles missing units property")]
    public void Parse_MissingUnits_ReturnsEmptyDataPoints()
    {
        var json = @"{
            ""tag"": ""TestTag"",
            ""label"": ""Test Label""
        }";
        var jObject = JObject.Parse(json);

        var result = Fact.Parse(jObject);

        result.DataPoints.Should().NotBeNull();
        result.DataPoints.Should().BeEmpty();
    }

    [Fact(DisplayName = "Parse throws InvalidOperationException when data point parsing fails")]
    public void Parse_InvalidDataPoint_ThrowsInvalidOperationException()
    {
        var json = @"{
            ""tag"": ""TestTag"",
            ""units"": {
                ""USD"": [
                    {
                        ""end"": ""invalid-date"",
                        ""val"": 1000,
                        ""filed"": ""2024-01-01""
                    }
                ]
            }
        }";
        var jObject = JObject.Parse(json);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            Fact.Parse(jObject));

        exception.Message.Should().Contain("Failed to parse fact data");
        exception.InnerException.Should().NotBeNull();
    }
}
