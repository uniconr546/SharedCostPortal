IF DB_ID(N'PhanBoChiPhi') IS NULL
    EXEC(N'CREATE DATABASE PhanBoChiPhi');
GO
USE PhanBoChiPhi;
GO
IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NOT NULL DROP TABLE dbo.AuditLog;
IF OBJECT_ID(N'dbo.[Lock]', N'U') IS NOT NULL DROP TABLE dbo.[Lock];
IF OBJECT_ID(N'dbo.Review', N'U') IS NOT NULL DROP TABLE dbo.Review;
IF OBJECT_ID(N'dbo.AllocationResult', N'U') IS NOT NULL DROP TABLE dbo.AllocationResult;
IF OBJECT_ID(N'dbo.AllocationRun', N'U') IS NOT NULL DROP TABLE dbo.AllocationRun;
IF OBJECT_ID(N'dbo.SimulationResult', N'U') IS NOT NULL DROP TABLE dbo.SimulationResult;
IF OBJECT_ID(N'dbo.Simulation', N'U') IS NOT NULL DROP TABLE dbo.Simulation;
IF OBJECT_ID(N'dbo.RuleVersionTarget', N'U') IS NOT NULL DROP TABLE dbo.RuleVersionTarget;
IF OBJECT_ID(N'dbo.RuleVersion', N'U') IS NOT NULL DROP TABLE dbo.RuleVersion;
IF OBJECT_ID(N'dbo.RuleTarget', N'U') IS NOT NULL DROP TABLE dbo.RuleTarget;
IF OBJECT_ID(N'dbo.AllocationRule', N'U') IS NOT NULL DROP TABLE dbo.AllocationRule;
IF OBJECT_ID(N'dbo.DriverValue', N'U') IS NOT NULL DROP TABLE dbo.DriverValue;
IF OBJECT_ID(N'dbo.Target', N'U') IS NOT NULL DROP TABLE dbo.Target;
IF OBJECT_ID(N'dbo.Driver', N'U') IS NOT NULL DROP TABLE dbo.Driver;
IF OBJECT_ID(N'dbo.CostPool', N'U') IS NOT NULL DROP TABLE dbo.CostPool;
IF OBJECT_ID(N'dbo.NguoiDung', N'U') IS NOT NULL DROP TABLE dbo.NguoiDung;
IF OBJECT_ID(N'dbo.Role', N'U') IS NOT NULL DROP TABLE dbo.Role;
GO
CREATE TABLE dbo.Role
(
    MaRole VARCHAR(30) NOT NULL,
    TenRole NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(500) NULL,
    TrangThai VARCHAR(20) NOT NULL CONSTRAINT DF_Role_TrangThai DEFAULT ('ACTIVE'),
    CONSTRAINT PK_Role PRIMARY KEY (MaRole),
    CONSTRAINT UQ_Role_TenRole UNIQUE (TenRole),
    CONSTRAINT CK_Role_TrangThai CHECK (TrangThai IN ('ACTIVE','INACTIVE'))
);
GO
CREATE TABLE dbo.NguoiDung
(
    MaNguoiDung VARCHAR(30) NOT NULL,
    HoTen NVARCHAR(150) NOT NULL,
    TenDangNhap VARCHAR(100) NOT NULL,
    Email NVARCHAR(254) NULL,
    OidcIssuer NVARCHAR(300) NOT NULL,
    OidcSubject NVARCHAR(200) NOT NULL,
    MaRole VARCHAR(30) NOT NULL,
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_NguoiDung_TrangThai DEFAULT ('ACTIVE'),
    CONSTRAINT PK_NguoiDung PRIMARY KEY (MaNguoiDung),
    CONSTRAINT UQ_NguoiDung_TenDangNhap UNIQUE (TenDangNhap),
    CONSTRAINT UQ_NguoiDung_OIDC UNIQUE (OidcIssuer, OidcSubject),
    CONSTRAINT FK_NguoiDung_Role FOREIGN KEY (MaRole) REFERENCES dbo.Role(MaRole),
    CONSTRAINT CK_NguoiDung_TrangThai CHECK (TrangThai IN ('ACTIVE','INACTIVE','LOCKED'))
);
GO

CREATE TABLE dbo.CostPool
(
    MaCostPool VARCHAR(30) NOT NULL,
    TenCostPool NVARCHAR(150) NOT NULL,
    KyApDung VARCHAR(20) NOT NULL,
    SoTien DECIMAL(18,2) NOT NULL,
    NguonDuLieu NVARCHAR(200) NULL,
    MoTa NVARCHAR(500) NULL,
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_CostPool_TrangThai DEFAULT ('ACTIVE'),
    CONSTRAINT PK_CostPool PRIMARY KEY (MaCostPool),
    CONSTRAINT UQ_CostPool_Ten_Ky UNIQUE (TenCostPool, KyApDung),
    CONSTRAINT UQ_CostPool_Ma_Ky UNIQUE (MaCostPool, KyApDung),
    CONSTRAINT CK_CostPool_SoTien CHECK (SoTien >= 0),
    CONSTRAINT CK_CostPool_TrangThai CHECK (TrangThai IN ('DRAFT','ACTIVE','INACTIVE')),
    CONSTRAINT CK_CostPool_KyApDung_Format CHECK (
        LEN(KyApDung) = 7
        AND KyApDung LIKE '[1-2][0-9][0-9][0-9]-[0-1][0-9]'
        AND TRY_CONVERT(DATE, KyApDung + '-01', 23) IS NOT NULL
    )
);
GO
CREATE TABLE dbo.Driver
(
    MaDriver VARCHAR(30) NOT NULL,
    TenDriver NVARCHAR(150) NOT NULL,
    DonViTinh NVARCHAR(50) NOT NULL,
    MoTa NVARCHAR(500) NULL,
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_Driver_TrangThai DEFAULT ('ACTIVE'),
    CONSTRAINT PK_Driver PRIMARY KEY (MaDriver),
    CONSTRAINT UQ_Driver_TenDriver UNIQUE (TenDriver),
    CONSTRAINT CK_Driver_TrangThai CHECK (TrangThai IN ('DRAFT','ACTIVE','INACTIVE'))
);
GO
CREATE TABLE dbo.Target
(
    MaTarget VARCHAR(30) NOT NULL,
    TenTarget NVARCHAR(150) NOT NULL,
    LoaiTarget NVARCHAR(80) NOT NULL,
    MoTa NVARCHAR(500) NULL,
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_Target_TrangThai DEFAULT ('ACTIVE'),
    CONSTRAINT PK_Target PRIMARY KEY (MaTarget),
    CONSTRAINT UQ_Target_TenTarget UNIQUE (TenTarget),
    CONSTRAINT CK_Target_TrangThai CHECK (TrangThai IN ('DRAFT','ACTIVE','INACTIVE'))
);
GO
CREATE TABLE dbo.DriverValue
(
    MaDriverValue VARCHAR(30) NOT NULL,
    MaDriver VARCHAR(30) NOT NULL,
    MaTarget VARCHAR(30) NOT NULL,
    KyPhanBo VARCHAR(20) NOT NULL,
    GiaTriDriver DECIMAL(18,4) NOT NULL,
    DonViTinh NVARCHAR(50) NULL,
    NguonDuLieu NVARCHAR(200) NULL,
    CONSTRAINT PK_DriverValue PRIMARY KEY (MaDriverValue),
    CONSTRAINT UQ_DriverValue_Driver_Target_Ky UNIQUE (MaDriver, MaTarget, KyPhanBo),
    CONSTRAINT UQ_DriverValue_Trace UNIQUE (MaDriverValue, MaDriver, MaTarget, KyPhanBo),
    CONSTRAINT FK_DriverValue_Driver FOREIGN KEY (MaDriver) REFERENCES dbo.Driver(MaDriver),
    CONSTRAINT FK_DriverValue_Target FOREIGN KEY (MaTarget) REFERENCES dbo.Target(MaTarget),
    CONSTRAINT CK_DriverValue_GiaTri CHECK (GiaTriDriver >= 0),
    CONSTRAINT CK_DriverValue_KyPhanBo_Format CHECK (
        LEN(KyPhanBo) = 7
        AND KyPhanBo LIKE '[1-2][0-9][0-9][0-9]-[0-1][0-9]'
        AND TRY_CONVERT(DATE, KyPhanBo + '-01', 23) IS NOT NULL
    )
);
GO
CREATE TABLE dbo.AllocationRule
(
    MaRule VARCHAR(30) NOT NULL,
    TenRule NVARCHAR(150) NOT NULL,
    MaCostPool VARCHAR(30) NOT NULL,
    MaDriver VARCHAR(30) NOT NULL,
    CongThuc NVARCHAR(1000) NOT NULL,
    ThuTuThucHien INT NOT NULL CONSTRAINT DF_AllocationRule_ThuTu DEFAULT (1),
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_AllocationRule_TrangThai DEFAULT ('DRAFT'),
    MoTa NVARCHAR(500) NULL,
    CONSTRAINT PK_AllocationRule PRIMARY KEY (MaRule),
    CONSTRAINT UQ_AllocationRule_TenRule UNIQUE (TenRule),
    CONSTRAINT FK_AllocationRule_CostPool FOREIGN KEY (MaCostPool) REFERENCES dbo.CostPool(MaCostPool),
    CONSTRAINT FK_AllocationRule_Driver FOREIGN KEY (MaDriver) REFERENCES dbo.Driver(MaDriver),
    CONSTRAINT CK_AllocationRule_ThuTu CHECK (ThuTuThucHien > 0),
    CONSTRAINT CK_AllocationRule_TrangThai CHECK (TrangThai IN ('DRAFT','ACTIVE','INACTIVE'))
);
GO

