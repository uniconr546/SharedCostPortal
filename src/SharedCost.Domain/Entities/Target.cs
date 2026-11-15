namespace SharedCost.Domain.Entities;

public class Target
{
    public string MaTarget { get; set; } = string.Empty;
    public string TenTarget { get; set; } = string.Empty;
    public string LoaiTarget { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string TrangThai { get; set; } = "ACTIVE";

    public virtual ICollection<DriverValue> DriverValues { get; set; } = new List<DriverValue>();
    public virtual ICollection<RuleTarget> RuleTargets { get; set; } = new List<RuleTarget>();
    public virtual ICollection<RuleVersionTarget> RuleVersionTargets { get; set; } = new List<RuleVersionTarget>();
}
