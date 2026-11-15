using SharedCost.Domain.Common;
using SharedCost.Domain.Exceptions;

namespace SharedCost.Application.Services;

public record CalculationTargetInput(
    string MaTarget,
    int ThuTuTarget,
    string MaDriverValue,
    decimal GiaTriDriver);

public record CalculatedTargetAllocation(
    string MaTarget,
    int ThuTuTarget,
    string MaDriverValue,
    decimal GiaTriDriver,
    decimal TongDriver,
    decimal TyLePhanBo,
    decimal SoTienNguon,
    decimal SoTienPhanBo);

public static class AllocationCalculationEngine
{
    public static List<CalculatedTargetAllocation> Calculate(
        decimal tongTienNguon,
        IReadOnlyList<CalculationTargetInput> targets)
    {
        if (tongTienNguon < 0)
        {
            throw new DomainException(
                "Tổng số tiền nguồn của khoản chi phí không được nhỏ hơn 0.",
                "BR02_NEGATIVE_COST_POOL");
        }

        if (targets == null || targets.Count == 0)
        {
            throw new DomainException(
                "Danh sách đối tượng nhận chi phí trong phiên bản quy tắc không được để trống.",
                "BR05_EMPTY_TARGETS");
        }

        if (targets.Any(t => t.GiaTriDriver < 0))
        {
            throw new DomainException(
                "Giá trị tiêu thức phân bổ không được âm.",
                "BR04_NEGATIVE_DRIVER_VALUE");
        }

        var tongDriver = Math.Round(targets.Sum(t => t.GiaTriDriver), CalculationRules.DriverDecimalPlaces);
        if (tongDriver <= 0)
        {
            throw new DomainException(
                "Tổng giá trị tiêu thức của các đối tượng hợp lệ phải lớn hơn 0 trước khi tính phân bổ.",
                "BR06_ZERO_TOTAL_DRIVER");
        }

        const decimal ratioScaleFactor = 1_000_000m;
        var workingItems = targets.Select(t =>
        {
            decimal exactRatio = t.GiaTriDriver / tongDriver;
            decimal scaledRatio = exactRatio * ratioScaleFactor;
            decimal flooredScaledRatio = Math.Floor(scaledRatio);
            decimal ratioRemainder = scaledRatio - flooredScaledRatio;

            decimal exactAmount = tongTienNguon * exactRatio;
            decimal scaledAmount = exactAmount * 100m;
            decimal flooredScaledAmount = Math.Floor(scaledAmount);
            decimal amountRemainder = scaledAmount - flooredScaledAmount;

            return new WorkingAllocationItem
            {
                Input = t,
                FlooredRatio = flooredScaledRatio / ratioScaleFactor,
                RatioRemainder = ratioRemainder,
                FlooredAmount = flooredScaledAmount / 100m,
                AmountRemainder = amountRemainder
            };
        }).ToList();

        // Phân bổ phần dư làm tròn tỷ lệ (đảm bảo SUM(TyLePhanBo) == 1.000000)
        decimal currentRatioSum = workingItems.Sum(x => x.FlooredRatio);
        int missingRatioSteps = (int)Math.Round((CalculationRules.ExpectedTotalRatio - currentRatioSum) * ratioScaleFactor, 0);

        if (missingRatioSteps > 0)
        {
            var orderedForRatio = workingItems
                .OrderByDescending(x => x.RatioRemainder)
                .ThenBy(x => x.Input.ThuTuTarget)
                .ThenBy(x => x.Input.MaTarget, StringComparer.Ordinal)
                .ToList();

            for (int i = 0; i < missingRatioSteps; i++)
            {
                orderedForRatio[i % orderedForRatio.Count].FlooredRatio += 0.000001m;
            }
        }

        // Phân bổ phần dư làm tròn số tiền (BR10: đảm bảo SUM(SoTienPhanBo) == TongTienNguon)
        decimal currentAmountSum = workingItems.Sum(x => x.FlooredAmount);
        long missingAmountSteps = (long)Math.Round((tongTienNguon - currentAmountSum) * 100m, 0);

        if (missingAmountSteps > 0)
        {
            var orderedForAmount = workingItems
                .OrderByDescending(x => x.AmountRemainder)
                .ThenBy(x => x.Input.ThuTuTarget)
                .ThenBy(x => x.Input.MaTarget, StringComparer.Ordinal)
                .ToList();

            for (long i = 0; i < missingAmountSteps; i++)
            {
                orderedForAmount[(int)(i % orderedForAmount.Count)].FlooredAmount += 0.01m;
            }
        }

        var finalResults = workingItems
            .OrderBy(x => x.Input.ThuTuTarget)
            .ThenBy(x => x.Input.MaTarget, StringComparer.Ordinal)
            .Select(x => new CalculatedTargetAllocation(
                x.Input.MaTarget,
                x.Input.ThuTuTarget,
                x.Input.MaDriverValue,
                Math.Round(x.Input.GiaTriDriver, CalculationRules.DriverDecimalPlaces),
                tongDriver,
                Math.Round(x.FlooredRatio, CalculationRules.RatioDecimalPlaces),
                Math.Round(tongTienNguon, CalculationRules.AmountDecimalPlaces),
                Math.Round(x.FlooredAmount, CalculationRules.AmountDecimalPlaces)))
            .ToList();

        decimal finalRatioSum = finalResults.Sum(x => x.TyLePhanBo);
        decimal finalAmountSum = finalResults.Sum(x => x.SoTienPhanBo);

        if (finalRatioSum != CalculationRules.ExpectedTotalRatio || finalAmountSum != tongTienNguon)
        {
            throw new DomainException(
                $"Tổng tỷ lệ ({finalRatioSum:F6}) khác 1.000000 hoặc tổng tiền phân bổ ({finalAmountSum:N2}) không khớp tổng tiền nguồn ({tongTienNguon:N2}).",
                "BR10_RECONCILIATION_FAILED");
        }

        return finalResults;
    }

    private sealed class WorkingAllocationItem
    {
        public required CalculationTargetInput Input { get; init; }
        public decimal FlooredRatio { get; set; }
        public decimal RatioRemainder { get; set; }
        public decimal FlooredAmount { get; set; }
        public decimal AmountRemainder { get; set; }
    }
}
