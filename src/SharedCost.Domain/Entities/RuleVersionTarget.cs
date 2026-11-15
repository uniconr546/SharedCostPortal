namespace SharedCost.Domain.Entities;

public class RuleVersionTarget
{
    public string MaRuleVersion { get; set; } = string.Empty;
    public string MaRule { get; set; } = string.Empty;
    public string MaTarget { get; set; } = string.Empty;
    public int ThuTuTarget { get; set; } = 1;
    public decimal? TyLeMacDinh { get; set; }
    public string? ThamSoJson { get; set; }

    public virtual RuleVersion? RuleVersion { get; set; }
    public virtual Target? Target { get; set; }
    public virtual ICollection<SimulationResult> SimulationResults { get; set; } = new List<SimulationResult>();
    public virtual ICollection<AllocationResult> AllocationResults { get; set; } = new List<AllocationResult>();
}
