namespace SharedCost.Domain.Entities;

public class NguoiDung
{
    public string MaNguoiDung { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string TenDangNhap { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string OidcIssuer { get; set; } = string.Empty;
    public string OidcSubject { get; set; } = string.Empty;
    public string MaRole { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "ACTIVE";

    public virtual Role? Role { get; set; }
    public virtual ICollection<RuleVersion> RuleVersions { get; set; } = new List<RuleVersion>();
    public virtual ICollection<Simulation> Simulations { get; set; } = new List<Simulation>();
    public virtual ICollection<AllocationRun> AllocationRuns { get; set; } = new List<AllocationRun>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Lock> Locks { get; set; } = new List<Lock>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
