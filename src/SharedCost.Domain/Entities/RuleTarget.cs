namespace SharedCost.Domain.Entities;

public class RuleTarget
{
    public string MaRule { get; set; } = string.Empty;
    public string MaTarget { get; set; } = string.Empty;
    public int ThuTuTarget { get; set; } = 1;
    public decimal? TyLeMacDinh { get; set; }
    public string? ThamSoJson { get; set; }

    public virtual AllocationRule? AllocationRule { get; set; }
    public virtual Target? Target { get; set; }
}
