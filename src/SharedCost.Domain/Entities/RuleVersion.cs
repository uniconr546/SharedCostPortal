namespace SharedCost.Domain.Entities;

public class RuleVersion
{
    public string MaRuleVersion { get; set; } = string.Empty;
    public string MaRule { get; set; } = string.Empty;
    public string MaCostPool { get; set; } = string.Empty;
    public string MaDriver { get; set; } = string.Empty;
    public int SoPhienBan { get; set; }
    public DateTime NgayHieuLuc { get; set; }
    public DateTime? NgayHetHieuLuc { get; set; }
    public string CongThuc { get; set; } = string.Empty;
    public int ThuTuThucHien { get; set; } = 1;
    public string? ThamSoJson { get; set; }
    public string TrangThai { get; set; } = "DRAFT";
    public string NguoiTao { get; set; } = string.Empty;

    public virtual AllocationRule? AllocationRule { get; set; }
    public virtual CostPool? CostPool { get; set; }
    public virtual Driver? Driver { get; set; }
    public virtual NguoiDung? NguoiTaoNavigation { get; set; }
    public virtual ICollection<RuleVersionTarget> RuleVersionTargets { get; set; } = new List<RuleVersionTarget>();
    public virtual ICollection<Simulation> Simulations { get; set; } = new List<Simulation>();
    public virtual ICollection<AllocationRun> AllocationRuns { get; set; } = new List<AllocationRun>();
}
