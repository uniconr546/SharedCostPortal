namespace SharedCost.Domain.Entities;

public class AllocationResult
{
    public string MaResult { get; set; } = string.Empty;
    public string MaRun { get; set; } = string.Empty;
    public string MaRule { get; set; } = string.Empty;
    public string MaRuleVersion { get; set; } = string.Empty;
    public string MaTarget { get; set; } = string.Empty;
    public string MaDriver { get; set; } = string.Empty;
    public string KyPhanBo { get; set; } = string.Empty;
    public string MaDriverValue { get; set; } = string.Empty;
    public decimal GiaTriDriver { get; set; }
    public decimal TongDriver { get; set; }
    public decimal TyLePhanBo { get; set; }
    public decimal SoTienNguon { get; set; }
    public decimal SoTienPhanBo { get; set; }
    public string TrangThai { get; set; } = "CALCULATED";

    public virtual AllocationRun? AllocationRun { get; set; }
    public virtual RuleVersionTarget? RuleVersionTarget { get; set; }
    public virtual DriverValue? DriverValue { get; set; }
}
