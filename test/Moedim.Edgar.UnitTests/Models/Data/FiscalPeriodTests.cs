using Moedim.Edgar.Models.Data;

namespace Moedim.Edgar.UnitTests.Models.Data;

public class FiscalPeriodTests
{
    [Theory(DisplayName = "FiscalPeriod enum has correct values")]
    [InlineData(FiscalPeriod.FiscalYear, 0)]
    [InlineData(FiscalPeriod.Q1, 1)]
    [InlineData(FiscalPeriod.Q2, 2)]
    [InlineData(FiscalPeriod.Q3, 3)]
    [InlineData(FiscalPeriod.Q4, 4)]
    public void FiscalPeriod_EnumValues_AreCorrect(FiscalPeriod period, int expectedValue)
    {
        ((int)period).Should().Be(expectedValue);
    }

    [Fact(DisplayName = "FiscalPeriod enum can be cast from int")]
    public void FiscalPeriod_CastFromInt_ReturnsCorrectValue()
    {
        FiscalPeriod period = (FiscalPeriod)2;

        period.Should().Be(FiscalPeriod.Q2);
    }

    [Fact(DisplayName = "FiscalPeriod enum ToString returns name")]
    public void FiscalPeriod_ToString_ReturnsEnumName()
    {
        var period = FiscalPeriod.Q3;

        period.ToString().Should().Be("Q3");
    }

    [Fact(DisplayName = "FiscalPeriod FiscalYear is default value")]
    public void FiscalPeriod_DefaultValue_IsFiscalYear()
    {
        FiscalPeriod defaultPeriod = default;

        defaultPeriod.Should().Be(FiscalPeriod.FiscalYear);
    }
}
