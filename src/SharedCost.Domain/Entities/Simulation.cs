namespace SharedCost.Domain.Entities;

public class Simulation
{
    public string MaSimulation { get; set; } = string.Empty;
    public string MaRuleVersion { get; set; } = string.Empty;
    public string MaRule { get; set; } = string.Empty;
    public string MaCostPool { get; set; } = string.Empty;
    public string MaDriver { get; set; } = string.Empty;
    public DateTime ThoiGianChay { get; set; } = DateTime.Now;
    public string NguoiThucHien { get; set; } = string.Empty;
    public string KyPhanBo { get; set; } = string.Empty;
    public decimal TongTienNguon { get; set; }
    public string TrangThai { get; set; } = "RUNNING";
    public string? GhiChu { get; set; }

    public virtual RuleVersion? RuleVersion { get; set; }
    public virtual CostPool? CostPool { get; set; }
    public virtual NguoiDung? NguoiThucHienNavigation { get; set; }
    public virtual ICollection<SimulationResult> SimulationResults { get; set; } = new List<SimulationResult>();
}
