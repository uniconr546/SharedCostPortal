namespace SharedCost.Application.DTOs;

public record RoleDto(string MaRole, string TenRole, string? MoTa, string TrangThai);

public record NguoiDungDto(
    string MaNguoiDung,
    string HoTen,
    string TenDangNhap,
    string? Email,
    string OidcIssuer,
    string OidcSubject,
    string MaRole,
    string TenRole,
    string TrangThai);

public record SaveNguoiDungRequest(
    string MaNguoiDung,
    string HoTen,
    string TenDangNhap,
    string? Email,
    string OidcIssuer,
    string OidcSubject,
    string MaRole,
    string TrangThai = "ACTIVE");

public record OidcLoginRequest(string TenDangNhap, string? OidcIssuer = null, string? OidcSubject = null);

public record AuthSessionDto(
    string AccessToken,
    NguoiDungDto User);

public record CostPoolDto(
    string MaCostPool,
    string TenCostPool,
    string KyApDung,
    decimal SoTien,
    string? NguonDuLieu,
    string? MoTa,
    string TrangThai);

public record SaveCostPoolRequest(
    string MaCostPool,
    string TenCostPool,
    string KyApDung,
    decimal SoTien,
    string? NguonDuLieu,
    string? MoTa,
    string TrangThai = "ACTIVE");

public record DriverDto(
    string MaDriver,
    string TenDriver,
    string DonViTinh,
    string? MoTa,
    string TrangThai);

public record SaveDriverRequest(
    string MaDriver,
    string TenDriver,
    string DonViTinh,
    string? MoTa,
    string TrangThai = "ACTIVE");

public record TargetDto(
    string MaTarget,
    string TenTarget,
    string LoaiTarget,
    string? MoTa,
    string TrangThai);

public record SaveTargetRequest(
    string MaTarget,
    string TenTarget,
    string LoaiTarget,
    string? MoTa,
    string TrangThai = "ACTIVE");

public record DriverValueDto(
    string MaDriverValue,
    string MaDriver,
    string TenDriver,
    string MaTarget,
    string TenTarget,
    string KyPhanBo,
    decimal GiaTriDriver,
    string? DonViTinh,
    string? NguonDuLieu);

public record SaveDriverValueRequest(
    string MaDriverValue,
    string MaDriver,
    string MaTarget,
    string KyPhanBo,
    decimal GiaTriDriver,
    string? DonViTinh,
    string? NguonDuLieu);

public record RuleTargetItemDto(
    string MaTarget,
    string? TenTarget,
    int ThuTuTarget,
    decimal? TyLeMacDinh,
    string? ThamSoJson);

public record AllocationRuleDto(
    string MaRule,
    string TenRule,
    string MaCostPool,
    string TenCostPool,
    string MaDriver,
    string TenDriver,
    string CongThuc,
    int ThuTuThucHien,
    string TrangThai,
    string? MoTa,
    List<RuleTargetItemDto> Targets);

public record SaveAllocationRuleRequest(
    string MaRule,
    string TenRule,
    string MaCostPool,
    string MaDriver,
    string CongThuc,
    int ThuTuThucHien,
    string TrangThai,
    string? MoTa,
    DateTime? NgayHieuLucVersionDau,
    List<RuleTargetItemDto> Targets);

public record RuleVersionDto(
    string MaRuleVersion,
    string MaRule,
    string TenRule,
    string MaCostPool,
    string TenCostPool,
    string MaDriver,
    string TenDriver,
    int SoPhienBan,
    DateTime NgayHieuLuc,
    DateTime? NgayHetHieuLuc,
    string CongThuc,
    int ThuTuThucHien,
    string? ThamSoJson,
    string TrangThai,
    string NguoiTao,
    List<RuleTargetItemDto> Targets);

public record CreateRuleVersionRequest(
    string MaRuleVersion,
    string MaRule,
    string MaCostPool,
    string MaDriver,
    DateTime NgayHieuLuc,
    DateTime? NgayHetHieuLuc,
    string CongThuc,
    int ThuTuThucHien,
    string? ThamSoJson,
    bool KichHoatNgay,
    List<RuleTargetItemDto> Targets);

public record RunSimulationRequest(
    string? MaSimulation,
    string MaRuleVersion,
    string KyPhanBo,
    string? GhiChu);

