using Microsoft.EntityFrameworkCore;
using SharedCost.Application.Common.Interfaces;
using SharedCost.Application.Common.Validation;
using SharedCost.Application.DTOs;
using SharedCost.Domain.Common;
using SharedCost.Domain.Entities;
using SharedCost.Domain.Exceptions;

namespace SharedCost.Application.Services;

public interface IRuleManagementService
{
    Task<List<AllocationRuleDto>> GetRulesAsync(string? trangThai = null, string? keyword = null, CancellationToken ct = default);
    Task<AllocationRuleDto> SaveAllocationRuleAsync(SaveAllocationRuleRequest request, CancellationToken ct = default);

    Task<List<RuleVersionDto>> GetRuleVersionsAsync(string? maRule = null, string? trangThai = null, CancellationToken ct = default);
    Task<RuleVersionDto> CreateRuleVersionAsync(CreateRuleVersionRequest request, CancellationToken ct = default);
    Task<RuleVersionDto> ChangeRuleVersionStatusAsync(string maRuleVersion, string trangThaiMoi, CancellationToken ct = default);
}

public class RuleManagementService : IRuleManagementService
{
    private readonly IPhanBoChiPhiDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLog;

    public RuleManagementService(
        IPhanBoChiPhiDbContext context,
        ICurrentUserService currentUser,
        IAuditLogService auditLog)
    {
        _context = context;
        _currentUser = currentUser;
        _auditLog = auditLog;
    }

    public async Task<List<AllocationRuleDto>> GetRulesAsync(string? trangThai = null, string? keyword = null, CancellationToken ct = default)
    {
        var query = _context.AllocationRules
            .AsNoTracking()
            .Include(r => r.CostPool)
            .Include(r => r.Driver)
            .Include(r => r.RuleTargets)
                .ThenInclude(rt => rt.Target)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(r => r.TrangThai == trangThai);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(r => r.MaRule.Contains(keyword) || r.TenRule.Contains(keyword));

        var list = await query.OrderBy(r => r.ThuTuThucHien).ThenBy(r => r.MaRule).ToListAsync(ct);

        return list.Select(r => new AllocationRuleDto(
            r.MaRule,
            r.TenRule,
            r.MaCostPool,
            r.CostPool?.TenCostPool ?? r.MaCostPool,
            r.MaDriver,
            r.Driver?.TenDriver ?? r.MaDriver,
            r.CongThuc,
            r.ThuTuThucHien,
            r.TrangThai,
            r.MoTa,
            r.RuleTargets
                .OrderBy(rt => rt.ThuTuTarget)
                .Select(rt => new RuleTargetItemDto(
                    rt.MaTarget,
                    rt.Target?.TenTarget ?? rt.MaTarget,
                    rt.ThuTuTarget,
                    rt.TyLeMacDinh,
                    rt.ThamSoJson))
                .ToList()
        )).ToList();
    }