CREATE TABLE dbo.RuleTarget
(
    MaRule VARCHAR(30) NOT NULL,
    MaTarget VARCHAR(30) NOT NULL,
    ThuTuTarget INT NOT NULL CONSTRAINT DF_RuleTarget_ThuTu DEFAULT (1),
    TyLeMacDinh DECIMAL(18,6) NULL,
    ThamSoJson NVARCHAR(MAX) NULL,
    CONSTRAINT PK_RuleTarget PRIMARY KEY (MaRule, MaTarget),
    CONSTRAINT FK_RuleTarget_Rule FOREIGN KEY (MaRule) REFERENCES dbo.AllocationRule(MaRule),
    CONSTRAINT FK_RuleTarget_Target FOREIGN KEY (MaTarget) REFERENCES dbo.Target(MaTarget),
    CONSTRAINT CK_RuleTarget_ThuTu CHECK (ThuTuTarget > 0),
    CONSTRAINT CK_RuleTarget_TyLe CHECK (TyLeMacDinh IS NULL OR (TyLeMacDinh >= 0 AND TyLeMacDinh <= 1)),
    CONSTRAINT CK_RuleTarget_Json CHECK (ThamSoJson IS NULL OR ISJSON(ThamSoJson) = 1)
);
GO
CREATE TABLE dbo.RuleVersion
(
    MaRuleVersion VARCHAR(30) NOT NULL,
    MaRule VARCHAR(30) NOT NULL,
    MaCostPool VARCHAR(30) NOT NULL,
    MaDriver VARCHAR(30) NOT NULL,
    SoPhienBan INT NOT NULL,
    NgayHieuLuc DATE NOT NULL,
    NgayHetHieuLuc DATE NULL,
    CongThuc NVARCHAR(1000) NOT NULL,
    ThuTuThucHien INT NOT NULL,
    ThamSoJson NVARCHAR(MAX) NULL,
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_RuleVersion_TrangThai DEFAULT ('DRAFT'),
    NguoiTao VARCHAR(30) NOT NULL,
    CONSTRAINT PK_RuleVersion PRIMARY KEY (MaRuleVersion),
    CONSTRAINT UQ_RuleVersion_Rule_PhienBan UNIQUE (MaRule, SoPhienBan),
    CONSTRAINT UQ_RuleVersion_MaRule UNIQUE (MaRuleVersion, MaRule),
    CONSTRAINT UQ_RuleVersion_Full UNIQUE (MaRuleVersion, MaRule, MaCostPool, MaDriver),
    CONSTRAINT FK_RuleVersion_Rule FOREIGN KEY (MaRule) REFERENCES dbo.AllocationRule(MaRule),
    CONSTRAINT FK_RuleVersion_CostPool FOREIGN KEY (MaCostPool) REFERENCES dbo.CostPool(MaCostPool),
    CONSTRAINT FK_RuleVersion_Driver FOREIGN KEY (MaDriver) REFERENCES dbo.Driver(MaDriver),
    CONSTRAINT FK_RuleVersion_NguoiTao FOREIGN KEY (NguoiTao) REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT CK_RuleVersion_SoPhienBan CHECK (SoPhienBan > 0),
    CONSTRAINT CK_RuleVersion_ThuTu CHECK (ThuTuThucHien > 0),
    CONSTRAINT CK_RuleVersion_Ngay CHECK (NgayHetHieuLuc IS NULL OR NgayHetHieuLuc >= NgayHieuLuc),
    CONSTRAINT CK_RuleVersion_TrangThai CHECK (TrangThai IN ('DRAFT','ACTIVE','EXPIRED','INACTIVE')),
    CONSTRAINT CK_RuleVersion_Json CHECK (ThamSoJson IS NULL OR ISJSON(ThamSoJson) = 1)
);
GO
CREATE TABLE dbo.RuleVersionTarget
(
    MaRuleVersion VARCHAR(30) NOT NULL,
    MaRule VARCHAR(30) NOT NULL,
    MaTarget VARCHAR(30) NOT NULL,
    ThuTuTarget INT NOT NULL,
    TyLeMacDinh DECIMAL(18,6) NULL,
    ThamSoJson NVARCHAR(MAX) NULL,
    CONSTRAINT PK_RuleVersionTarget PRIMARY KEY (MaRuleVersion, MaTarget),
    CONSTRAINT FK_RuleVersionTarget_Version FOREIGN KEY (MaRuleVersion, MaRule)
        REFERENCES dbo.RuleVersion(MaRuleVersion, MaRule),
    CONSTRAINT FK_RuleVersionTarget_Target FOREIGN KEY (MaTarget)
        REFERENCES dbo.Target(MaTarget),
    CONSTRAINT CK_RuleVersionTarget_ThuTu CHECK (ThuTuTarget > 0),
    CONSTRAINT CK_RuleVersionTarget_TyLe CHECK (TyLeMacDinh IS NULL OR (TyLeMacDinh >= 0 AND TyLeMacDinh <= 1)),
    CONSTRAINT CK_RuleVersionTarget_Json CHECK (ThamSoJson IS NULL OR ISJSON(ThamSoJson) = 1)
);
GO
CREATE TABLE dbo.Simulation
(
    MaSimulation VARCHAR(30) NOT NULL,
    MaRuleVersion VARCHAR(30) NOT NULL,
    MaRule VARCHAR(30) NOT NULL,
    MaCostPool VARCHAR(30) NOT NULL,
    MaDriver VARCHAR(30) NOT NULL,
    ThoiGianChay DATETIME2(0) NOT NULL CONSTRAINT DF_Simulation_ThoiGian DEFAULT (SYSDATETIME()),
    NguoiThucHien VARCHAR(30) NOT NULL,
    KyPhanBo VARCHAR(20) NOT NULL,
    TongTienNguon DECIMAL(18,2) NOT NULL,
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_Simulation_TrangThai DEFAULT ('RUNNING'),
    GhiChu NVARCHAR(500) NULL,
    CONSTRAINT PK_Simulation PRIMARY KEY (MaSimulation),
    CONSTRAINT UQ_Simulation_Context UNIQUE (MaSimulation, MaRuleVersion, MaDriver, KyPhanBo),
    CONSTRAINT FK_Simulation_RuleVersion FOREIGN KEY (MaRuleVersion, MaRule, MaCostPool, MaDriver)
        REFERENCES dbo.RuleVersion(MaRuleVersion, MaRule, MaCostPool, MaDriver),
    CONSTRAINT FK_Simulation_CostPool_Ky FOREIGN KEY (MaCostPool, KyPhanBo)
        REFERENCES dbo.CostPool(MaCostPool, KyApDung),
    CONSTRAINT FK_Simulation_NguoiDung FOREIGN KEY (NguoiThucHien) REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT CK_Simulation_TienNguon CHECK (TongTienNguon >= 0),
    CONSTRAINT CK_Simulation_TrangThai CHECK (TrangThai IN ('RUNNING','COMPLETED','FAILED')),
    CONSTRAINT CK_Simulation_KyPhanBo_Format CHECK (
        LEN(KyPhanBo) = 7
        AND KyPhanBo LIKE '[1-2][0-9][0-9][0-9]-[0-1][0-9]'
        AND TRY_CONVERT(DATE, KyPhanBo + '-01', 23) IS NOT NULL
    )
);
GO
CREATE TABLE dbo.SimulationResult
(
    MaSimulationResult VARCHAR(30) NOT NULL,
    MaSimulation VARCHAR(30) NOT NULL,
    MaRuleVersion VARCHAR(30) NOT NULL,
    MaDriver VARCHAR(30) NOT NULL,
    MaTarget VARCHAR(30) NOT NULL,
    KyPhanBo VARCHAR(20) NOT NULL,
    MaDriverValue VARCHAR(30) NOT NULL,
    GiaTriDriver DECIMAL(18,4) NOT NULL,
    TongDriver DECIMAL(18,4) NOT NULL,
    TyLePhanBo DECIMAL(18,6) NOT NULL,
    SoTienNguon DECIMAL(18,2) NOT NULL,
    SoTienPhanBo DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_SimulationResult PRIMARY KEY (MaSimulationResult),
    CONSTRAINT UQ_SimulationResult_Target UNIQUE (MaSimulation, MaTarget, MaDriver),
    CONSTRAINT FK_SimulationResult_Simulation FOREIGN KEY (MaSimulation, MaRuleVersion, MaDriver, KyPhanBo)
        REFERENCES dbo.Simulation(MaSimulation, MaRuleVersion, MaDriver, KyPhanBo),
    CONSTRAINT FK_SimulationResult_VersionTarget FOREIGN KEY (MaRuleVersion, MaTarget)
        REFERENCES dbo.RuleVersionTarget(MaRuleVersion, MaTarget),
    CONSTRAINT FK_SimulationResult_DriverValue FOREIGN KEY (MaDriverValue, MaDriver, MaTarget, KyPhanBo)
        REFERENCES dbo.DriverValue(MaDriverValue, MaDriver, MaTarget, KyPhanBo),
    CONSTRAINT CK_SimulationResult_GiaTri CHECK (GiaTriDriver >= 0),
    CONSTRAINT CK_SimulationResult_Tong CHECK (TongDriver > 0),
    CONSTRAINT CK_SimulationResult_TyLe CHECK (TyLePhanBo >= 0 AND TyLePhanBo <= 1),
    CONSTRAINT CK_SimulationResult_Tien CHECK (SoTienNguon >= 0 AND SoTienPhanBo >= 0)
);
GO
CREATE TABLE dbo.AllocationRun
(
    MaRun VARCHAR(30) NOT NULL,
    MaCostPool VARCHAR(30) NOT NULL,
    MaRule VARCHAR(30) NOT NULL,
    MaDriver VARCHAR(30) NOT NULL,
    MaRuleVersion VARCHAR(30) NOT NULL,
    ThoiGianChay DATETIME2(0) NOT NULL CONSTRAINT DF_AllocationRun_ThoiGian DEFAULT (SYSDATETIME()),
    NguoiThucHien VARCHAR(30) NOT NULL,
    KyPhanBo VARCHAR(20) NOT NULL,
    TongTienNguon DECIMAL(18,2) NOT NULL,
    TongTienPhanBo DECIMAL(18,2) NOT NULL CONSTRAINT DF_AllocationRun_TongTien DEFAULT (0),
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_AllocationRun_TrangThai DEFAULT ('DRAFT'),
    CONSTRAINT PK_AllocationRun PRIMARY KEY (MaRun),
    CONSTRAINT UQ_AllocationRun_Full UNIQUE (MaRun, MaRule, MaRuleVersion, MaDriver, KyPhanBo),
    CONSTRAINT FK_AllocationRun_RuleVersion FOREIGN KEY (MaRuleVersion, MaRule, MaCostPool, MaDriver)
        REFERENCES dbo.RuleVersion(MaRuleVersion, MaRule, MaCostPool, MaDriver),
    CONSTRAINT FK_AllocationRun_CostPool_Ky FOREIGN KEY (MaCostPool, KyPhanBo)
        REFERENCES dbo.CostPool(MaCostPool, KyApDung),
    CONSTRAINT FK_AllocationRun_NguoiDung FOREIGN KEY (NguoiThucHien) REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT CK_AllocationRun_TienNguon CHECK (TongTienNguon >= 0),
    CONSTRAINT CK_AllocationRun_TienPhanBo CHECK (TongTienPhanBo >= 0),
    CONSTRAINT CK_AllocationRun_TrangThai CHECK
        (TrangThai IN ('DRAFT','CALCULATED','PENDING_REVIEW','REVIEWED','REJECTED','LOCKED')),
    CONSTRAINT CK_AllocationRun_KyPhanBo_Format CHECK (
        LEN(KyPhanBo) = 7
        AND KyPhanBo LIKE '[1-2][0-9][0-9][0-9]-[0-1][0-9]'
        AND TRY_CONVERT(DATE, KyPhanBo + '-01', 23) IS NOT NULL
    )
);
GO
CREATE TABLE dbo.AllocationResult
(
    MaResult VARCHAR(30) NOT NULL,
    MaRun VARCHAR(30) NOT NULL,
    MaRule VARCHAR(30) NOT NULL,
    MaRuleVersion VARCHAR(30) NOT NULL,
    MaTarget VARCHAR(30) NOT NULL,
    MaDriver VARCHAR(30) NOT NULL,
    KyPhanBo VARCHAR(20) NOT NULL,
    MaDriverValue VARCHAR(30) NOT NULL,
    GiaTriDriver DECIMAL(18,4) NOT NULL,
    TongDriver DECIMAL(18,4) NOT NULL,
    TyLePhanBo DECIMAL(18,6) NOT NULL,
    SoTienNguon DECIMAL(18,2) NOT NULL,
    SoTienPhanBo DECIMAL(18,2) NOT NULL,
    TrangThai VARCHAR(30) NOT NULL CONSTRAINT DF_AllocationResult_TrangThai DEFAULT ('CALCULATED'),
    CONSTRAINT PK_AllocationResult PRIMARY KEY (MaResult),
    CONSTRAINT UQ_AllocationResult_Run_Target_Driver UNIQUE (MaRun, MaTarget, MaDriver),
    CONSTRAINT FK_AllocationResult_Run FOREIGN KEY (MaRun, MaRule, MaRuleVersion, MaDriver, KyPhanBo)
        REFERENCES dbo.AllocationRun(MaRun, MaRule, MaRuleVersion, MaDriver, KyPhanBo),
    CONSTRAINT FK_AllocationResult_VersionTarget FOREIGN KEY (MaRuleVersion, MaTarget)
        REFERENCES dbo.RuleVersionTarget(MaRuleVersion, MaTarget),
    CONSTRAINT FK_AllocationResult_DriverValue FOREIGN KEY (MaDriverValue, MaDriver, MaTarget, KyPhanBo)
        REFERENCES dbo.DriverValue(MaDriverValue, MaDriver, MaTarget, KyPhanBo),
    CONSTRAINT CK_AllocationResult_GiaTri CHECK (GiaTriDriver >= 0),
    CONSTRAINT CK_AllocationResult_Tong CHECK (TongDriver > 0),
    CONSTRAINT CK_AllocationResult_TyLe CHECK (TyLePhanBo >= 0 AND TyLePhanBo <= 1),
    CONSTRAINT CK_AllocationResult_Tien CHECK (SoTienNguon >= 0 AND SoTienPhanBo >= 0),
    CONSTRAINT CK_AllocationResult_TrangThai CHECK (TrangThai = 'CALCULATED')
);
GO
CREATE TABLE dbo.Review
(
    MaReview VARCHAR(30) NOT NULL,
    MaRun VARCHAR(30) NOT NULL,
    LanDuyet INT NOT NULL,
    NguoiDuyet VARCHAR(30) NOT NULL,
    ThoiGianDuyet DATETIME2(0) NOT NULL CONSTRAINT DF_Review_ThoiGian DEFAULT (SYSDATETIME()),
    TrangThai VARCHAR(30) NOT NULL,
    NhanXet NVARCHAR(1000) NULL,
    CONSTRAINT PK_Review PRIMARY KEY (MaReview),
    CONSTRAINT UQ_Review_Run_LanDuyet UNIQUE (MaRun, LanDuyet),
    CONSTRAINT FK_Review_Run FOREIGN KEY (MaRun) REFERENCES dbo.AllocationRun(MaRun),
    CONSTRAINT FK_Review_NguoiDuyet FOREIGN KEY (NguoiDuyet) REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT CK_Review_LanDuyet CHECK (LanDuyet > 0),
    CONSTRAINT CK_Review_TrangThai CHECK (TrangThai IN ('APPROVED','REJECTED'))
);
GO

