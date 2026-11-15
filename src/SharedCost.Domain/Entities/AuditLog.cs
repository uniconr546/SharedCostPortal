namespace SharedCost.Domain.Entities;

public class AuditLog
{
    public long MaAuditLog { get; set; }
    public string MaNguoiDung { get; set; } = string.Empty;
    public DateTime ThoiGian { get; set; } = DateTime.Now;
    public string HanhDong { get; set; } = string.Empty;
    public string DoiTuong { get; set; } = string.Empty;
    public string? MaDoiTuong { get; set; }
    public string? NoiDung { get; set; }
    public string? GiaTriCu { get; set; }
    public string? GiaTriMoi { get; set; }
    public string? DiaChiIP { get; set; }

    public virtual NguoiDung? NguoiDung { get; set; }
}
