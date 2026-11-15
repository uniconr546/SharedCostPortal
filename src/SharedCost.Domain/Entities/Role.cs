namespace SharedCost.Domain.Entities;

public class Role
{
    public string MaRole { get; set; } = string.Empty;
    public string TenRole { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string TrangThai { get; set; } = "ACTIVE";

    public virtual ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
}
