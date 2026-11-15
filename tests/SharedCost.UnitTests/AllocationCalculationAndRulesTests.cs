using SharedCost.Application.Common.Validation;
using SharedCost.Application.Services;
using SharedCost.Domain.Exceptions;
using Xunit;

namespace SharedCost.UnitTests;

public class AllocationCalculationAndRulesTests
{
    [Fact]
    public void Calculate_ProportionalDriverValues_ShouldReturnExactAmounts()
    {
        decimal poolAmount = 100_000_000m;
        var inputs = new List<CalculationTargetInput>
        {
            new("TGT_PBA", 1, "DV_01", 400m),
            new("TGT_PBB", 2, "DV_02", 350m),
            new("TGT_PBC", 3, "DV_03", 250m)
        };

        var results = AllocationCalculationEngine.Calculate(poolAmount, inputs);

        Assert.Equal(3, results.Count);
        Assert.Equal(1.000000m, results.Sum(r => r.TyLePhanBo));
        Assert.Equal(100_000_000m, results.Sum(r => r.SoTienPhanBo));

        var pba = results.First(r => r.MaTarget == "TGT_PBA");
        Assert.Equal(0.400000m, pba.TyLePhanBo);
        Assert.Equal(40_000_000m, pba.SoTienPhanBo);
    }

    [Fact]
    public void Calculate_RoundingRemainderWithThreeEqualTargets_ShouldReconcileExactTotal_BR10()
    {
        decimal poolAmount = 100_000_000m;
        var inputs = new List<CalculationTargetInput>
        {
            new("TGT_PBA", 1, "DV_NS_1", 10m),
            new("TGT_PBB", 2, "DV_NS_2", 10m),
            new("TGT_PBC", 3, "DV_NS_3", 10m)
        };

        var results = AllocationCalculationEngine.Calculate(poolAmount, inputs);

        Assert.Equal(1.000000m, results.Sum(r => r.TyLePhanBo));
        Assert.Equal(100_000_000m, results.Sum(r => r.SoTienPhanBo));

        Assert.Equal(0.333334m, results[0].TyLePhanBo);
        Assert.Equal(33_333_333.34m, results[0].SoTienPhanBo);

        Assert.Equal(0.333333m, results[1].TyLePhanBo);
        Assert.Equal(33_333_333.33m, results[1].SoTienPhanBo);

        Assert.Equal(0.333333m, results[2].TyLePhanBo);
        Assert.Equal(33_333_333.33m, results[2].SoTienPhanBo);
    }

    [Fact]
    public void Calculate_WhenTotalDriverIsZero_ShouldThrowDomainException_BR06()
    {
        var inputs = new List<CalculationTargetInput>
        {
            new("TGT_PBA", 1, "DV_1", 0m),
            new("TGT_PBB", 2, "DV_2", 0m)
        };

        var ex = Assert.Throws<DomainException>(() =>
            AllocationCalculationEngine.Calculate(50_000_000m, inputs));

        Assert.Equal("BR06_ZERO_TOTAL_DRIVER", ex.ErrorCode);
    }

    [Fact]
    public void Calculate_WhenDriverValueIsNegative_ShouldThrowDomainException_BR04()
    {
        var inputs = new List<CalculationTargetInput>
        {
            new("TGT_PBA", 1, "DV_1", -5m),
            new("TGT_PBB", 2, "DV_2", 20m)
        };

        var ex = Assert.Throws<DomainException>(() =>
            AllocationCalculationEngine.Calculate(50_000_000m, inputs));

        Assert.Equal("BR04_NEGATIVE_DRIVER_VALUE", ex.ErrorCode);
    }

    [Theory]
    [InlineData("2026-13")]
    [InlineData("2026-00")]
    [InlineData("2026/10")]
    [InlineData("10-2026")]
    public void ValidatePeriod_WhenInvalidFormat_ShouldThrowDomainException_BR02(string invalidPeriod)
    {
        var ex = Assert.Throws<DomainException>(() =>
            PeriodValidator.ValidateAndParsePeriod(invalidPeriod));

        Assert.StartsWith("BR02_", ex.ErrorCode);
    }

    [Fact]
    public void EnsureRuleVersionCoversPeriod_WhenExpiredOrOutOfRange_ShouldThrow_BR13()
    {
        var ex = Assert.Throws<DomainException>(() =>
            PeriodValidator.EnsureRuleVersionCoversPeriod(
                "RV_TEST",
                "ACTIVE",
                new DateTime(2026, 5, 1),
                new DateTime(2026, 12, 31),
                "2026-04"));

        Assert.Equal("BR13_PERIOD_OUT_OF_RANGE", ex.ErrorCode);
    }
}
