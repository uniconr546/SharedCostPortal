using Microsoft.EntityFrameworkCore;
using SharedCost.Application.Common.Interfaces;
using SharedCost.Application.Common.Validation;
using SharedCost.Application.DTOs;
using SharedCost.Domain.Common;
using SharedCost.Domain.Entities;
using SharedCost.Domain.Exceptions;

namespace SharedCost.Application.Services;

public interface ICatalogService
{
    Task<List<RoleDto>> GetRolesAsync(CancellationToken ct = default);
    Task<List<NguoiDungDto>> GetUsersAsync(string? maRole = null, string? trangThai = null, string? keyword = null, CancellationToken ct = default);
    Task<NguoiDungDto> SaveUserAsync(SaveNguoiDungRequest request, CancellationToken ct = default);

    Task<List<CostPoolDto>> GetCostPoolsAsync(string? kyApDung = null, string? trangThai = null, string? keyword = null, CancellationToken ct = default);
    Task<CostPoolDto> SaveCostPoolAsync(SaveCostPoolRequest request, CancellationToken ct = default);

    Task<List<DriverDto>> GetDriversAsync(string? trangThai = null, string? keyword = null, CancellationToken ct = default);
    Task<DriverDto> SaveDriverAsync(SaveDriverRequest request, CancellationToken ct = default);

    Task<List<TargetDto>> GetTargetsAsync(string? trangThai = null, string? keyword = null, CancellationToken ct = default);
    Task<TargetDto> SaveTargetAsync(SaveTargetRequest request, CancellationToken ct = default);

    Task<List<DriverValueDto>> GetDriverValuesAsync(string? maDriver = null, string? maTarget = null, string? kyPhanBo = null, CancellationToken ct = default);
    Task<DriverValueDto> SaveDriverValueAsync(SaveDriverValueRequest request, CancellationToken ct = default);
}

public class CatalogService : ICatalogService
{
    private readonly IPhanBoChiPhiDbContext _context;
    private readonly IAuditLogService _auditLog;