CREATE TABLE dbo.[Lock]
(
    MaLock VARCHAR(30) NOT NULL,
    MaRun VARCHAR(30) NOT NULL,
    NguoiKhoa VARCHAR(30) NOT NULL,
    ThoiGianKhoa DATETIME2(0) NOT NULL CONSTRAINT DF_Lock_ThoiGian DEFAULT (SYSDATETIME()),
    LyDoKhoa NVARCHAR(500) NULL,
    CONSTRAINT PK_Lock PRIMARY KEY (MaLock),
    CONSTRAINT UQ_Lock_MaRun UNIQUE (MaRun),
    CONSTRAINT FK_Lock_Run FOREIGN KEY (MaRun) REFERENCES dbo.AllocationRun(MaRun),
    CONSTRAINT FK_Lock_NguoiKhoa FOREIGN KEY (NguoiKhoa) REFERENCES dbo.NguoiDung(MaNguoiDung)
);
GO
CREATE TABLE dbo.AuditLog
(
    MaAuditLog BIGINT IDENTITY(1,1) NOT NULL,
    MaNguoiDung VARCHAR(30) NOT NULL,
    ThoiGian DATETIME2(0) NOT NULL CONSTRAINT DF_AuditLog_ThoiGian DEFAULT (SYSDATETIME()),
    HanhDong VARCHAR(100) NOT NULL,
    DoiTuong VARCHAR(100) NOT NULL,
    MaDoiTuong VARCHAR(30) NULL,
    NoiDung NVARCHAR(1000) NULL,
    GiaTriCu NVARCHAR(MAX) NULL,
    GiaTriMoi NVARCHAR(MAX) NULL,
    DiaChiIP VARCHAR(50) NULL,
    CONSTRAINT PK_AuditLog PRIMARY KEY (MaAuditLog),
    CONSTRAINT FK_AuditLog_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT CK_AuditLog_GiaTriCu_Json CHECK (GiaTriCu IS NULL OR ISJSON(GiaTriCu) = 1),
    CONSTRAINT CK_AuditLog_GiaTriMoi_Json CHECK (GiaTriMoi IS NULL OR ISJSON(GiaTriMoi) = 1)
);
GO
CREATE UNIQUE INDEX UX_RuleVersion_OneActivePerRule
    ON dbo.RuleVersion(MaRule)
    WHERE TrangThai = 'ACTIVE';