public record SimulationResultItemDto(
    string MaSimulationResult,
    string MaTarget,
    string TenTarget,
    string MaDriverValue,
    decimal GiaTriDriver,
    decimal TongDriver,
    decimal TyLePhanBo,
    decimal SoTienNguon,
    decimal SoTienPhanBo);

public record SimulationDto(
    string MaSimulation,
    string MaRuleVersion,
    string MaRule,
    string MaCostPool,
    string TenCostPool,
    string MaDriver,
    string TenDriver,
    DateTime ThoiGianChay,
    string NguoiThucHien,
    string KyPhanBo,
    decimal TongTienNguon,
    string TrangThai,
    string? GhiChu,
    List<SimulationResultItemDto> Results);

public record ExecuteAllocationRunRequest(
    string? MaRun,
    string MaRuleVersion,
    string KyPhanBo);

public record AllocationResultItemDto(
    string MaResult,
    string MaRun,
    string MaTarget,
    string TenTarget,
    string MaDriver,
    string MaDriverValue,
    string KyPhanBo,
    decimal GiaTriDriver,
    decimal TongDriver,
    decimal TyLePhanBo,
    decimal SoTienNguon,
    decimal SoTienPhanBo,
    string TrangThai);

public record ReviewDto(
    string MaReview,
    string MaRun,
    int LanDuyet,
    string NguoiDuyet,
    string TenNguoiDuyet,
    DateTime ThoiGianDuyet,
    string TrangThai,
    string? NhanXet);

public record LockDto(
    string MaLock,
    string MaRun,
    string NguoiKhoa,
    string TenNguoiKhoa,
    DateTime ThoiGianKhoa,
    string? LyDoKhoa);

public record AllocationRunDto(
    string MaRun,
    string MaCostPool,
    string TenCostPool,
    string MaRule,
    string TenRule,
    string MaDriver,
    string TenDriver,
    string MaRuleVersion,
    int SoPhienBan,
    DateTime ThoiGianChay,
    string NguoiThucHien,
    string KyPhanBo,
    decimal TongTienNguon,
    decimal TongTienPhanBo,
    string TrangThai,
    List<AllocationResultItemDto> Results,
    List<ReviewDto> Reviews,
    LockDto? LockInfo);

public record SubmitReviewDecisionRequest(
    string MaRun,
    string TrangThai,
    string? NhanXet);

public record LockAllocationRunRequest(
    string MaRun,
    string? LyDoKhoa);

public record TraceabilityDto(
    string MaResult,
    string MaRun,
    string TrangThaiRun,
    string KyPhanBo,
    DateTime ThoiGianChay,
    string NguoiThucHien,
    string MaCostPool,
    string TenCostPool,
    decimal TongTienNguon,
    string? NguonDuLieuChiPhi,
    string MaRule,
    string TenRule,
    string MaRuleVersion,
    int SoPhienBan,
    DateTime NgayHieuLucRule,
    DateTime? NgayHetHieuLucRule,
    string CongThuc,
    string MaDriver,
    string TenDriver,
    string DonViTinhDriver,
    string MaDriverValue,
    decimal GiaTriDriverTarget,
    decimal TongDriver,
    string? NguonDuLieuDriver,
    string MaTarget,
    string TenTarget,
    int ThuTuTarget,
    decimal TyLePhanBo,
    decimal SoTienPhanBo,
    List<ReviewDto> LichSuReview,
    LockDto? ThongTinKhoa);

public record AuditLogDto(
    long MaAuditLog,
    string MaNguoiDung,
    string HoTenNguoiDung,
    DateTime ThoiGian,
    string HanhDong,
    string DoiTuong,
    string? MaDoiTuong,
    string? NoiDung,
    string? GiaTriCu,
    string? GiaTriMoi,
    string? DiaChiIP);

public record TargetReportItemDto(
    string MaTarget,
    string TenTarget,
    string LoaiTarget,
    int SoLanPhanBo,
    decimal TongTienNhanPhanBo);

public record CostPoolReportItemDto(
    string MaCostPool,
    string TenCostPool,
    string KyApDung,
    decimal SoTienNguon,
    decimal TongTienDaPhanBo,
    string TrangThaiRunMoiNhat);

public record DashboardSummaryDto(
    int TongSoCostPool,
    decimal TongGiaTriCostPool,
    int TongSoRuleActive,
    int TongSoAllocationRun,
    int SoRunChoDuyet,
    int SoRunDaKhoa,
    List<TargetReportItemDto> PhanBoTheoTarget,
    List<CostPoolReportItemDto> PhanBoTheoCostPool);
