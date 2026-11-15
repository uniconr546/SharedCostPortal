namespace SharedCost.Domain.Entities;

public class Review
{
    public string MaReview { get; set; } = string.Empty;
    public string MaRun { get; set; } = string.Empty;
    public int LanDuyet { get; set; }
    public string NguoiDuyet { get; set; } = string.Empty;
    public DateTime ThoiGianDuyet { get; set; } = DateTime.Now;
    public string TrangThai { get; set; } = string.Empty;
    public string? NhanXet { get; set; }

    public virtual AllocationRun? AllocationRun { get; set; }
    public virtual NguoiDung? NguoiDuyetNavigation { get; set; }
}
