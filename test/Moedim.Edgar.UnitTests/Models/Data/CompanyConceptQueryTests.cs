using Moedim.Edgar.Models.Data;
using Newtonsoft.Json.Linq;

namespace Moedim.Edgar.UnitTests.Models.Data;

public class CompanyConceptQueryTests
{
    [Fact(DisplayName = "CompanyConceptQuery properties can be set and retrieved")]
    public void CompanyConceptQuery_PropertiesSetAndGet()
    {
        var query = new CompanyConceptQuery
        {
            CIK = 1234567,
            EntityName = "Test Company",
            Result = new Fact { Tag = "TestTag" }
        };

        query.CIK.Should().Be(1234567);
        query.EntityName.Should().Be("Test Company");
        query.Result.Should().NotBeNull();
        query.Result!.Tag.Should().Be("TestTag");
    }

    [Fact(DisplayName = "Parse returns valid CompanyConceptQuery from JObject")]
    public void Parse_ValidJObject_ReturnsCompanyConceptQuery()
    {
        var json = @"{
            ""cik"": ""1234567"",
            ""entityName"": ""Apple Inc."",
            ""tag"": ""AccountsPayableCurrent"",
            ""label"": ""Accounts Payable"",
            ""description"": ""Carrying value as of the balance sheet date"",
            ""units"": {
                ""USD"": []
            }
        }";
        var jObject = JObject.Parse(json);

        var result = CompanyConceptQuery.Parse(jObject);

        result.Should().NotBeNull();
        result.CIK.Should().Be(1234567);
        result.EntityName.Should().Be("Apple Inc.");
        result.Result.Should().NotBeNull();
    }

    [Fact(DisplayName = "Parse throws ArgumentNullException when JObject is null")]
    public void Parse_NullJObject_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            CompanyConceptQuery.Parse(null!));

        exception.ParamName.Should().Be("jo");
    }

    [Fact(DisplayName = "Parse handles missing CIK property")]
    public void Parse_MissingCIK_ReturnsQueryWithDefaultCIK()
    {
        var json = @"{
            ""entityName"": ""Test Company"",
            ""tag"": ""TestTag"",
            ""units"": {}
        }";
        var jObject = JObject.Parse(json);

        var result = CompanyConceptQuery.Parse(jObject);

        result.Should().NotBeNull();
        result.CIK.Should().Be(0);
        result.EntityName.Should().Be("Test Company");
    }

    [Fact(DisplayName = "Parse handles missing EntityName property")]
    public void Parse_MissingEntityName_ReturnsQueryWithNullEntityName()
    {
        var json = @"{
            ""cik"": ""999999"",
            ""tag"": ""TestTag"",
            ""units"": {}
        }";
        var jObject = JObject.Parse(json);

        var result = CompanyConceptQuery.Parse(jObject);

        result.Should().NotBeNull();
        result.CIK.Should().Be(999999);
        result.EntityName.Should().BeNull();
    }

    [Fact(DisplayName = "Parse throws InvalidOperationException for invalid CIK format")]
    public void Parse_InvalidCIKFormat_ThrowsInvalidOperationException()
    {
        var json = @"{
            ""cik"": ""invalid"",
            ""entityName"": ""Test Company"",
            ""units"": {}
        }";
        var jObject = JObject.Parse(json);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            CompanyConceptQuery.Parse(jObject));

        exception.Message.Should().Contain("Failed to parse company concept data");
        exception.InnerException.Should().NotBeNull();
    }
}