CREATE INDEX IX_DriverValue_Driver_Ky ON dbo.DriverValue(MaDriver, KyPhanBo);
CREATE INDEX IX_DriverValue_Target_Ky ON dbo.DriverValue(MaTarget, KyPhanBo);
CREATE INDEX IX_AllocationRule_CostPool ON dbo.AllocationRule(MaCostPool);
CREATE INDEX IX_AllocationRule_Driver ON dbo.AllocationRule(MaDriver);
CREATE INDEX IX_RuleVersion_Rule_Status ON dbo.RuleVersion(MaRule, TrangThai);
CREATE INDEX IX_RuleVersionTarget_Target ON dbo.RuleVersionTarget(MaTarget);
CREATE INDEX IX_Simulation_Version_Time ON dbo.Simulation(MaRuleVersion, ThoiGianChay);
CREATE INDEX IX_SimulationResult_Simulation ON dbo.SimulationResult(MaSimulation);
CREATE INDEX IX_SimulationResult_DriverValue ON dbo.SimulationResult(MaDriverValue);
CREATE INDEX IX_AllocationRun_CostPool_Period ON dbo.AllocationRun(MaCostPool, KyPhanBo);
CREATE INDEX IX_AllocationRun_Version ON dbo.AllocationRun(MaRuleVersion);
CREATE INDEX IX_AllocationResult_Run ON dbo.AllocationResult(MaRun);
CREATE INDEX IX_AllocationResult_Target ON dbo.AllocationResult(MaTarget);
CREATE INDEX IX_AllocationResult_DriverValue ON dbo.AllocationResult(MaDriverValue);
CREATE INDEX IX_Review_Run ON dbo.Review(MaRun);
CREATE INDEX IX_Review_Reviewer ON dbo.Review(NguoiDuyet);
CREATE INDEX IX_Lock_User ON dbo.[Lock](NguoiKhoa);
CREATE INDEX IX_AuditLog_User_Time ON dbo.AuditLog(MaNguoiDung, ThoiGian);
GO
CREATE OR ALTER TRIGGER dbo.TRG_AllocationRun_ValidateAndStateMachine
ON dbo.AllocationRun
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted) AND NOT EXISTS (SELECT 1 FROM inserted)
    BEGIN
        IF EXISTS (SELECT 1 FROM deleted WHERE TrangThai <> 'DRAFT')
        BEGIN
            RAISERROR (N'Lỗi BR11: Không được phép xóa lần chạy phân bổ (AllocationRun) đã tính toán hoặc đã khóa sổ.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM deleted)
    BEGIN
        IF EXISTS (SELECT 1 FROM inserted WHERE TrangThai <> 'DRAFT')
        BEGIN
            RAISERROR (N'Lỗi quy trình: Khi khởi tạo mới, AllocationRun bắt buộc phải ở trạng thái DRAFT trước khi chèn AllocationResult.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
    END
    IF EXISTS (SELECT 1 FROM deleted) AND EXISTS (SELECT 1 FROM inserted)
    BEGIN
        IF EXISTS (SELECT 1 FROM deleted d WHERE d.TrangThai IN ('LOCKED', 'REJECTED'))
        BEGIN
            RAISERROR (N'Lỗi BR11: Lần chạy phân bổ đã ở trạng thái kết thúc (LOCKED hoặc REJECTED), không được phép chỉnh sửa.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        IF EXISTS (
            SELECT 1
            FROM inserted i
            JOIN deleted d ON rtrim(i.MaRun) = rtrim(d.MaRun)
            WHERE i.MaCostPool <> d.MaCostPool
               OR i.MaRule <> d.MaRule
               OR i.MaDriver <> d.MaDriver
               OR i.MaRuleVersion <> d.MaRuleVersion
               OR i.KyPhanBo <> d.KyPhanBo
               OR i.TongTienNguon <> d.TongTienNguon
        )
        BEGIN
            RAISERROR (N'Lỗi toàn vẹn: Không được thay đổi CostPool, RuleVersion, Kỳ phân bổ hoặc Tổng tiền nguồn của lần chạy đã tạo.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF EXISTS (
            SELECT 1
            FROM inserted i
            JOIN deleted d ON i.MaRun = d.MaRun
            WHERE i.TrangThai <> d.TrangThai
              AND NOT (
                  (d.TrangThai = 'DRAFT' AND i.TrangThai = 'CALCULATED')
                  OR (d.TrangThai = 'CALCULATED' AND i.TrangThai IN ('PENDING_REVIEW', 'REVIEWED', 'REJECTED'))
                  OR (d.TrangThai = 'PENDING_REVIEW' AND i.TrangThai IN ('REVIEWED', 'REJECTED'))
                  OR (d.TrangThai = 'REVIEWED' AND i.TrangThai = 'LOCKED')
              )
        )
        BEGIN
            RAISERROR (N'Lỗi quy trình: Bước chuyển trạng thái AllocationRun không hợp lệ. Phải tuân thủ: DRAFT -> CALCULATED -> PENDING_REVIEW -> REVIEWED/REJECTED -> LOCKED.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF EXISTS (
            SELECT 1
            FROM inserted i
            JOIN deleted d ON i.MaRun = d.MaRun
            WHERE i.TrangThai <> d.TrangThai
              AND i.TrangThai IN ('REVIEWED', 'REJECTED')
              AND NOT EXISTS (
                  SELECT 1
                  FROM dbo.Review rv
                  WHERE rv.MaRun = i.MaRun
                    AND rv.TrangThai = CASE WHEN i.TrangThai = 'REVIEWED' THEN 'APPROVED' ELSE 'REJECTED' END
                    AND rv.LanDuyet = (SELECT MAX(r2.LanDuyet) FROM dbo.Review r2 WHERE r2.MaRun = i.MaRun)
              )
        )
        BEGIN
            RAISERROR (N'Lỗi quy trình: Không thể chuyển AllocationRun sang REVIEWED hoặc REJECTED khi chưa có bản ghi kiểm duyệt tương ứng trong bảng Review.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        IF EXISTS (
            SELECT 1
            FROM inserted i
            JOIN deleted d ON i.MaRun = d.MaRun
            WHERE i.TrangThai = 'LOCKED'
              AND NOT EXISTS (SELECT 1 FROM dbo.[Lock] lk WHERE lk.MaRun = i.MaRun)
        )
        BEGIN
            RAISERROR (N'Lỗi BR11: Không thể trực tiếp chuyển AllocationRun sang LOCKED nếu chưa tạo bản ghi khóa sổ trong bảng Lock.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
    END

    IF NOT EXISTS (SELECT 1 FROM deleted)
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM inserted i
            JOIN dbo.RuleVersion rv ON i.MaRuleVersion = rv.MaRuleVersion
            CROSS APPLY (SELECT TRY_CONVERT(DATE, i.KyPhanBo + '-01', 23) AS NgayDauKy) p
            WHERE p.NgayDauKy IS NULL
               OR rv.TrangThai <> 'ACTIVE'
               OR rv.NgayHieuLuc > p.NgayDauKy
               OR (rv.NgayHetHieuLuc IS NOT NULL AND rv.NgayHetHieuLuc < EOMONTH(p.NgayDauKy))
        )
        BEGIN
            RAISERROR (N'Lỗi BR13: Phiên bản quy tắc không ACTIVE hoặc không bao phủ kỳ phân bổ.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
    END
    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE i.TrangThai IN ('CALCULATED', 'PENDING_REVIEW', 'REVIEWED', 'LOCKED')
          AND (
              i.TongTienPhanBo <> i.TongTienNguon
              OR NOT EXISTS (SELECT 1 FROM dbo.AllocationResult ar WHERE ar.MaRun = i.MaRun)
              OR EXISTS (
                  SELECT ar.MaRun
                  FROM dbo.AllocationResult ar
                  WHERE ar.MaRun = i.MaRun
                  GROUP BY ar.MaRun
                  HAVING SUM(ar.TyLePhanBo) <> 1.000000
                      OR SUM(ar.SoTienPhanBo) <> i.TongTienNguon
              )
          )
    )
    BEGIN
        RAISERROR (N'Lỗi BR10: Tổng tỷ lệ phân bổ của các Target phải bằng 1.000000 và tổng số tiền phân bổ phải khớp chính xác với tổng tiền nguồn.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO


CREATE OR ALTER TRIGGER dbo.TRG_AllocationResult_ProtectImmutability
ON dbo.AllocationResult
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        RAISERROR (N'Lỗi toàn vẹn: Không được phép chỉnh sửa trực tiếp dòng kết quả phân bổ (AllocationResult).', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN dbo.AllocationRun r ON r.MaRun = i.MaRun
        WHERE r.TrangThai <> 'DRAFT'
    )
    OR EXISTS (
        SELECT 1
        FROM deleted d
        JOIN dbo.AllocationRun r ON r.MaRun = d.MaRun
        WHERE r.TrangThai <> 'DRAFT'
    )
    BEGIN
        RAISERROR (N'Lỗi BR10/BR11: Chỉ được phép tạo hoặc xóa dòng kết quả khi AllocationRun đang ở trạng thái DRAFT. Không thể thay đổi kết quả sau khi đã chuyển sang CALCULATED hoặc đã khóa sổ.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

CREATE OR ALTER TRIGGER dbo.TRG_Review_ValidateAndSyncRun
ON dbo.Review
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted)
    BEGIN
        RAISERROR (N'Lỗi BR15: Lịch sử kiểm duyệt (Review) là dữ liệu kiểm toán, không được phép chỉnh sửa hoặc xóa.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (SELECT MaRun FROM inserted GROUP BY MaRun HAVING COUNT(*) > 1)
    BEGIN
        RAISERROR (N'Lỗi quy trình: Mỗi thao tác chỉ được phép thêm tối đa một bản ghi Review cho mỗi AllocationRun.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN dbo.AllocationRun r ON r.MaRun = i.MaRun
        WHERE r.TrangThai NOT IN ('CALCULATED', 'PENDING_REVIEW')
    )
    BEGIN
        RAISERROR (N'Lỗi quy trình: Chỉ được phép kiểm duyệt (Review) khi AllocationRun đã tính toán hoàn tất (CALCULATED) hoặc đang chờ duyệt (PENDING_REVIEW).', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TrangThai = 'REJECTED'
          AND (NhanXet IS NULL OR LTRIM(RTRIM(NhanXet)) = N'')
    )
    BEGIN
        RAISERROR (N'Lỗi quy trình: Khi từ chối kết quả phân bổ (REJECTED), bắt buộc phải nhập lý do từ chối vào cột NhanXet.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    UPDATE r
    SET r.TrangThai = CASE WHEN i.TrangThai = 'APPROVED' THEN 'REVIEWED' ELSE 'REJECTED' END
    FROM dbo.AllocationRun r
    JOIN inserted i ON r.MaRun = i.MaRun;
END;
GO

CREATE OR ALTER TRIGGER dbo.TRG_Lock_ValidateAndSyncRun
ON dbo.[Lock]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted)
    BEGIN
        RAISERROR (N'Lỗi BR11: Bản ghi khóa sổ (Lock) không được phép chỉnh sửa hoặc xóa.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN dbo.AllocationRun r ON r.MaRun = i.MaRun
        WHERE r.TrangThai <> 'REVIEWED'
           OR NOT EXISTS (
               SELECT 1
               FROM dbo.Review rv
               WHERE rv.MaRun = i.MaRun
                 AND rv.TrangThai = 'APPROVED'
           )
    )
    BEGIN
        RAISERROR (N'Lỗi BR10/BR11: Chỉ được phép khóa sổ (Lock) khi AllocationRun đã được phê duyệt (REVIEWED).', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    UPDATE r
    SET r.TrangThai = 'LOCKED'
    FROM dbo.AllocationRun r
    JOIN inserted i ON r.MaRun = i.MaRun;
END;
GO


INSERT INTO dbo.Role(MaRole, TenRole, MoTa, TrangThai) VALUES
('ADMIN', N'Quản trị hệ thống', N'Quản lý người dùng và cấu hình hệ thống', 'ACTIVE'),
('FINANCE', N'Nhân viên tài chính', N'Quản lý Cost Pool và dữ liệu phân bổ', 'ACTIVE'),
('REVIEWER', N'Người phê duyệt', N'Kiểm tra và phê duyệt kết quả phân bổ', 'ACTIVE'),
('VIEWER', N'Người xem', N'Chỉ xem dữ liệu và báo cáo được cấp quyền', 'ACTIVE'),
('AUDITOR', N'Kiểm toán viên', N'Tra cứu lịch sử và nhật ký kiểm toán', 'ACTIVE');
GO
PRINT N'Đã khởi tạo CSDL PhanBoChiPhi hoàn chỉnh!';

SET NOCOUNT ON;
BEGIN TRANSACTION;

BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE MaNguoiDung = 'USR_ADMIN')
    INSERT INTO dbo.NguoiDung (MaNguoiDung, HoTen, TenDangNhap, Email, OidcIssuer, OidcSubject, MaRole, TrangThai)
    VALUES
    ('USR_ADMIN',    N'Nguyễn Quản Trị',   'admin',    'admin@sharedcost.local',    'https://oidc.sharedcost.local', 'sub-admin-001',    'ADMIN',    'ACTIVE'),
    ('USR_FINANCE',  N'Trần Tài Chính',    'finance',  'finance@sharedcost.local',  'https://oidc.sharedcost.local', 'sub-finance-001',  'FINANCE',  'ACTIVE'),
    ('USR_REVIEWER', N'Lê Kiểm Duyệt',     'reviewer', 'reviewer@sharedcost.local', 'https://oidc.sharedcost.local', 'sub-reviewer-001', 'REVIEWER', 'ACTIVE'),
    ('USR_AUDITOR',  N'Phạm Kiểm Toán',    'auditor',  'auditor@sharedcost.local',  'https://oidc.sharedcost.local', 'sub-auditor-001',  'AUDITOR',  'ACTIVE'),
    ('USR_VIEWER',   N'Hoàng Quản Lý',     'viewer',   'viewer@sharedcost.local',   'https://oidc.sharedcost.local', 'sub-viewer-001',   'VIEWER',   'ACTIVE');

    IF NOT EXISTS (SELECT 1 FROM dbo.CostPool WHERE MaCostPool = 'CP_DIEN_2610')
    INSERT INTO dbo.CostPool (MaCostPool, TenCostPool, KyApDung, SoTien, NguonDuLieu, MoTa, TrangThai)
    VALUES
    ('CP_DIEN_2610', N'Chi phí điện văn phòng T10/2026',      '2026-10', 100000000.00, N'Hóa đơn điện lực HĐ-EVN-202610', N'Chi phí điện dùng chung toàn tòa nhà kỳ 2026-10', 'ACTIVE'),
    ('CP_IT_2610',   N'Chi phí hạ tầng CNTT T10/2026',        '2026-10', 100000000.00, N'Bảng kê Cloud & License T10/2026', N'Chi phí máy chủ và bản quyền phần mềm dùng chung (kiểm thử làm tròn BR10)', 'ACTIVE'),
    ('CP_DIEN_2611', N'Chi phí điện văn phòng T11/2026',      '2026-11', 120000000.00, N'Hóa đơn điện lực HĐ-EVN-202611', N'Chi phí điện dùng chung toàn tòa nhà kỳ 2026-11', 'ACTIVE');

    IF NOT EXISTS (SELECT 1 FROM dbo.Driver WHERE MaDriver = 'DRV_DIENTICH')
    INSERT INTO dbo.Driver (MaDriver, TenDriver, DonViTinh, MoTa, TrangThai)
    VALUES
    ('DRV_DIENTICH', N'Diện tích sử dụng',  N'm2',    N'Phân bổ theo diện tích mặt sàn văn phòng thực tế', 'ACTIVE'),
    ('DRV_NHANSU',   N'Số lượng nhân sự',   N'Người', N'Phân bổ theo định biên nhân sự thực tế trong kỳ',  'ACTIVE');

    IF NOT EXISTS (SELECT 1 FROM dbo.Target WHERE MaTarget = 'TGT_PBA')
    INSERT INTO dbo.Target (MaTarget, TenTarget, LoaiTarget, MoTa, TrangThai)
    VALUES
    ('TGT_PBA', N'Phòng ban A - Khối Kinh doanh', N'Phòng ban', N'Đơn vị kinh doanh và bán hàng',       'ACTIVE'),
    ('TGT_PBB', N'Phòng ban B - Khối Kỹ thuật',   N'Phòng ban', N'Đơn vị phát triển và vận hành kỹ thuật', 'ACTIVE'),
    ('TGT_PBC', N'Phòng ban C - Khối Hành chính', N'Phòng ban', N'Đơn vị hành chính nhân sự và kế toán', 'ACTIVE');

    IF NOT EXISTS (SELECT 1 FROM dbo.DriverValue WHERE MaDriverValue = 'DV_DT_PBA_2610')
    INSERT INTO dbo.DriverValue (MaDriverValue, MaDriver, MaTarget, KyPhanBo, GiaTriDriver, DonViTinh, NguonDuLieu)
    VALUES
    -- Kỳ 2026-10: Tổng diện tích 
    ('DV_DT_PBA_2610', 'DRV_DIENTICH', 'TGT_PBA', '2026-10', 400.0000, N'm2',    N'Biên bản đo đạc mặt bằng T10/2026'),
    ('DV_DT_PBB_2610', 'DRV_DIENTICH', 'TGT_PBB', '2026-10', 350.0000, N'm2',    N'Biên bản đo đạc mặt bằng T10/2026'),
    ('DV_DT_PBC_2610', 'DRV_DIENTICH', 'TGT_PBC', '2026-10', 250.0000, N'm2',    N'Biên bản đo đạc mặt bằng T10/2026'),
    -- Kỳ 2026-10: Tổng nhân sự 
    ('DV_NS_PBA_2610', 'DRV_NHANSU',   'TGT_PBA', '2026-10',  10.0000, N'Người', N'Bảng chấm công nhân sự T10/2026'),
    ('DV_NS_PBB_2610', 'DRV_NHANSU',   'TGT_PBB', '2026-10',  10.0000, N'Người', N'Bảng chấm công nhân sự T10/2026'),
    ('DV_NS_PBC_2610', 'DRV_NHANSU',   'TGT_PBC', '2026-10',  10.0000, N'Người', N'Bảng chấm công nhân sự T10/2026'),
    -- Kỳ 2026-11: Diện tích
    ('DV_DT_PBA_2611', 'DRV_DIENTICH', 'TGT_PBA', '2026-11', 400.0000, N'm2',    N'Biên bản đo đạc mặt bằng T11/2026'),
    ('DV_DT_PBB_2611', 'DRV_DIENTICH', 'TGT_PBB', '2026-11', 350.0000, N'm2',    N'Biên bản đo đạc mặt bằng T11/2026'),
    ('DV_DT_PBC_2611', 'DRV_DIENTICH', 'TGT_PBC', '2026-11', 250.0000, N'm2',    N'Biên bản đo đạc mặt bằng T11/2026');

    IF NOT EXISTS (SELECT 1 FROM dbo.AllocationRule WHERE MaRule = 'RULE_DIEN')
    BEGIN
        INSERT INTO dbo.AllocationRule (MaRule, TenRule, MaCostPool, MaDriver, CongThuc, ThuTuThucHien, TrangThai, MoTa)
        VALUES
        ('RULE_DIEN', N'Quy tắc phân bổ chi phí điện theo diện tích', 'CP_DIEN_2610', 'DRV_DIENTICH', N'SoTienPhanBo = TongTienNguon * (GiaTriDriver / TongDriver)', 1, 'ACTIVE', N'Phân bổ chi phí điện văn phòng theo tỷ lệ diện tích m2'),
        ('RULE_IT',   N'Quy tắc phân bổ chi phí CNTT theo nhân sự',   'CP_IT_2610',   'DRV_NHANSU',   N'SoTienPhanBo = TongTienNguon * (GiaTriDriver / TongDriver)', 2, 'ACTIVE', N'Phân bổ chi phí CNTT theo số lượng nhân sự');

        INSERT INTO dbo.RuleTarget (MaRule, MaTarget, ThuTuTarget, TyLeMacDinh, ThamSoJson)
        VALUES
        ('RULE_DIEN', 'TGT_PBA', 1, 0.400000, N'{"uuTienLamTron": 1}'),
        ('RULE_DIEN', 'TGT_PBB', 2, 0.350000, N'{"uuTienLamTron": 2}'),
        ('RULE_DIEN', 'TGT_PBC', 3, 0.250000, N'{"uuTienLamTron": 3}'),
        ('RULE_IT',   'TGT_PBA', 1, 0.333334, N'{"uuTienLamTron": 1}'),
        ('RULE_IT',   'TGT_PBB', 2, 0.333333, N'{"uuTienLamTron": 2}'),
        ('RULE_IT',   'TGT_PBC', 3, 0.333333, N'{"uuTienLamTron": 3}');
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.RuleVersion WHERE MaRuleVersion = 'RV_DIEN_V1')
    BEGIN
        INSERT INTO dbo.RuleVersion (MaRuleVersion, MaRule, MaCostPool, MaDriver, SoPhienBan, NgayHieuLuc, NgayHetHieuLuc, CongThuc, ThuTuThucHien, ThamSoJson, TrangThai, NguoiTao)
        VALUES
        ('RV_DIEN_V1', 'RULE_DIEN', 'CP_DIEN_2610', 'DRV_DIENTICH', 1, '2026-01-01', '2026-12-31', N'SoTienPhanBo = TongTienNguon * (GiaTriDriver / TongDriver)', 1, N'{"roundingScale": 2, "ratioScale": 6}', 'ACTIVE', 'USR_FINANCE'),
        ('RV_IT_V1',   'RULE_IT',   'CP_IT_2610',   'DRV_NHANSU',   1, '2026-01-01', '2026-12-31', N'SoTienPhanBo = TongTienNguon * (GiaTriDriver / TongDriver)', 2, N'{"roundingScale": 2, "ratioScale": 6}', 'ACTIVE', 'USR_FINANCE');

        INSERT INTO dbo.RuleVersionTarget (MaRuleVersion, MaRule, MaTarget, ThuTuTarget, TyLeMacDinh, ThamSoJson)
        VALUES
        ('RV_DIEN_V1', 'RULE_DIEN', 'TGT_PBA', 1, 0.400000, N'{"uuTienLamTron": 1}'),
        ('RV_DIEN_V1', 'RULE_DIEN', 'TGT_PBB', 2, 0.350000, N'{"uuTienLamTron": 2}'),
        ('RV_DIEN_V1', 'RULE_DIEN', 'TGT_PBC', 3, 0.250000, N'{"uuTienLamTron": 3}'),
        ('RV_IT_V1',   'RULE_IT',   'TGT_PBA', 1, 0.333334, N'{"uuTienLamTron": 1}'),
        ('RV_IT_V1',   'RULE_IT',   'TGT_PBB', 2, 0.333333, N'{"uuTienLamTron": 2}'),
        ('RV_IT_V1',   'RULE_IT',   'TGT_PBC', 3, 0.333333, N'{"uuTienLamTron": 3}');
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Simulation WHERE MaSimulation = 'SIM_DIEN_2610_01')
    BEGIN
        INSERT INTO dbo.Simulation (MaSimulation, MaRuleVersion, MaRule, MaCostPool, MaDriver, NguoiThucHien, KyPhanBo, TongTienNguon, TrangThai, GhiChu)
        VALUES ('SIM_DIEN_2610_01', 'RV_DIEN_V1', 'RULE_DIEN', 'CP_DIEN_2610', 'DRV_DIENTICH', 'USR_FINANCE', '2026-10', 100000000.00, 'COMPLETED', N'Mô phỏng chi phí điện kỳ 2026-10 trước khi chạy chính thức');

        INSERT INTO dbo.SimulationResult (MaSimulationResult, MaSimulation, MaRuleVersion, MaDriver, MaTarget, KyPhanBo, MaDriverValue, GiaTriDriver, TongDriver, TyLePhanBo, SoTienNguon, SoTienPhanBo)
        VALUES
        ('SIMRES_01', 'SIM_DIEN_2610_01', 'RV_DIEN_V1', 'DRV_DIENTICH', 'TGT_PBA', '2026-10', 'DV_DT_PBA_2610', 400.0000, 1000.0000, 0.400000, 100000000.00, 40000000.00),
        ('SIMRES_02', 'SIM_DIEN_2610_01', 'RV_DIEN_V1', 'DRV_DIENTICH', 'TGT_PBB', '2026-10', 'DV_DT_PBB_2610', 350.0000, 1000.0000, 0.350000, 100000000.00, 35000000.00),
        ('SIMRES_03', 'SIM_DIEN_2610_01', 'RV_DIEN_V1', 'DRV_DIENTICH', 'TGT_PBC', '2026-10', 'DV_DT_PBC_2610', 250.0000, 1000.0000, 0.250000, 100000000.00, 25000000.00);
    END


    IF NOT EXISTS (SELECT 1 FROM dbo.AllocationRun WHERE MaRun = 'RUN_DIEN_2610_01')
    BEGIN
        INSERT INTO dbo.AllocationRun (MaRun, MaCostPool, MaRule, MaDriver, MaRuleVersion, NguoiThucHien, KyPhanBo, TongTienNguon, TongTienPhanBo, TrangThai)
        VALUES ('RUN_DIEN_2610_01', 'CP_DIEN_2610', 'RULE_DIEN', 'DRV_DIENTICH', 'RV_DIEN_V1', 'USR_FINANCE', '2026-10', 100000000.00, 0.00, 'DRAFT');

        INSERT INTO dbo.AllocationResult (MaResult, MaRun, MaRule, MaRuleVersion, MaTarget, MaDriver, KyPhanBo, MaDriverValue, GiaTriDriver, TongDriver, TyLePhanBo, SoTienNguon, SoTienPhanBo, TrangThai)
        VALUES
        ('RES_DIEN_2610_PBA', 'RUN_DIEN_2610_01', 'RULE_DIEN', 'RV_DIEN_V1', 'TGT_PBA', 'DRV_DIENTICH', '2026-10', 'DV_DT_PBA_2610', 400.0000, 1000.0000, 0.400000, 100000000.00, 40000000.00, 'CALCULATED'),
        ('RES_DIEN_2610_PBB', 'RUN_DIEN_2610_01', 'RULE_DIEN', 'RV_DIEN_V1', 'TGT_PBB', 'DRV_DIENTICH', '2026-10', 'DV_DT_PBB_2610', 350.0000, 1000.0000, 0.350000, 100000000.00, 35000000.00, 'CALCULATED'),
        ('RES_DIEN_2610_PBC', 'RUN_DIEN_2610_01', 'RULE_DIEN', 'RV_DIEN_V1', 'TGT_PBC', 'DRV_DIENTICH', '2026-10', 'DV_DT_PBC_2610', 250.0000, 1000.0000, 0.250000, 100000000.00, 25000000.00, 'CALCULATED');

        UPDATE dbo.AllocationRun
        SET TongTienPhanBo = 100000000.00,
            TrangThai = 'CALCULATED'
        WHERE MaRun = 'RUN_DIEN_2610_01';

        UPDATE dbo.AllocationRun
        SET TrangThai = 'PENDING_REVIEW'
        WHERE MaRun = 'RUN_DIEN_2610_01';

        INSERT INTO dbo.Review (MaReview, MaRun, LanDuyet, NguoiDuyet, TrangThai, NhanXet)
        VALUES ('REV_DIEN_2610_01', 'RUN_DIEN_2610_01', 1, 'USR_REVIEWER', 'APPROVED', N'Số liệu phân bổ điện kỳ 2026-10 khớp hóa đơn EVN và biên bản diện tích.');

        INSERT INTO dbo.[Lock] (MaLock, MaRun, NguoiKhoa, LyDoKhoa)
        VALUES ('LCK_DIEN_2610_01', 'RUN_DIEN_2610_01', 'USR_REVIEWER', N'Chốt sổ chi phí điện kỳ 2026-10');

        INSERT INTO dbo.AuditLog (MaNguoiDung, HanhDong, DoiTuong, MaDoiTuong, NoiDung, GiaTriCu, GiaTriMoi, DiaChiIP)
        VALUES
        ('USR_FINANCE',  'RUN_ALLOCATION', 'AllocationRun', 'RUN_DIEN_2610_01', N'Thực hiện phân bổ chính thức chi phí điện kỳ 2026-10', N'{"TrangThai":"DRAFT"}',      N'{"TrangThai":"CALCULATED","TongTienPhanBo":100000000.00}', '127.0.0.1'),
        ('USR_REVIEWER', 'REVIEW_APPROVE', 'AllocationRun', 'RUN_DIEN_2610_01', N'Phê duyệt kết quả phân bổ RUN_DIEN_2610_01',           N'{"TrangThai":"PENDING_REVIEW"}', N'{"TrangThai":"REVIEWED","MaReview":"REV_DIEN_2610_01"}',   '127.0.0.1'),
        ('USR_REVIEWER', 'LOCK_RUN',       'AllocationRun', 'RUN_DIEN_2610_01', N'Khóa sổ kết quả phân bổ RUN_DIEN_2610_01',             N'{"TrangThai":"REVIEWED"}',   N'{"TrangThai":"LOCKED","MaLock":"LCK_DIEN_2610_01"}',       '127.0.0.1');
    END

    COMMIT TRANSACTION;
    PRINT N'Nạp dữ liệu mẫu và kiểm chứng quy trình Trigger thành công!';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO