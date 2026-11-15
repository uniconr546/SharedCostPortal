export interface RoleDto {
  maRole: string;
  tenRole: string;
  moTa?: string;
  trangThai: string;
}

export interface NguoiDungDto {
  maNguoiDung: string;
  hoTen: string;
  tenDangNhap: string;
  email?: string;
  oidcIssuer: string;
  oidcSubject: string;
  maRole: string;
  tenRole: string;
  trangThai: string;
}

export interface AuthSessionDto {
  accessToken: string;
  user: NguoiDungDto;
}

export interface CostPoolDto {
  maCostPool: string;
  tenCostPool: string;
  kyApDung: string;
  soTien: number;
  nguonDuLieu?: string;
  moTa?: string;
  trangThai: string;
}

export interface DriverDto {
  maDriver: string;
  tenDriver: string;
  donViTinh: string;
  moTa?: string;
  trangThai: string;
}

export interface TargetDto {
  maTarget: string;
  tenTarget: string;
  loaiTarget: string;
  moTa?: string;
  trangThai: string;
}

export interface DriverValueDto {
  maDriverValue: string;
  maDriver: string;
  tenDriver: string;
  maTarget: string;
  tenTarget: string;
  kyPhanBo: string;
  giaTriDriver: number;
  donViTinh?: string;
  nguonDuLieu?: string;
}

export interface RuleTargetItemDto {
  maTarget: string;
  tenTarget?: string;
  thuTuTarget: number;
  tyLeMacDinh?: number | null;
  thamSoJson?: string | null;
}

export interface AllocationRuleDto {
  maRule: string;
  tenRule: string;
  maCostPool: string;
  tenCostPool: string;
  maDriver: string;
  tenDriver: string;
  congThuc: string;
  thuTuThucHien: number;
  trangThai: string;
  moTa?: string;
  targets: RuleTargetItemDto[];
}

export interface RuleVersionDto {
  maRuleVersion: string;
  maRule: string;
  tenRule: string;
  maCostPool: string;
  tenCostPool: string;
  maDriver: string;
  tenDriver: string;
  soPhienBan: number;
  ngayHieuLuc: string;
  ngayHetHieuLuc?: string | null;
  congThuc: string;
  thuTuThucHien: number;
  thamSoJson?: string | null;
  trangThai: string;
  nguoiTao: string;
  targets: RuleTargetItemDto[];
}

export interface SimulationResultItemDto {
  maSimulationResult: string;
  maTarget: string;
  tenTarget: string;
  maDriverValue: string;
  giaTriDriver: number;
  tongDriver: number;
  tyLePhanBo: number;
  soTienNguon: number;
  soTienPhanBo: number;
}

export interface SimulationDto {
  maSimulation: string;
  maRuleVersion: string;
  maRule: string;
  maCostPool: string;
  tenCostPool: string;
  maDriver: string;
  tenDriver: string;
  thoiGianChay: string;
  nguoiThucHien: string;
  kyPhanBo: string;
  tongTienNguon: number;
  trangThai: string;
  ghiChu?: string;
  results: SimulationResultItemDto[];
}

export interface AllocationResultItemDto {
  maResult: string;
  maRun: string;
  maTarget: string;
  tenTarget: string;
  maDriver: string;
  maDriverValue: string;
  kyPhanBo: string;
  giaTriDriver: number;
  tongDriver: number;
  tyLePhanBo: number;
  soTienNguon: number;
  soTienPhanBo: number;
  trangThai: string;
}

export interface ReviewDto {
  maReview: string;
  maRun: string;
  lanDuyet: number;
  nguoiDuyet: string;
  tenNguoiDuyet: string;
  thoiGianDuyet: string;
  trangThai: string;
  nhanXet?: string;
}

export interface LockDto {
  maLock: string;
  maRun: string;
  nguoiKhoa: string;
  tenNguoiKhoa: string;
  thoiGianKhoa: string;
  lyDoKhoa?: string;
}

export interface AllocationRunDto {
  maRun: string;
  maCostPool: string;
  tenCostPool: string;
  maRule: string;
  tenRule: string;
  maDriver: string;
  tenDriver: string;
  maRuleVersion: string;
  soPhienBan: number;
  thoiGianChay: string;
  nguoiThucHien: string;
  kyPhanBo: string;
  tongTienNguon: number;
  tongTienPhanBo: number;
  trangThai: string;
  results: AllocationResultItemDto[];
  reviews: ReviewDto[];
  lockInfo?: LockDto | null;
}

export interface TraceabilityDto {
  maResult: string;
  maRun: string;
  trangThaiRun: string;
  kyPhanBo: string;
  thoiGianChay: string;
  nguoiThucHien: string;
  maCostPool: string;
  tenCostPool: string;
  tongTienNguon: number;
  nguonDuLieuChiPhi?: string;
  maRule: string;
  tenRule: string;
  maRuleVersion: string;
  soPhienBan: number;
  ngayHieuLucRule: string;
  ngayHetHieuLucRule?: string | null;
  congThuc: string;
  maDriver: string;
  tenDriver: string;
  donViTinhDriver: string;
  maDriverValue: string;
  giaTriDriverTarget: number;
  tongDriver: number;
  nguonDuLieuDriver?: string;
  maTarget: string;
  tenTarget: string;
  thuTuTarget: number;
  tyLePhanBo: number;
  soTienPhanBo: number;
  lichSuReview: ReviewDto[];
  thongTinKhoa?: LockDto | null;
}

export interface AuditLogDto {
  maAuditLog: number;
  maNguoiDung: string;
  hoTenNguoiDung: string;
  thoiGian: string;
  hanhDong: string;
  doiTuong: string;
  maDoiTuong?: string;
  noiDung?: string;
  giaTriCu?: string;
  giaTriMoi?: string;
  diaChiIP?: string;
}

export interface TargetReportItemDto {
  maTarget: string;
  tenTarget: string;
  loaiTarget: string;
  soLanPhanBo: number;
  tongTienNhanPhanBo: number;
}

export interface CostPoolReportItemDto {
  maCostPool: string;
  tenCostPool: string;
  kyApDung: string;
  soTienNguon: number;
  tongTienDaPhanBo: number;
  trangThaiRunMoiNhat: string;
}

export interface DashboardSummaryDto {
  tongSoCostPool: number;
  tongGiaTriCostPool: number;
  tongSoRuleActive: number;
  tongSoAllocationRun: number;
  soRunChoDuyet: number;
  soRunDaKhoa: number;
  phanBoTheoTarget: TargetReportItemDto[];
  phanBoTheoCostPool: CostPoolReportItemDto[];
}
