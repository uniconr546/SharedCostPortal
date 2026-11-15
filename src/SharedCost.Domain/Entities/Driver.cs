namespace SharedCost.Domain.Entities;

public class Driver
{
    public string MaDriver { get; set; } = string.Empty;
    public string TenDriver { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string TrangThai { get; set; } = "ACTIVE";

    public virtual ICollection<DriverValue> DriverValues { get; set; } = new List<DriverValue>();
    public virtual ICollection<AllocationRule> AllocationRules { get; set; } = new List<AllocationRule>();
    public virtual ICollection<RuleVersion> RuleVersions { get; set; } = new List<RuleVersion>();
}
