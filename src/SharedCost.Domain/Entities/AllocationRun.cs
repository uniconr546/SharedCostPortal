namespace SharedCost.Domain.Entities;

public class AllocationRun
{
    public string MaRun { get; set; } = string.Empty;
    public string MaCostPool { get; set; } = string.Empty;
    public string MaRule { get; set; } = string.Empty;
    public string MaDriver { get; set; } = string.Empty;
    public string MaRuleVersion { get; set; } = string.Empty;
    public DateTime ThoiGianChay { get; set; } = DateTime.Now;
    public string NguoiThucHien { get; set; } = string.Empty;
    public string KyPhanBo { get; set; } = string.Empty;
    public decimal TongTienNguon { get; set; }
    public decimal TongTienPhanBo { get; set; }
    public string TrangThai { get; set; } = "DRAFT";

    public virtual RuleVersion? RuleVersion { get; set; }
    public virtual CostPool? CostPool { get; set; }
    public virtual NguoiDung? NguoiThucHienNavigation { get; set; }
    public virtual ICollection<AllocationResult> AllocationResults { get; set; } = new List<AllocationResult>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual Lock? Lock { get; set; }
}
