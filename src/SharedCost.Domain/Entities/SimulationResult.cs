namespace SharedCost.Domain.Entities;

public class SimulationResult
{
    public string MaSimulationResult { get; set; } = string.Empty;
    public string MaSimulation { get; set; } = string.Empty;
    public string MaRuleVersion { get; set; } = string.Empty;
    public string MaDriver { get; set; } = string.Empty;
    public string MaTarget { get; set; } = string.Empty;
    public string KyPhanBo { get; set; } = string.Empty;
    public string MaDriverValue { get; set; } = string.Empty;
    public decimal GiaTriDriver { get; set; }
    public decimal TongDriver { get; set; }
    public decimal TyLePhanBo { get; set; }
    public decimal SoTienNguon { get; set; }
    public decimal SoTienPhanBo { get; set; }

    public virtual Simulation? Simulation { get; set; }
    public virtual RuleVersionTarget? RuleVersionTarget { get; set; }
    public virtual DriverValue? DriverValue { get; set; }
}
