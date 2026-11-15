using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using SharedCost.Domain.Exceptions;

namespace SharedCost.Application.Common.Validation;

public static class PeriodValidator
{
    private static readonly Regex PeriodRegex = new(@"^[1-2][0-9]{3}-[0-1][0-9]$", RegexOptions.Compiled);

    public static (DateTime FirstDay, DateTime LastDay) ValidateAndParsePeriod(string? period, string fieldName = "KyPhanBo")
    {
        if (string.IsNullOrWhiteSpace(period) || period.Length != 7 || !PeriodRegex.IsMatch(period))
        {
            throw new DomainException(
                $"Trường {fieldName} ('{period}') không đúng định dạng kỳ kế toán 'YYYY-MM' (ví dụ: '2026-10').",
                "BR02_INVALID_PERIOD_FORMAT");
        }

        if (!DateTime.TryParseExact(
                period + "-01",
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var firstDay))
        {
            throw new DomainException(
                $"Trường {fieldName} ('{period}') không phải là tháng hợp lệ trong năm.",
                "BR02_INVALID_PERIOD_DATE");
        }

        var lastDay = new DateTime(firstDay.Year, firstDay.Month, DateTime.DaysInMonth(firstDay.Year, firstDay.Month));
        return (firstDay, lastDay);
    }

    public static void EnsureRuleVersionCoversPeriod(
        string maRuleVersion,
        string trangThaiRuleVersion,
        DateTime ngayHieuLuc,
        DateTime? ngayHetHieuLuc,
        string kyPhanBo)
    {
        var (firstDay, lastDay) = ValidateAndParsePeriod(kyPhanBo, nameof(kyPhanBo));

        if (!string.Equals(trangThaiRuleVersion, "ACTIVE", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException(
                $"Lỗi BR13: Phiên bản quy tắc '{maRuleVersion}' đang ở trạng thái '{trangThaiRuleVersion}', chỉ phiên bản 'ACTIVE' mới được phép chạy phân bổ chính thức.",
                "BR13_RULE_VERSION_NOT_ACTIVE");
        }

        if (ngayHieuLuc.Date > firstDay.Date || (ngayHetHieuLuc.HasValue && ngayHetHieuLuc.Value.Date < lastDay.Date))
        {
            throw new DomainException(
                $"Lỗi BR13: Phiên bản quy tắc '{maRuleVersion}' (Hiệu lực: {ngayHieuLuc:yyyy-MM-dd} đến {(ngayHetHieuLuc?.ToString("yyyy-MM-dd") ?? "Vô thời hạn")}) không bao phủ trọn vẹn kỳ phân bổ '{kyPhanBo}' ({firstDay:yyyy-MM-dd} đến {lastDay:yyyy-MM-dd}).",
                "BR13_PERIOD_OUT_OF_RANGE");
        }
    }

    public static void ValidateOptionalJson(string? jsonString, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return;

        try
        {
            using var _ = JsonDocument.Parse(jsonString);
        }
        catch (JsonException)
        {
            throw new DomainException(
                $"Trường {fieldName} không phải là chuỗi JSON hợp lệ.",
                "INVALID_JSON_FORMAT");
        }
    }
}