    public CatalogService(IPhanBoChiPhiDbContext context, IAuditLogService auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }
    public async Task<List<RoleDto>> GetRolesAsync(CancellationToken ct = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.MaRole)
            .Select(r => new RoleDto(r.MaRole, r.TenRole, r.MoTa, r.TrangThai))
            .ToListAsync(ct);
    }

    public async Task<List<NguoiDungDto>> GetUsersAsync(string? maRole = null, string? trangThai = null, string? keyword = null, CancellationToken ct = default)
    {
        var query = _context.NguoiDungs
            .AsNoTracking()
            .Include(u => u.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(maRole))
            query = query.Where(u => u.MaRole == maRole);

        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(u => u.TrangThai == trangThai);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(u => u.MaNguoiDung.Contains(keyword) || u.HoTen.Contains(keyword) || u.TenDangNhap.Contains(keyword));

        return await query
            .OrderBy(u => u.MaNguoiDung)
            .Select(u => new NguoiDungDto(
                u.MaNguoiDung,
                u.HoTen,
                u.TenDangNhap,
                u.Email,
                u.OidcIssuer,
                u.OidcSubject,
                u.MaRole,
                u.Role != null ? u.Role.TenRole : u.MaRole,
                u.TrangThai))
            .ToListAsync(ct);
    }

    public async Task<NguoiDungDto> SaveUserAsync(SaveNguoiDungRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.MaNguoiDung) || string.IsNullOrWhiteSpace(request.TenDangNhap) || string.IsNullOrWhiteSpace(request.HoTen))
            throw new DomainException("Mã người dùng, Họ tên và Tên đăng nhập không được để trống.", "INVALID_USER_INPUT");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.MaRole == request.MaRole, ct)
            ?? throw new DomainException($"Vai trò '{request.MaRole}' không tồn tại trong hệ thống.", "BR14_ROLE_NOT_FOUND");

        var existing = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung == request.MaNguoiDung, ct);
        if (existing == null)
        {
            if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == request.TenDangNhap, ct))
                throw new DomainException($"Tên đăng nhập '{request.TenDangNhap}' đã tồn tại.", "DUPLICATE_USERNAME");

            var newUser = new NguoiDung
            {
                MaNguoiDung = request.MaNguoiDung.Trim(),
                HoTen = request.HoTen.Trim(),
                TenDangNhap = request.TenDangNhap.Trim(),
                Email = request.Email?.Trim(),
                OidcIssuer = string.IsNullOrWhiteSpace(request.OidcIssuer) ? "https://oidc.sharedcost.local" : request.OidcIssuer.Trim(),
                OidcSubject = string.IsNullOrWhiteSpace(request.OidcSubject) ? $"sub-{request.TenDangNhap.Trim()}" : request.OidcSubject.Trim(),
                MaRole = request.MaRole,
                TrangThai = request.TrangThai
            };

            _context.NguoiDungs.Add(newUser);
            await _context.SaveChangesAsync(ct);
            await _auditLog.LogAsync("CREATE_USER", "NguoiDung", newUser.MaNguoiDung, $"Tạo mới người dùng {newUser.HoTen}", null, newUser, ct);

            return new NguoiDungDto(newUser.MaNguoiDung, newUser.HoTen, newUser.TenDangNhap, newUser.Email, newUser.OidcIssuer, newUser.OidcSubject, newUser.MaRole, role.TenRole, newUser.TrangThai);
        }
        else
        {
            var oldSnapshot = new { existing.HoTen, existing.TenDangNhap, existing.Email, existing.MaRole, existing.TrangThai };
            existing.HoTen = request.HoTen.Trim();
            existing.Email = request.Email?.Trim();
            existing.MaRole = request.MaRole;
            existing.TrangThai = request.TrangThai;
            if (!string.IsNullOrWhiteSpace(request.OidcIssuer)) existing.OidcIssuer = request.OidcIssuer.Trim();
            if (!string.IsNullOrWhiteSpace(request.OidcSubject)) existing.OidcSubject = request.OidcSubject.Trim();

            await _context.SaveChangesAsync(ct);
            await _auditLog.LogAsync("UPDATE_USER", "NguoiDung", existing.MaNguoiDung, $"Cập nhật người dùng {existing.HoTen}", oldSnapshot, existing, ct);

            return new NguoiDungDto(existing.MaNguoiDung, existing.HoTen, existing.TenDangNhap, existing.Email, existing.OidcIssuer, existing.OidcSubject, existing.MaRole, role.TenRole, existing.TrangThai);
        }
    }
    public async Task<List<CostPoolDto>> GetCostPoolsAsync(string? kyApDung = null, string? trangThai = null, string? keyword = null, CancellationToken ct = default)
    {
        var query = _context.CostPools.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(kyApDung))
            query = query.Where(c => c.KyApDung == kyApDung);

        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(c => c.TrangThai == trangThai);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(c => c.MaCostPool.Contains(keyword) || c.TenCostPool.Contains(keyword) || (c.NguonDuLieu != null && c.NguonDuLieu.Contains(keyword)));

        return await query
            .OrderByDescending(c => c.KyApDung)
            .ThenBy(c => c.MaCostPool)
            .Select(c => new CostPoolDto(c.MaCostPool, c.TenCostPool, c.KyApDung, c.SoTien, c.NguonDuLieu, c.MoTa, c.TrangThai))
            .ToListAsync(ct);
    }

    public async Task<CostPoolDto> SaveCostPoolAsync(SaveCostPoolRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.MaCostPool) || string.IsNullOrWhiteSpace(request.TenCostPool))
            throw new DomainException("Lỗi BR01: Mã Cost Pool và Tên Cost Pool không được để trống.", "BR01_INVALID_COSTPOOL");

        PeriodValidator.ValidateAndParsePeriod(request.KyApDung, nameof(request.KyApDung));

        if (request.SoTien < 0)
            throw new DomainException("Lỗi BR02: Số tiền của Cost Pool phải lớn hơn hoặc bằng 0.", "BR02_NEGATIVE_AMOUNT");

        var existing = await _context.CostPools.FirstOrDefaultAsync(c => c.MaCostPool == request.MaCostPool, ct);
        if (existing == null)
        {
            if (await _context.CostPools.AnyAsync(c => c.TenCostPool == request.TenCostPool && c.KyApDung == request.KyApDung, ct))
                throw new DomainException($"Lỗi BR01: Tên Cost Pool '{request.TenCostPool}' đã tồn tại trong kỳ '{request.KyApDung}'.", "BR01_DUPLICATE_COSTPOOL_NAME_PERIOD");

            var entity = new CostPool
            {
                MaCostPool = request.MaCostPool.Trim(),
                TenCostPool = request.TenCostPool.Trim(),
                KyApDung = request.KyApDung.Trim(),
                SoTien = Math.Round(request.SoTien, 2),
                NguonDuLieu = request.NguonDuLieu?.Trim(),
                MoTa = request.MoTa?.Trim(),
                TrangThai = request.TrangThai
            };

            _context.CostPools.Add(entity);
            await _context.SaveChangesAsync(ct);
            await _auditLog.LogAsync("CREATE_COSTPOOL", "CostPool", entity.MaCostPool, $"Thêm mới Cost Pool {entity.TenCostPool} kỳ {entity.KyApDung}", null, entity, ct);

            return new CostPoolDto(entity.MaCostPool, entity.TenCostPool, entity.KyApDung, entity.SoTien, entity.NguonDuLieu, entity.MoTa, entity.TrangThai);
        }
        else
        {
            bool isLockedInRun = await _context.AllocationRuns.AnyAsync(
                r => r.MaCostPool == existing.MaCostPool && r.TrangThai == AllocationRunStatus.Locked, ct);

            if (isLockedInRun && (existing.SoTien != Math.Round(request.SoTien, 2) || existing.KyApDung != request.KyApDung.Trim()))
            {
                throw new DomainException(
                    $"Lỗi BR11: Cost Pool '{existing.MaCostPool}' đã có kết quả phân bổ khóa sổ (LOCKED), không được sửa Số tiền hoặc Kỳ áp dụng.",
                    "BR11_COSTPOOL_LOCKED");
            }

            var oldSnapshot = new { existing.TenCostPool, existing.KyApDung, existing.SoTien, existing.NguonDuLieu, existing.MoTa, existing.TrangThai };
            existing.TenCostPool = request.TenCostPool.Trim();
            existing.KyApDung = request.KyApDung.Trim();
            existing.SoTien = Math.Round(request.SoTien, 2);
            existing.NguonDuLieu = request.NguonDuLieu?.Trim();
            existing.MoTa = request.MoTa?.Trim();
            existing.TrangThai = request.TrangThai;

            await _context.SaveChangesAsync(ct);
            await _auditLog.LogAsync("UPDATE_COSTPOOL", "CostPool", existing.MaCostPool, $"Cập nhật Cost Pool {existing.TenCostPool}", oldSnapshot, existing, ct);

            return new CostPoolDto(existing.MaCostPool, existing.TenCostPool, existing.KyApDung, existing.SoTien, existing.NguonDuLieu, existing.MoTa, existing.TrangThai);
        }
    }
    public async Task<List<DriverDto>> GetDriversAsync(string? trangThai = null, string? keyword = null, CancellationToken ct = default)
    {
        var query = _context.Drivers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(d => d.TrangThai == trangThai);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(d => d.MaDriver.Contains(keyword) || d.TenDriver.Contains(keyword));

        return await query
            .OrderBy(d => d.MaDriver)
            .Select(d => new DriverDto(d.MaDriver, d.TenDriver, d.DonViTinh, d.MoTa, d.TrangThai))
            .ToListAsync(ct);
    }

    public async Task<DriverDto> SaveDriverAsync(SaveDriverRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.MaDriver) || string.IsNullOrWhiteSpace(request.TenDriver) || string.IsNullOrWhiteSpace(request.DonViTinh))
            throw new DomainException("Lỗi BR03: Mã Driver, Tên Driver và Đơn vị tính là bắt buộc.", "BR03_INVALID_DRIVER");

        var existing = await _context.Drivers.FirstOrDefaultAsync(d => d.MaDriver == request.MaDriver, ct);
        if (existing == null)
        {
            if (await _context.Drivers.AnyAsync(d => d.TenDriver == request.TenDriver, ct))
                throw new DomainException($"Lỗi BR03: Tên Driver '{request.TenDriver}' đã tồn tại.", "BR03_DUPLICATE_DRIVER_NAME");

            var entity = new Driver
            {
                MaDriver = request.MaDriver.Trim(),
                TenDriver = request.TenDriver.Trim(),
                DonViTinh = request.DonViTinh.Trim(),
                MoTa = request.MoTa?.Trim(),
                TrangThai = request.TrangThai
            };

            _context.Drivers.Add(entity);
            await _context.SaveChangesAsync(ct);
            await _auditLog.LogAsync("CREATE_DRIVER", "Driver", entity.MaDriver, $"Tạo mới tiêu thức {entity.TenDriver}", null, entity, ct);

            return new DriverDto(entity.MaDriver, entity.TenDriver, entity.DonViTinh, entity.MoTa, entity.TrangThai);
        }
        else
        {
            var oldSnapshot = new { existing.TenDriver, existing.DonViTinh, existing.MoTa, existing.TrangThai };
            existing.TenDriver = request.TenDriver.Trim();
            existing.DonViTinh = request.DonViTinh.Trim();
            existing.MoTa = request.MoTa?.Trim();
            existing.TrangThai = request.TrangThai;

            await _context.SaveChangesAsync(ct);
            await _auditLog.LogAsync("UPDATE_DRIVER", "Driver", existing.MaDriver, $"Cập nhật tiêu thức {existing.TenDriver}", oldSnapshot, existing, ct);

            return new DriverDto(existing.MaDriver, existing.TenDriver, existing.DonViTinh, existing.MoTa, existing.TrangThai);
        }
    }
    public async Task<List<TargetDto>> GetTargetsAsync(string? trangThai = null, string? keyword = null, CancellationToken ct = default)
    {
        var query = _context.Targets.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(t => t.TrangThai == trangThai);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(t => t.MaTarget.Contains(keyword) || t.TenTarget.Contains(keyword));

        return await query
            .OrderBy(t => t.MaTarget)
            .Select(t => new TargetDto(t.MaTarget, t.TenTarget, t.LoaiTarget, t.MoTa, t.TrangThai))
            .ToListAsync(ct);
    }

    public async Task<TargetDto> SaveTargetAsync(SaveTargetRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.MaTarget) || string.IsNullOrWhiteSpace(request.TenTarget) || string.IsNullOrWhiteSpace(request.LoaiTarget))
            throw new DomainException("Mã Target, Tên Target và Loại Target không được để trống.", "INVALID_TARGET");

        var existing = await _context.Targets.FirstOrDefaultAsync(t => t.MaTarget == request.MaTarget, ct);
        if (existing == null)
        {
            if (await _context.Targets.AnyAsync(t => t.TenTarget == request.TenTarget, ct))
                throw new DomainException($"Tên Target '{request.TenTarget}' đã tồn tại.", "DUPLICATE_TARGET_NAME");

            var entity = new Target
            {
                MaTarget = request.MaTarget.Trim(),
                TenTarget = request.TenTarget.Trim(),
                LoaiTarget = request.LoaiTarget.Trim(),
                MoTa = request.MoTa?.Trim(),
                TrangThai = request.TrangThai
            };

            _context.Targets.Add(entity);
            await _context.SaveChangesAsync(ct);
            await _auditLog.LogAsync("CREATE_TARGET", "Target", entity.MaTarget, $"Tạo mới đối tượng nhận chi phí {entity.TenTarget}", null, entity, ct);

            return new TargetDto(entity.MaTarget, entity.TenTarget, entity.LoaiTarget, entity.MoTa, entity.TrangThai);
        }
        else
        {
            var oldSnapshot = new { existing.TenTarget, existing.LoaiTarget, existing.MoTa, existing.TrangThai };
            existing.TenTarget = request.TenTarget.Trim();
            existing.LoaiTarget = request.LoaiTarget.Trim();
            existing.MoTa = request.MoTa?.Trim();
            existing.TrangThai = request.TrangThai;

            await _context.SaveChangesAsync(ct);
            await _auditLog.LogAsync("UPDATE_TARGET", "Target", existing.MaTarget, $"Cập nhật đối tượng nhận chi phí {existing.TenTarget}", oldSnapshot, existing, ct);

            return new TargetDto(existing.MaTarget, existing.TenTarget, existing.LoaiTarget, existing.MoTa, existing.TrangThai);
        }
    }
    public async Task<List<DriverValueDto>> GetDriverValuesAsync(string? maDriver = null, string? maTarget = null, string? kyPhanBo = null, CancellationToken ct = default)
    {
        var query = _context.DriverValues
            .AsNoTracking()
            .Include(dv => dv.Driver)
            .Include(dv => dv.Target)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(maDriver))
            query = query.Where(dv => dv.MaDriver == maDriver);

        if (!string.IsNullOrWhiteSpace(maTarget))
            query = query.Where(dv => dv.MaTarget == maTarget);

        if (!string.IsNullOrWhiteSpace(kyPhanBo))
            query = query.Where(dv => dv.KyPhanBo == kyPhanBo);

        return await query
            .OrderByDescending(dv => dv.KyPhanBo)
            .ThenBy(dv => dv.MaDriver)
            .ThenBy(dv => dv.MaTarget)
            .Select(dv => new DriverValueDto(
                dv.MaDriverValue,
                dv.MaDriver,
                dv.Driver != null ? dv.Driver.TenDriver : dv.MaDriver,
                dv.MaTarget,
                dv.Target != null ? dv.Target.TenTarget : dv.MaTarget,
                dv.KyPhanBo,
                dv.GiaTriDriver,
                dv.DonViTinh,
                dv.NguonDuLieu))
            .ToListAsync(ct);
    }

    public async Task<DriverValueDto> SaveDriverValueAsync(SaveDriverValueRequest request, CancellationToken ct = default)
    {
        PeriodValidator.ValidateAndParsePeriod(request.KyPhanBo, nameof(request.KyPhanBo));

        if (request.GiaTriDriver < 0)
            throw new DomainException("Lỗi BR04: Giá trị Driver phải lớn hơn hoặc bằng 0.", "BR04_NEGATIVE_DRIVER_VALUE");

        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.MaDriver == request.MaDriver, ct)
            ?? throw new DomainException($"Lỗi BR04: Driver '{request.MaDriver}' không tồn tại.", "BR04_DRIVER_NOT_FOUND");

        var target = await _context.Targets.FirstOrDefaultAsync(t => t.MaTarget == request.MaTarget, ct)
            ?? throw new DomainException($"Lỗi BR04: Target '{request.MaTarget}' không tồn tại.", "BR04_TARGET_NOT_FOUND");

        var existing = await _context.DriverValues.FirstOrDefaultAsync(
            dv => dv.MaDriverValue == request.MaDriverValue
               || (dv.MaDriver == request.MaDriver && dv.MaTarget == request.MaTarget && dv.KyPhanBo == request.KyPhanBo),
            ct);

        if (existing == null)
        {
            var maDv = string.IsNullOrWhiteSpace(request.MaDriverValue)
                ? $"DV_{DateTime.Now:yyMMddHHmmssfff}"
                : request.MaDriverValue.Trim();

            var entity = new DriverValue
            {
                MaDriverValue = maDv,
                MaDriver = request.MaDriver.Trim(),
                MaTarget = request.MaTarget.Trim(),
                KyPhanBo = request.KyPhanBo.Trim(),
                GiaTriDriver = Math.Round(request.GiaTriDriver, 4),
                DonViTinh = string.IsNullOrWhiteSpace(request.DonViTinh) ? driver.DonViTinh : request.DonViTinh.Trim(),
                NguonDuLieu = request.NguonDuLieu?.Trim()
            };

            _context.DriverValues.Add(entity);
            await _context.SaveChangesAsync(ct);
            await _auditLog.LogAsync("CREATE_DRIVER_VALUE", "DriverValue", entity.MaDriverValue, $"Khai báo giá trị Driver {entity.MaDriver} cho {entity.MaTarget} kỳ {entity.KyPhanBo}", null, entity, ct);

            return new DriverValueDto(entity.MaDriverValue, entity.MaDriver, driver.TenDriver, entity.MaTarget, target.TenTarget, entity.KyPhanBo, entity.GiaTriDriver, entity.DonViTinh, entity.NguonDuLieu);
        }
        else
        {
            bool usedInLockedRun = await _context.AllocationResults
                .Include(ar => ar.AllocationRun)
                .AnyAsync(ar => ar.MaDriverValue == existing.MaDriverValue && ar.AllocationRun != null && ar.AllocationRun.TrangThai == AllocationRunStatus.Locked, ct);

            if (usedInLockedRun && existing.GiaTriDriver != Math.Round(request.GiaTriDriver, 4))
            {
                throw new DomainException(
                    $"Lỗi BR11: Bản ghi DriverValue '{existing.MaDriverValue}' đã được sử dụng trong lần chạy phân bổ đã khóa sổ (LOCKED), không được phép sửa giá trị.",
                    "BR11_DRIVER_VALUE_LOCKED");
            }

            var oldSnapshot = new { existing.GiaTriDriver, existing.DonViTinh, existing.NguonDuLieu };
            existing.GiaTriDriver = Math.Round(request.GiaTriDriver, 4);
            existing.DonViTinh = string.IsNullOrWhiteSpace(request.DonViTinh) ? driver.DonViTinh : request.DonViTinh.Trim();
            existing.NguonDuLieu = request.NguonDuLieu?.Trim();

            await _context.SaveChangesAsync(ct);
            await _auditLog.LogAsync("UPDATE_DRIVER_VALUE", "DriverValue", existing.MaDriverValue, $"Cập nhật giá trị Driver {existing.MaDriver} cho {existing.MaTarget} kỳ {existing.KyPhanBo}", oldSnapshot, existing, ct);

            return new DriverValueDto(existing.MaDriverValue, existing.MaDriver, driver.TenDriver, existing.MaTarget, target.TenTarget, existing.KyPhanBo, existing.GiaTriDriver, existing.DonViTinh, existing.NguonDuLieu);
        }
    }
}
