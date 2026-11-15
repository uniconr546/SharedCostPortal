namespace SharedCost.Domain.Entities;

public class AllocationRule
{
    public string MaRule { get; set; } = string.Empty;
    public string TenRule { get; set; } = string.Empty;
    public string MaCostPool { get; set; } = string.Empty;
    public string MaDriver { get; set; } = string.Empty;
    public string CongThuc { get; set; } = string.Empty;
    public int ThuTuThucHien { get; set; } = 1;
    public string TrangThai { get; set; } = "DRAFT";
    public string? MoTa { get; set; }

    public virtual CostPool? CostPool { get; set; }
    public virtual Driver? Driver { get; set; }
    public virtual ICollection<RuleTarget> RuleTargets { get; set; } = new List<RuleTarget>();
    public virtual ICollection<RuleVersion> RuleVersions { get; set; } = new List<RuleVersion>();
}
