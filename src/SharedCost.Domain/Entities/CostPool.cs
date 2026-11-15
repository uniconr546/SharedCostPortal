namespace SharedCost.Domain.Entities;

public class CostPool
{
    public string MaCostPool { get; set; } = string.Empty;
    public string TenCostPool { get; set; } = string.Empty;
    public string KyApDung { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string? NguonDuLieu { get; set; }
    public string? MoTa { get; set; }
    public string TrangThai { get; set; } = "ACTIVE";

    public virtual ICollection<AllocationRule> AllocationRules { get; set; } = new List<AllocationRule>();
    public virtual ICollection<RuleVersion> RuleVersions { get; set; } = new List<RuleVersion>();
    public virtual ICollection<Simulation> Simulations { get; set; } = new List<Simulation>();
    public virtual ICollection<AllocationRun> AllocationRuns { get; set; } = new List<AllocationRun>();
}