    public async Task<AllocationRuleDto> SaveAllocationRuleAsync(SaveAllocationRuleRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.MaRule) || string.IsNullOrWhiteSpace(request.TenRule) || string.IsNullOrWhiteSpace(request.CongThuc))
            throw new DomainException("Lỗi BR05/BR06: Mã quy tắc, Tên quy tắc và Công thức không được để trống.", "BR05_INVALID_RULE");

        if (request.Targets == null || request.Targets.Count == 0)
            throw new DomainException("Lỗi BR05: Quy tắc phân bổ phải xác định ít nhất một Target nhận chi phí.", "BR05_EMPTY_RULE_TARGETS");

        var costPool = await _context.CostPools.FirstOrDefaultAsync(c => c.MaCostPool == request.MaCostPool, ct)
            ?? throw new DomainException($"Lỗi BR05: Cost Pool '{request.MaCostPool}' không tồn tại.", "BR05_COSTPOOL_NOT_FOUND");

        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.MaDriver == request.MaDriver, ct)
            ?? throw new DomainException($"Lỗi BR05: Driver '{request.MaDriver}' không tồn tại.", "BR05_DRIVER_NOT_FOUND");

        foreach (var t in request.Targets)
        {
            PeriodValidator.ValidateOptionalJson(t.ThamSoJson, $"ThamSoJson({t.MaTarget})");
        }

        var existing = await _context.AllocationRules
            .Include(r => r.RuleTargets)
            .FirstOrDefaultAsync(r => r.MaRule == request.MaRule, ct);

        var userId = string.IsNullOrWhiteSpace(_currentUser.MaNguoiDung) ? "USR_FINANCE" : _currentUser.MaNguoiDung;

        if (existing == null)
        {
            if (await _context.AllocationRules.AnyAsync(r => r.TenRule == request.TenRule, ct))
                throw new DomainException($"Tên quy tắc '{request.TenRule}' đã tồn tại.", "DUPLICATE_RULE_NAME");

            var newRule = new AllocationRule
            {
                MaRule = request.MaRule.Trim(),
                TenRule = request.TenRule.Trim(),
                MaCostPool = request.MaCostPool.Trim(),
                MaDriver = request.MaDriver.Trim(),
                CongThuc = request.CongThuc.Trim(),
                ThuTuThucHien = request.ThuTuThucHien > 0 ? request.ThuTuThucHien : 1,
                TrangThai = request.TrangThai,
                MoTa = request.MoTa?.Trim()
            };

            foreach (var item in request.Targets)
            {
                newRule.RuleTargets.Add(new RuleTarget
                {
                    MaRule = newRule.MaRule,
                    MaTarget = item.MaTarget.Trim(),
                    ThuTuTarget = item.ThuTuTarget > 0 ? item.ThuTuTarget : 1,
                    TyLeMacDinh = item.TyLeMacDinh,
                    ThamSoJson = string.IsNullOrWhiteSpace(item.ThamSoJson) ? null : item.ThamSoJson.Trim()
                });
            }

            _context.AllocationRules.Add(newRule);

            // Khởi tạo phiên bản mặc định đầu tiên (V1) và đóng băng danh sách Target (BR07)
            var rawVerId = $"RV_{newRule.MaRule}_V1";
            var verId = rawVerId.Length <= 30 ? rawVerId : $"RV_{DateTime.Now:yyMMddHHmmss}_V1";
            var version1 = new RuleVersion
            {
                MaRuleVersion = verId,
                MaRule = newRule.MaRule,
                MaCostPool = newRule.MaCostPool,
                MaDriver = newRule.MaDriver,
                SoPhienBan = 1,
                NgayHieuLuc = request.NgayHieuLucVersionDau?.Date ?? new DateTime(DateTime.Now.Year, 1, 1),
                NgayHetHieuLuc = null,
                CongThuc = newRule.CongThuc,
                ThuTuThucHien = newRule.ThuTuThucHien,
                ThamSoJson = @"{""roundingScale"": 2, ""ratioScale"": 6}",
                TrangThai = newRule.TrangThai == CommonStatus.Active ? CommonStatus.Active : CommonStatus.Draft,
                NguoiTao = userId
            };

            foreach (var item in request.Targets)
            {
                version1.RuleVersionTargets.Add(new RuleVersionTarget
                {
                    MaRuleVersion = version1.MaRuleVersion,
                    MaRule = newRule.MaRule,
                    MaTarget = item.MaTarget.Trim(),
                    ThuTuTarget = item.ThuTuTarget > 0 ? item.ThuTuTarget : 1,
                    TyLeMacDinh = item.TyLeMacDinh,
                    ThamSoJson = string.IsNullOrWhiteSpace(item.ThamSoJson) ? null : item.ThamSoJson.Trim()
                });
            }

            _context.RuleVersions.Add(version1);
            await _context.SaveChangesAsync(ct);

            await _auditLog.LogAsync(
                "CREATE_RULE",
                "AllocationRule",
                newRule.MaRule,
                $"Tạo mới quy tắc {newRule.TenRule} kèm phiên bản đầu tiên {version1.MaRuleVersion}",
                null,
                new { newRule.MaRule, newRule.TenRule, Version1 = version1.MaRuleVersion },
                ct);
        }
        else
        {
            // Cập nhật mẫu cấu hình mặc định trên AllocationRule và RuleTarget (các RuleVersion lịch sử vẫn bất biến theo BR07)
            var oldSnapshot = new { existing.TenRule, existing.MaCostPool, existing.MaDriver, existing.CongThuc, existing.TrangThai };

            existing.TenRule = request.TenRule.Trim();
            existing.MaCostPool = request.MaCostPool.Trim();
            existing.MaDriver = request.MaDriver.Trim();
            existing.CongThuc = request.CongThuc.Trim();
            existing.ThuTuThucHien = request.ThuTuThucHien > 0 ? request.ThuTuThucHien : 1;
            existing.TrangThai = request.TrangThai;
            existing.MoTa = request.MoTa?.Trim();

            _context.RuleTargets.RemoveRange(existing.RuleTargets);
            foreach (var item in request.Targets)
            {
                _context.RuleTargets.Add(new RuleTarget
                {
                    MaRule = existing.MaRule,
                    MaTarget = item.MaTarget.Trim(),
                    ThuTuTarget = item.ThuTuTarget > 0 ? item.ThuTuTarget : 1,
                    TyLeMacDinh = item.TyLeMacDinh,
                    ThamSoJson = string.IsNullOrWhiteSpace(item.ThamSoJson) ? null : item.ThamSoJson.Trim()
                });
            }

            await _context.SaveChangesAsync(ct);
            await _auditLog.LogAsync("UPDATE_RULE", "AllocationRule", existing.MaRule, $"Cập nhật quy tắc mẫu {existing.TenRule}", oldSnapshot, new { existing.TenRule, existing.MaCostPool, existing.MaDriver, existing.CongThuc, existing.TrangThai }, ct);
        }

        var rules = await GetRulesAsync(null, request.MaRule, ct);
        return rules.First(r => r.MaRule == request.MaRule);
    }

    public async Task<List<RuleVersionDto>> GetRuleVersionsAsync(string? maRule = null, string? trangThai = null, CancellationToken ct = default)
    {
        var query = _context.RuleVersions
            .AsNoTracking()
            .Include(rv => rv.AllocationRule)
            .Include(rv => rv.CostPool)
            .Include(rv => rv.Driver)
            .Include(rv => rv.RuleVersionTargets)
                .ThenInclude(rvt => rvt.Target)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(maRule))
            query = query.Where(rv => rv.MaRule == maRule);

        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(rv => rv.TrangThai == trangThai);

        var list = await query
            .OrderBy(rv => rv.MaRule)
            .ThenByDescending(rv => rv.SoPhienBan)
            .ToListAsync(ct);

        return list.Select(rv => new RuleVersionDto(
            rv.MaRuleVersion,
            rv.MaRule,
            rv.AllocationRule?.TenRule ?? rv.MaRule,
            rv.MaCostPool,
            rv.CostPool?.TenCostPool ?? rv.MaCostPool,
            rv.MaDriver,
            rv.Driver?.TenDriver ?? rv.MaDriver,
            rv.SoPhienBan,
            rv.NgayHieuLuc,
            rv.NgayHetHieuLuc,
            rv.CongThuc,
            rv.ThuTuThucHien,
            rv.ThamSoJson,
            rv.TrangThai,
            rv.NguoiTao,
            rv.RuleVersionTargets
                .OrderBy(t => t.ThuTuTarget)
                .Select(t => new RuleTargetItemDto(
                    t.MaTarget,
                    t.Target?.TenTarget ?? t.MaTarget,
                    t.ThuTuTarget,
                    t.TyLeMacDinh,
                    t.ThamSoJson))
                .ToList()
        )).ToList();
    }

    public async Task<RuleVersionDto> CreateRuleVersionAsync(CreateRuleVersionRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.MaRule) || string.IsNullOrWhiteSpace(request.CongThuc))
            throw new DomainException("Lỗi BR07: Mã quy tắc gốc và Công thức phiên bản là bắt buộc.", "BR07_INVALID_RULE_VERSION");

        if (request.Targets == null || request.Targets.Count == 0)
            throw new DomainException("Lỗi BR05: Phiên bản quy tắc phải có danh sách Target nhận chi phí.", "BR05_EMPTY_VERSION_TARGETS");

        if (request.NgayHetHieuLuc.HasValue && request.NgayHetHieuLuc.Value.Date < request.NgayHieuLuc.Date)
            throw new DomainException("Ngày hết hiệu lực phải lớn hơn hoặc bằng Ngày hiệu lực.", "INVALID_EFFECTIVE_DATES");

        PeriodValidator.ValidateOptionalJson(request.ThamSoJson, nameof(request.ThamSoJson));

        var rule = await _context.AllocationRules.FirstOrDefaultAsync(r => r.MaRule == request.MaRule, ct)
            ?? throw new DomainException($"Quy tắc gốc '{request.MaRule}' không tồn tại.", "RULE_NOT_FOUND");

        var maxVersion = await _context.RuleVersions
            .Where(rv => rv.MaRule == request.MaRule)
            .Select(rv => (int?)rv.SoPhienBan)
            .MaxAsync(ct) ?? 0;

        int nextVersion = maxVersion + 1;
        string maRv = string.IsNullOrWhiteSpace(request.MaRuleVersion)
            ? $"RV_{request.MaRule}_V{nextVersion}"
            : request.MaRuleVersion.Trim();

        if (maRv.Length > 30)
            maRv = $"RV_{DateTime.Now:yyMMddHHmmss}_V{nextVersion}";

        if (await _context.RuleVersions.AnyAsync(rv => rv.MaRuleVersion == maRv, ct))
            throw new DomainException($"Mã phiên bản quy tắc '{maRv}' đã tồn tại (BR07: Không được ghi đè phiên bản cũ).", "BR07_DUPLICATE_RULE_VERSION");

        // Nếu kích hoạt ngay phiên bản mới, chuyển phiên bản ACTIVE hiện hành của cùng MaRule sang EXPIRED để đảm bảo UX_RuleVersion_OneActivePerRule
        if (request.KichHoatNgay)
        {
            var currentActives = await _context.RuleVersions
                .Where(rv => rv.MaRule == request.MaRule && rv.TrangThai == CommonStatus.Active)
                .ToListAsync(ct);

            foreach (var activeVer in currentActives)
            {
                activeVer.TrangThai = CommonStatus.Expired;
            }
            await _context.SaveChangesAsync(ct);
        }

        var userId = string.IsNullOrWhiteSpace(_currentUser.MaNguoiDung) ? "USR_FINANCE" : _currentUser.MaNguoiDung;

        var newVersion = new RuleVersion
        {
            MaRuleVersion = maRv,
            MaRule = rule.MaRule,
            MaCostPool = request.MaCostPool.Trim(),
            MaDriver = request.MaDriver.Trim(),
            SoPhienBan = nextVersion,
            NgayHieuLuc = request.NgayHieuLuc.Date,
            NgayHetHieuLuc = request.NgayHetHieuLuc?.Date,
            CongThuc = request.CongThuc.Trim(),
            ThuTuThucHien = request.ThuTuThucHien > 0 ? request.ThuTuThucHien : rule.ThuTuThucHien,
            ThamSoJson = string.IsNullOrWhiteSpace(request.ThamSoJson) ? null : request.ThamSoJson.Trim(),
            TrangThai = request.KichHoatNgay ? CommonStatus.Active : CommonStatus.Draft,
            NguoiTao = userId
        };

        foreach (var item in request.Targets)
        {
            PeriodValidator.ValidateOptionalJson(item.ThamSoJson, $"ThamSoJson({item.MaTarget})");
            newVersion.RuleVersionTargets.Add(new RuleVersionTarget
            {
                MaRuleVersion = newVersion.MaRuleVersion,
                MaRule = newVersion.MaRule,
                MaTarget = item.MaTarget.Trim(),
                ThuTuTarget = item.ThuTuTarget > 0 ? item.ThuTuTarget : 1,
                TyLeMacDinh = item.TyLeMacDinh,
                ThamSoJson = string.IsNullOrWhiteSpace(item.ThamSoJson) ? null : item.ThamSoJson.Trim()
            });
        }

        _context.RuleVersions.Add(newVersion);
        await _context.SaveChangesAsync(ct);

        await _auditLog.LogAsync(
            "CREATE_RULE_VERSION",
            "RuleVersion",
            newVersion.MaRuleVersion,
            $"Phát hành phiên bản quy tắc mới {newVersion.MaRuleVersion} (V{newVersion.SoPhienBan}) cho quy tắc {newVersion.MaRule}",
            null,
            new { newVersion.MaRuleVersion, newVersion.MaRule, newVersion.SoPhienBan, newVersion.TrangThai },
            ct);

        var versions = await GetRuleVersionsAsync(newVersion.MaRule, null, ct);
        return versions.First(v => v.MaRuleVersion == newVersion.MaRuleVersion);
    }

    public async Task<RuleVersionDto> ChangeRuleVersionStatusAsync(string maRuleVersion, string trangThaiMoi, CancellationToken ct = default)
    {
        var version = await _context.RuleVersions.FirstOrDefaultAsync(rv => rv.MaRuleVersion == maRuleVersion, ct)
            ?? throw new DomainException($"Phiên bản quy tắc '{maRuleVersion}' không tồn tại.", "RULE_VERSION_NOT_FOUND");

        var oldStatus = version.TrangThai;

        if (trangThaiMoi == CommonStatus.Active && oldStatus != CommonStatus.Active)
        {
            var otherActives = await _context.RuleVersions
                .Where(rv => rv.MaRule == version.MaRule && rv.MaRuleVersion != maRuleVersion && rv.TrangThai == CommonStatus.Active)
                .ToListAsync(ct);

            foreach (var activeVer in otherActives)
            {
                activeVer.TrangThai = CommonStatus.Expired;
            }
            await _context.SaveChangesAsync(ct);
        }

        version.TrangThai = trangThaiMoi;
        await _context.SaveChangesAsync(ct);

        await _auditLog.LogAsync(
            "CHANGE_VERSION_STATUS",
            "RuleVersion",
            version.MaRuleVersion,
            $"Chuyển trạng thái RuleVersion {version.MaRuleVersion} từ {oldStatus} sang {trangThaiMoi}",
            new { TrangThai = oldStatus },
            new { TrangThai = trangThaiMoi },
            ct);

        var versions = await GetRuleVersionsAsync(version.MaRule, null, ct);
        return versions.First(v => v.MaRuleVersion == maRuleVersion);
    }
}
