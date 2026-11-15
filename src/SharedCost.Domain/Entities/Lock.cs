namespace SharedCost.Domain.Entities;

public class Lock
{
    public string MaLock { get; set; } = string.Empty;
    public string MaRun { get; set; } = string.Empty;
    public string NguoiKhoa { get; set; } = string.Empty;
    public DateTime ThoiGianKhoa { get; set; } = DateTime.Now;
    public string? LyDoKhoa { get; set; }

    public virtual AllocationRun? AllocationRun { get; set; }
    public virtual NguoiDung? NguoiKhoaNavigation { get; set; }
}
