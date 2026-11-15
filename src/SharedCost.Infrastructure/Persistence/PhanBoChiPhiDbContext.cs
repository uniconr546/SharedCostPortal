using Microsoft.EntityFrameworkCore;
using SharedCost.Application.Common.Interfaces;
using SharedCost.Domain.Entities;

namespace SharedCost.Infrastructure.Persistence;

public class PhanBoChiPhiDbContext : DbContext, IPhanBoChiPhiDbContext
{
    public PhanBoChiPhiDbContext(DbContextOptions<PhanBoChiPhiDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
    public DbSet<CostPool> CostPools => Set<CostPool>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Target> Targets => Set<Target>();
    public DbSet<DriverValue> DriverValues => Set<DriverValue>();
    public DbSet<AllocationRule> AllocationRules => Set<AllocationRule>();
    public DbSet<RuleTarget> RuleTargets => Set<RuleTarget>();
    public DbSet<RuleVersion> RuleVersions => Set<RuleVersion>();
    public DbSet<RuleVersionTarget> RuleVersionTargets => Set<RuleVersionTarget>();
    public DbSet<Simulation> Simulations => Set<Simulation>();
    public DbSet<SimulationResult> SimulationResults => Set<SimulationResult>();
    public DbSet<AllocationRun> AllocationRuns => Set<AllocationRun>();
    public DbSet<AllocationResult> AllocationResults => Set<AllocationResult>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Lock> Locks => Set<Lock>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role", "dbo");
            entity.HasKey(e => e.MaRole).HasName("PK_Role");
            entity.Property(e => e.MaRole).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.TenRole).HasMaxLength(100).IsRequired();
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).HasDefaultValue("ACTIVE");
            entity.HasIndex(e => e.TenRole, "UQ_Role_TenRole").IsUnique();
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.ToTable("NguoiDung", "dbo");
            entity.HasKey(e => e.MaNguoiDung).HasName("PK_NguoiDung");
            entity.Property(e => e.MaNguoiDung).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(150).IsRequired();
            entity.Property(e => e.TenDangNhap).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.OidcIssuer).HasMaxLength(300).IsRequired();
            entity.Property(e => e.OidcSubject).HasMaxLength(200).IsRequired();
            entity.Property(e => e.MaRole).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.TrangThai).HasMaxLength(30).IsUnicode(false).HasDefaultValue("ACTIVE");

            entity.HasIndex(e => e.TenDangNhap, "UQ_NguoiDung_TenDangNhap").IsUnique();
            entity.HasIndex(e => new { e.OidcIssuer, e.OidcSubject }, "UQ_NguoiDung_OIDC").IsUnique();

            entity.HasOne(d => d.Role)
                .WithMany(p => p.NguoiDungs)
                .HasForeignKey(d => d.MaRole)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_NguoiDung_Role");
        });

        modelBuilder.Entity<CostPool>(entity =>
        {
            entity.ToTable("CostPool", "dbo");
            entity.HasKey(e => e.MaCostPool).HasName("PK_CostPool");
            entity.Property(e => e.MaCostPool).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.TenCostPool).HasMaxLength(150).IsRequired();
            entity.Property(e => e.KyApDung).HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(e => e.SoTien).HasPrecision(18, 2);
            entity.Property(e => e.NguonDuLieu).HasMaxLength(200);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TrangThai).HasMaxLength(30).IsUnicode(false).HasDefaultValue("ACTIVE");

            entity.HasIndex(e => new { e.TenCostPool, e.KyApDung }, "UQ_CostPool_Ten_Ky").IsUnique();
            entity.HasIndex(e => new { e.MaCostPool, e.KyApDung }, "UQ_CostPool_Ma_Ky").IsUnique();
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.ToTable("Driver", "dbo");
            entity.HasKey(e => e.MaDriver).HasName("PK_Driver");
            entity.Property(e => e.MaDriver).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.TenDriver).HasMaxLength(150).IsRequired();
            entity.Property(e => e.DonViTinh).HasMaxLength(50).IsRequired();
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TrangThai).HasMaxLength(30).IsUnicode(false).HasDefaultValue("ACTIVE");

            entity.HasIndex(e => e.TenDriver, "UQ_Driver_TenDriver").IsUnique();
        });

        modelBuilder.Entity<Target>(entity =>
        {
            entity.ToTable("Target", "dbo");
            entity.HasKey(e => e.MaTarget).HasName("PK_Target");
            entity.Property(e => e.MaTarget).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.TenTarget).HasMaxLength(150).IsRequired();
            entity.Property(e => e.LoaiTarget).HasMaxLength(80).IsRequired();
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TrangThai).HasMaxLength(30).IsUnicode(false).HasDefaultValue("ACTIVE");

            entity.HasIndex(e => e.TenTarget, "UQ_Target_TenTarget").IsUnique();
        });

        modelBuilder.Entity<DriverValue>(entity =>
        {
            entity.ToTable("DriverValue", "dbo");
            entity.HasKey(e => e.MaDriverValue).HasName("PK_DriverValue");
            entity.Property(e => e.MaDriverValue).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.MaDriver).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaTarget).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.KyPhanBo).HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(e => e.GiaTriDriver).HasPrecision(18, 4);
            entity.Property(e => e.DonViTinh).HasMaxLength(50);
            entity.Property(e => e.NguonDuLieu).HasMaxLength(200);

            entity.HasIndex(e => new { e.MaDriver, e.MaTarget, e.KyPhanBo }, "UQ_DriverValue_Driver_Target_Ky").IsUnique();
            entity.HasIndex(e => new { e.MaDriverValue, e.MaDriver, e.MaTarget, e.KyPhanBo }, "UQ_DriverValue_Trace").IsUnique();

            entity.HasOne(d => d.Driver)
                .WithMany(p => p.DriverValues)
                .HasForeignKey(d => d.MaDriver)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DriverValue_Driver");

            entity.HasOne(d => d.Target)
                .WithMany(p => p.DriverValues)
                .HasForeignKey(d => d.MaTarget)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DriverValue_Target");
        });

        modelBuilder.Entity<AllocationRule>(entity =>
        {
            entity.ToTable("AllocationRule", "dbo");
            entity.HasKey(e => e.MaRule).HasName("PK_AllocationRule");
            entity.Property(e => e.MaRule).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.TenRule).HasMaxLength(150).IsRequired();
            entity.Property(e => e.MaCostPool).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaDriver).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.CongThuc).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.ThuTuThucHien).HasDefaultValue(1);
            entity.Property(e => e.TrangThai).HasMaxLength(30).IsUnicode(false).HasDefaultValue("DRAFT");
            entity.Property(e => e.MoTa).HasMaxLength(500);

            entity.HasIndex(e => e.TenRule, "UQ_AllocationRule_TenRule").IsUnique();

            entity.HasOne(d => d.CostPool)
                .WithMany(p => p.AllocationRules)
                .HasForeignKey(d => d.MaCostPool)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AllocationRule_CostPool");

            entity.HasOne(d => d.Driver)
                .WithMany(p => p.AllocationRules)
                .HasForeignKey(d => d.MaDriver)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AllocationRule_Driver");
        });

        modelBuilder.Entity<RuleTarget>(entity =>
        {
            entity.ToTable("RuleTarget", "dbo");
            entity.HasKey(e => new { e.MaRule, e.MaTarget }).HasName("PK_RuleTarget");
            entity.Property(e => e.MaRule).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.MaTarget).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.ThuTuTarget).HasDefaultValue(1);
            entity.Property(e => e.TyLeMacDinh).HasPrecision(18, 6);

            entity.HasOne(d => d.AllocationRule)
                .WithMany(p => p.RuleTargets)
                .HasForeignKey(d => d.MaRule)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RuleTarget_Rule");

            entity.HasOne(d => d.Target)
                .WithMany(p => p.RuleTargets)
                .HasForeignKey(d => d.MaTarget)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RuleTarget_Target");
        });

        modelBuilder.Entity<RuleVersion>(entity =>
        {
            entity.ToTable("RuleVersion", "dbo");
            entity.HasKey(e => e.MaRuleVersion).HasName("PK_RuleVersion");
            entity.Property(e => e.MaRuleVersion).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.MaRule).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaCostPool).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaDriver).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.NgayHieuLuc).HasColumnType("date");
            entity.Property(e => e.NgayHetHieuLuc).HasColumnType("date");
            entity.Property(e => e.CongThuc).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.TrangThai).HasMaxLength(30).IsUnicode(false).HasDefaultValue("DRAFT");
            entity.Property(e => e.NguoiTao).HasMaxLength(30).IsUnicode(false).IsRequired();

            entity.HasIndex(e => new { e.MaRule, e.SoPhienBan }, "UQ_RuleVersion_Rule_PhienBan").IsUnique();

            entity.HasOne(d => d.AllocationRule)
                .WithMany(p => p.RuleVersions)
                .HasForeignKey(d => d.MaRule)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RuleVersion_Rule");

            entity.HasOne(d => d.CostPool)
                .WithMany(p => p.RuleVersions)
                .HasForeignKey(d => d.MaCostPool)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RuleVersion_CostPool");

            entity.HasOne(d => d.Driver)
                .WithMany(p => p.RuleVersions)
                .HasForeignKey(d => d.MaDriver)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RuleVersion_Driver");

            entity.HasOne(d => d.NguoiTaoNavigation)
                .WithMany(p => p.RuleVersions)
                .HasForeignKey(d => d.NguoiTao)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RuleVersion_NguoiTao");
        });

        modelBuilder.Entity<RuleVersionTarget>(entity =>
        {
            entity.ToTable("RuleVersionTarget", "dbo");
            entity.HasKey(e => new { e.MaRuleVersion, e.MaTarget }).HasName("PK_RuleVersionTarget");
            entity.Property(e => e.MaRuleVersion).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.MaRule).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaTarget).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.TyLeMacDinh).HasPrecision(18, 6);

            entity.HasOne(d => d.RuleVersion)
                .WithMany(p => p.RuleVersionTargets)
                .HasForeignKey(d => d.MaRuleVersion)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RuleVersionTarget_Version");

            entity.HasOne(d => d.Target)
                .WithMany(p => p.RuleVersionTargets)
                .HasForeignKey(d => d.MaTarget)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RuleVersionTarget_Target");
        });

        modelBuilder.Entity<Simulation>(entity =>
        {
            entity.ToTable("Simulation", "dbo");
            entity.HasKey(e => e.MaSimulation).HasName("PK_Simulation");
            entity.Property(e => e.MaSimulation).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.MaRuleVersion).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaRule).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaCostPool).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaDriver).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.ThoiGianChay).HasPrecision(0);
            entity.Property(e => e.NguoiThucHien).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.KyPhanBo).HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(e => e.TongTienNguon).HasPrecision(18, 2);
            entity.Property(e => e.TrangThai).HasMaxLength(30).IsUnicode(false).HasDefaultValue("RUNNING");
            entity.Property(e => e.GhiChu).HasMaxLength(500);

            entity.HasOne(d => d.RuleVersion)
                .WithMany(p => p.Simulations)
                .HasForeignKey(d => d.MaRuleVersion)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Simulation_RuleVersion");

            entity.HasOne(d => d.CostPool)
                .WithMany(p => p.Simulations)
                .HasForeignKey(d => d.MaCostPool)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Simulation_CostPool_Ky");

            entity.HasOne(d => d.NguoiThucHienNavigation)
                .WithMany(p => p.Simulations)
                .HasForeignKey(d => d.NguoiThucHien)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Simulation_NguoiDung");
        });

        modelBuilder.Entity<SimulationResult>(entity =>
        {
            entity.ToTable("SimulationResult", "dbo");
            entity.HasKey(e => e.MaSimulationResult).HasName("PK_SimulationResult");
            entity.Property(e => e.MaSimulationResult).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.MaSimulation).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaRuleVersion).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaDriver).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaTarget).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.KyPhanBo).HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaDriverValue).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.GiaTriDriver).HasPrecision(18, 4);
            entity.Property(e => e.TongDriver).HasPrecision(18, 4);
            entity.Property(e => e.TyLePhanBo).HasPrecision(18, 6);
            entity.Property(e => e.SoTienNguon).HasPrecision(18, 2);
            entity.Property(e => e.SoTienPhanBo).HasPrecision(18, 2);

            entity.HasOne(d => d.Simulation)
                .WithMany(p => p.SimulationResults)
                .HasForeignKey(d => d.MaSimulation)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SimulationResult_Simulation");

            entity.HasOne(d => d.RuleVersionTarget)
                .WithMany(p => p.SimulationResults)
                .HasForeignKey(d => new { d.MaRuleVersion, d.MaTarget })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_SimulationResult_VersionTarget");

            entity.HasOne(d => d.DriverValue)
                .WithMany(p => p.SimulationResults)
                .HasForeignKey(d => d.MaDriverValue)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_SimulationResult_DriverValue");
        });

        // Khai báo HasTrigger cho 4 bảng có AFTER TRIGGER để EF Core không sinh mệnh đề OUTPUT trực tiếp
        modelBuilder.Entity<AllocationRun>(entity =>
        {
            entity.ToTable("AllocationRun", "dbo", tb => tb.HasTrigger("TRG_AllocationRun_ValidateAndStateMachine"));
            entity.HasKey(e => e.MaRun).HasName("PK_AllocationRun");
            entity.Property(e => e.MaRun).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.MaCostPool).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaRule).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaDriver).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaRuleVersion).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.ThoiGianChay).HasPrecision(0);
            entity.Property(e => e.NguoiThucHien).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.KyPhanBo).HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(e => e.TongTienNguon).HasPrecision(18, 2);
            entity.Property(e => e.TongTienPhanBo).HasPrecision(18, 2);
            entity.Property(e => e.TrangThai).HasMaxLength(30).IsUnicode(false).HasDefaultValue("DRAFT");

            entity.HasOne(d => d.RuleVersion)
                .WithMany(p => p.AllocationRuns)
                .HasForeignKey(d => d.MaRuleVersion)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AllocationRun_RuleVersion");

            entity.HasOne(d => d.CostPool)
                .WithMany(p => p.AllocationRuns)
                .HasForeignKey(d => d.MaCostPool)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AllocationRun_CostPool_Ky");

            entity.HasOne(d => d.NguoiThucHienNavigation)
                .WithMany(p => p.AllocationRuns)
                .HasForeignKey(d => d.NguoiThucHien)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AllocationRun_NguoiDung");
        });

        modelBuilder.Entity<AllocationResult>(entity =>
        {
            entity.ToTable("AllocationResult", "dbo", tb => tb.HasTrigger("TRG_AllocationResult_ProtectImmutability"));
            entity.HasKey(e => e.MaResult).HasName("PK_AllocationResult");
            entity.Property(e => e.MaResult).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.MaRun).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaRule).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaRuleVersion).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaTarget).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaDriver).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.KyPhanBo).HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaDriverValue).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.GiaTriDriver).HasPrecision(18, 4);
            entity.Property(e => e.TongDriver).HasPrecision(18, 4);
            entity.Property(e => e.TyLePhanBo).HasPrecision(18, 6);
            entity.Property(e => e.SoTienNguon).HasPrecision(18, 2);
            entity.Property(e => e.SoTienPhanBo).HasPrecision(18, 2);
            entity.Property(e => e.TrangThai).HasMaxLength(30).IsUnicode(false).HasDefaultValue("CALCULATED");

            entity.HasOne(d => d.AllocationRun)
                .WithMany(p => p.AllocationResults)
                .HasForeignKey(d => d.MaRun)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AllocationResult_Run");

            entity.HasOne(d => d.RuleVersionTarget)
                .WithMany(p => p.AllocationResults)
                .HasForeignKey(d => new { d.MaRuleVersion, d.MaTarget })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AllocationResult_VersionTarget");

            entity.HasOne(d => d.DriverValue)
                .WithMany(p => p.AllocationResults)
                .HasForeignKey(d => d.MaDriverValue)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AllocationResult_DriverValue");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("Review", "dbo", tb => tb.HasTrigger("TRG_Review_ValidateAndSyncRun"));
            entity.HasKey(e => e.MaReview).HasName("PK_Review");
            entity.Property(e => e.MaReview).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.MaRun).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.NguoiDuyet).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.ThoiGianDuyet).HasPrecision(0);
            entity.Property(e => e.TrangThai).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.NhanXet).HasMaxLength(1000);

            entity.HasIndex(e => new { e.MaRun, e.LanDuyet }, "UQ_Review_Run_LanDuyet").IsUnique();

            entity.HasOne(d => d.AllocationRun)
                .WithMany(p => p.Reviews)
                .HasForeignKey(d => d.MaRun)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Review_Run");

            entity.HasOne(d => d.NguoiDuyetNavigation)
                .WithMany(p => p.Reviews)
                .HasForeignKey(d => d.NguoiDuyet)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Review_NguoiDuyet");
        });

        modelBuilder.Entity<Lock>(entity =>
        {
            entity.ToTable("Lock", "dbo", tb => tb.HasTrigger("TRG_Lock_ValidateAndSyncRun"));
            entity.HasKey(e => e.MaLock).HasName("PK_Lock");
            entity.Property(e => e.MaLock).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.MaRun).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.NguoiKhoa).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.ThoiGianKhoa).HasPrecision(0);
            entity.Property(e => e.LyDoKhoa).HasMaxLength(500);

            entity.HasIndex(e => e.MaRun, "UQ_Lock_MaRun").IsUnique();

            entity.HasOne(d => d.AllocationRun)
                .WithOne(p => p.Lock)
                .HasForeignKey<Lock>(d => d.MaRun)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Lock_Run");

            entity.HasOne(d => d.NguoiKhoaNavigation)
                .WithMany(p => p.Locks)
                .HasForeignKey(d => d.NguoiKhoa)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Lock_NguoiKhoa");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLog", "dbo");
            entity.HasKey(e => e.MaAuditLog).HasName("PK_AuditLog");
            entity.Property(e => e.MaAuditLog).ValueGeneratedOnAdd();
            entity.Property(e => e.MaNguoiDung).HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.ThoiGian).HasPrecision(0);
            entity.Property(e => e.HanhDong).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.DoiTuong).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.MaDoiTuong).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.NoiDung).HasMaxLength(1000);
            entity.Property(e => e.DiaChiIP).HasMaxLength(50).IsUnicode(false);

            entity.HasOne(d => d.NguoiDung)
                .WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AuditLog_NguoiDung");
        });
    }
}
