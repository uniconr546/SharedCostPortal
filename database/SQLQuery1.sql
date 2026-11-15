USE PhanBoChiPhi;
GO

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

