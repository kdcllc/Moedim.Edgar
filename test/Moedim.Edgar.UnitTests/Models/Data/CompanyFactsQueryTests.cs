using Moedim.Edgar.Models.Data;
using Newtonsoft.Json.Linq;

namespace Moedim.Edgar.UnitTests.Models.Data;

public class CompanyFactsQueryTests
{
    [Fact(DisplayName = "CompanyFactsQuery properties can be set and retrieved")]
    public void CompanyFactsQuery_PropertiesSetAndGet()
    {
        var facts = new Fact[] { new Fact { Tag = "Tag1" }, new Fact { Tag = "Tag2" } };
        var query = new CompanyFactsQuery
        {
            CIK = 789012,
            EntityName = "Microsoft Corporation",
            Facts = facts
        };

        query.CIK.Should().Be(789012);
        query.EntityName.Should().Be("Microsoft Corporation");
        query.Facts.Should().HaveCount(2);
        query.Facts![0].Tag.Should().Be("Tag1");
    }

    [Fact(DisplayName = "Parse returns valid CompanyFactsQuery from JObject")]
    public void Parse_ValidJObject_ReturnsCompanyFactsQuery()
    {
        var json = @"{
            ""cik"": ""320193"",
            ""entityName"": ""Apple Inc."",
            ""facts"": {
                ""us-gaap"": {
                    ""AccountsPayable"": {
                        ""label"": ""Accounts Payable"",
                        ""units"": {
                            ""USD"": []
                        }
                    }
                }
            }
        }";
        var jObject = JObject.Parse(json);

        var result = CompanyFactsQuery.Parse(jObject);

        result.Should().NotBeNull();
        result.CIK.Should().Be(320193);
        result.EntityName.Should().Be("Apple Inc.");
        result.Facts.Should().NotBeNull();
        result.Facts.Should().HaveCount(1);
        result.Facts![0].Tag.Should().Be("AccountsPayable");
    }

    [Fact(DisplayName = "Parse throws ArgumentNullException when JObject is null")]
    public void Parse_NullJObject_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            CompanyFactsQuery.Parse(null!));

        exception.ParamName.Should().Be("jo");
    }

    [Fact(DisplayName = "Parse handles multiple fact types and tags")]
    public void Parse_MultipleFacts_ParsesAllFacts()
    {
        var json = @"{
            ""cik"": ""123456"",
            ""entityName"": ""Test Corp"",
            ""facts"": {
                ""us-gaap"": {
                    ""Revenue"": {
                        ""label"": ""Revenue"",
                        ""units"": { ""USD"": [] }
                    },
                    ""NetIncome"": {
                        ""label"": ""Net Income"",
                        ""units"": { ""USD"": [] }
                    }
                },
                ""dei"": {
                    ""EntityCommonStockSharesOutstanding"": {
                        ""label"": ""Shares Outstanding"",
                        ""units"": { ""shares"": [] }
                    }
                }
            }
        }";
        var jObject = JObject.Parse(json);

        var result = CompanyFactsQuery.Parse(jObject);

        result.Facts.Should().HaveCount(3);
        result.Facts!.Select(f => f.Tag).Should().Contain(new[] { "Revenue", "NetIncome", "EntityCommonStockSharesOutstanding" });
    }

    [Fact(DisplayName = "Parse handles missing CIK using TryGetValue")]
    public void Parse_MissingCIK_DefaultsToZero()
    {
        var json = @"{
            ""entityName"": ""Test Company"",
            ""facts"": {}
        }";
        var jObject = JObject.Parse(json);

        var result = CompanyFactsQuery.Parse(jObject);

        result.CIK.Should().Be(0);
    }

    [Fact(DisplayName = "Parse handles missing EntityName")]
    public void Parse_MissingEntityName_ReturnsNull()
    {
        var json = @"{
            ""cik"": ""111111"",
            ""facts"": {}
        }";
        var jObject = JObject.Parse(json);

        var result = CompanyFactsQuery.Parse(jObject);

        result.EntityName.Should().BeNull();
    }

    [Fact(DisplayName = "Parse handles missing facts property")]
    public void Parse_MissingFacts_ReturnsEmptyArray()
    {
        var json = @"{
            ""cik"": ""222222"",
            ""entityName"": ""Empty Facts Corp""
        }";
        var jObject = JObject.Parse(json);

        var result = CompanyFactsQuery.Parse(jObject);

        result.Facts.Should().NotBeNull();
        result.Facts.Should().BeEmpty();
    }

    [Fact(DisplayName = "Parse throws InvalidOperationException for invalid CIK format")]
    public void Parse_InvalidCIK_ThrowsInvalidOperationException()
    {
        var json = @"{
            ""cik"": ""not-a-number"",
            ""facts"": {}
        }";
        var jObject = JObject.Parse(json);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            CompanyFactsQuery.Parse(jObject));

        exception.Message.Should().Contain("Failed to parse company facts data");
    }
}
