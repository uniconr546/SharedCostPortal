using System.Data;
using Microsoft.EntityFrameworkCore;
using SharedCost.Application.Common.Interfaces;
using SharedCost.Application.Common.Validation;
using SharedCost.Application.DTOs;
using SharedCost.Domain.Common;
using SharedCost.Domain.Entities;
using SharedCost.Domain.Exceptions;

namespace SharedCost.Application.Services;

public interface IAllocationWorkflowService
{
    Task<List<SimulationDto>> GetSimulationsAsync(string? maRuleVersion = null, string? kyPhanBo = null, CancellationToken ct = default);
    Task<SimulationDto> RunSimulationAsync(RunSimulationRequest request, CancellationToken ct = default);

    Task<List<AllocationRunDto>> GetAllocationRunsAsync(string? kyPhanBo = null, string? trangThai = null, string? maCostPool = null, CancellationToken ct = default);
    Task<AllocationRunDto> GetAllocationRunByIdAsync(string maRun, CancellationToken ct = default);
    Task<AllocationRunDto> ExecuteAllocationRunAsync(ExecuteAllocationRunRequest request, CancellationToken ct = default);

    Task<AllocationRunDto> SubmitForReviewAsync(string maRun, CancellationToken ct = default);
    Task<AllocationRunDto> ReviewAllocationRunAsync(SubmitReviewDecisionRequest request, CancellationToken ct = default);
    Task<AllocationRunDto> LockAllocationRunAsync(LockAllocationRunRequest request, CancellationToken ct = default);

    Task<TraceabilityDto> GetTraceabilityByResultAsync(string maResult, CancellationToken ct = default);
    Task<List<TraceabilityDto>> GetTraceabilityByRunAsync(string maRun, CancellationToken ct = default);
    Task<DashboardSummaryDto> GetDashboardAndReportAsync(string? kyPhanBo = null, string? maCostPool = null, string? maTarget = null, CancellationToken ct = default);
}

public class AllocationWorkflowService : IAllocationWorkflowService
{
    private readonly IPhanBoChiPhiDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLog;

    public AllocationWorkflowService(
        IPhanBoChiPhiDbContext context,
        ICurrentUserService currentUser,
        IAuditLogService auditLog)
    {
        _context = context;
        _currentUser = currentUser;
        _auditLog = auditLog;
    }
    public async Task<List<SimulationDto>> GetSimulationsAsync(string? maRuleVersion = null, string? kyPhanBo = null, CancellationToken ct = default)
    {
        var query = _context.Simulations
            .AsNoTracking()
            .Include(s => s.CostPool)
            .Include(s => s.RuleVersion)
                .ThenInclude(rv => rv!.Driver)
            .Include(s => s.SimulationResults)
                .ThenInclude(sr => sr.RuleVersionTarget)
                    .ThenInclude(rvt => rvt!.Target)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(maRuleVersion))
            query = query.Where(s => s.MaRuleVersion == maRuleVersion);

        if (!string.IsNullOrWhiteSpace(kyPhanBo))
            query = query.Where(s => s.KyPhanBo == kyPhanBo);

        var list = await query.OrderByDescending(s => s.ThoiGianChay).ToListAsync(ct);

        return list.Select(s => new SimulationDto(
            s.MaSimulation,
            s.MaRuleVersion,
            s.MaRule,
            s.MaCostPool,
            s.CostPool?.TenCostPool ?? s.MaCostPool,
            s.MaDriver,
            s.RuleVersion?.Driver?.TenDriver ?? s.MaDriver,
            s.ThoiGianChay,
            s.NguoiThucHien,
            s.KyPhanBo,
            s.TongTienNguon,
            s.TrangThai,
            s.GhiChu,
            s.SimulationResults
                .OrderBy(r => r.MaTarget)
                .Select(r => new SimulationResultItemDto(
                    r.MaSimulationResult,
                    r.MaTarget,
                    r.RuleVersionTarget?.Target?.TenTarget ?? r.MaTarget,
                    r.MaDriverValue,
                    r.GiaTriDriver,
                    r.TongDriver,
                    r.TyLePhanBo,
                    r.SoTienNguon,
                    r.SoTienPhanBo))
                .ToList()
        )).ToList();
    }

    public async Task<SimulationDto> RunSimulationAsync(RunSimulationRequest request, CancellationToken ct = default)
    {
        PeriodValidator.ValidateAndParsePeriod(request.KyPhanBo, nameof(request.KyPhanBo));

        var ruleVersion = await _context.RuleVersions
            .Include(rv => rv.RuleVersionTargets)
            .FirstOrDefaultAsync(rv => rv.MaRuleVersion == request.MaRuleVersion, ct)
            ?? throw new DomainException($"Phiên bản quy tắc '{request.MaRuleVersion}' không tồn tại.", "RULE_VERSION_NOT_FOUND");

        var costPool = await _context.CostPools
            .FirstOrDefaultAsync(cp => cp.MaCostPool == ruleVersion.MaCostPool && cp.KyApDung == request.KyPhanBo, ct)
            ?? throw new DomainException(
                $"Lỗi BR02: Cost Pool '{ruleVersion.MaCostPool}' của phiên bản quy tắc không có dữ liệu khớp với kỳ phân bổ '{request.KyPhanBo}'.",
                "BR02_COSTPOOL_PERIOD_MISMATCH");

        var targetCodes = ruleVersion.RuleVersionTargets.Select(t => t.MaTarget).ToList();
        var driverValues = await _context.DriverValues
            .Where(dv => dv.MaDriver == ruleVersion.MaDriver && dv.KyPhanBo == request.KyPhanBo && targetCodes.Contains(dv.MaTarget))
            .ToListAsync(ct);

        var calcInputs = new List<CalculationTargetInput>();
        foreach (var rvt in ruleVersion.RuleVersionTargets.OrderBy(x => x.ThuTuTarget))
        {
            var dv = driverValues.FirstOrDefault(x => x.MaTarget == rvt.MaTarget)
                ?? throw new DomainException(
                    $"Lỗi BR04: Thiếu giá trị Driver '{ruleVersion.MaDriver}' cho Target '{rvt.MaTarget}' trong kỳ '{request.KyPhanBo}'.",
                    "BR04_MISSING_DRIVER_VALUE");

            calcInputs.Add(new CalculationTargetInput(rvt.MaTarget, rvt.ThuTuTarget, dv.MaDriverValue, dv.GiaTriDriver));
        }

        var calculatedItems = AllocationCalculationEngine.Calculate(costPool.SoTien, calcInputs);

        var simId = string.IsNullOrWhiteSpace(request.MaSimulation)
            ? $"SIM_{DateTime.Now:yyMMddHHmmssfff}"
            : request.MaSimulation.Trim();

        var userId = string.IsNullOrWhiteSpace(_currentUser.MaNguoiDung) ? "USR_FINANCE" : _currentUser.MaNguoiDung;

        var simulation = new Simulation
        {
            MaSimulation = simId,
            MaRuleVersion = ruleVersion.MaRuleVersion,
            MaRule = ruleVersion.MaRule,
            MaCostPool = ruleVersion.MaCostPool,
            MaDriver = ruleVersion.MaDriver,
            ThoiGianChay = DateTime.Now,
            NguoiThucHien = userId,
            KyPhanBo = request.KyPhanBo.Trim(),
            TongTienNguon = costPool.SoTien,
            TrangThai = SimulationStatus.Completed,
            GhiChu = request.GhiChu?.Trim()
        };

        int idx = 1;
        foreach (var item in calculatedItems)
        {
            var resId = $"SR_{DateTime.Now:HHmmssfff}_{idx++:D2}";
            simulation.SimulationResults.Add(new SimulationResult
            {
                MaSimulationResult = resId,
                MaSimulation = simulation.MaSimulation,
                MaRuleVersion = simulation.MaRuleVersion,
                MaDriver = simulation.MaDriver,
                MaTarget = item.MaTarget,
                KyPhanBo = simulation.KyPhanBo,
                MaDriverValue = item.MaDriverValue,
                GiaTriDriver = item.GiaTriDriver,
                TongDriver = item.TongDriver,
                TyLePhanBo = item.TyLePhanBo,
                SoTienNguon = item.SoTienNguon,
                SoTienPhanBo = item.SoTienPhanBo
            });
        }

        _context.Simulations.Add(simulation);
        await _context.SaveChangesAsync(ct);

        await _auditLog.LogAsync(
            "SIMULATE",
            "Simulation",
            simulation.MaSimulation,
            $"Chạy mô phỏng phân bổ {simulation.MaSimulation} với RuleVersion {simulation.MaRuleVersion} kỳ {simulation.KyPhanBo}",
            null,
            new { simulation.MaSimulation, simulation.MaRuleVersion, simulation.KyPhanBo, simulation.TongTienNguon },
            ct);

        var list = await GetSimulationsAsync(simulation.MaRuleVersion, simulation.KyPhanBo, ct);
        return list.First(x => x.MaSimulation == simulation.MaSimulation);
    }
    public async Task<List<AllocationRunDto>> GetAllocationRunsAsync(string? kyPhanBo = null, string? trangThai = null, string? maCostPool = null, CancellationToken ct = default)
    {
        var query = _context.AllocationRuns
            .AsNoTracking()
            .Include(r => r.CostPool)
            .Include(r => r.RuleVersion)
                .ThenInclude(rv => rv!.AllocationRule)
            .Include(r => r.RuleVersion)
                .ThenInclude(rv => rv!.Driver)
            .Include(r => r.AllocationResults)
                .ThenInclude(ar => ar.RuleVersionTarget)
                    .ThenInclude(rvt => rvt!.Target)
            .Include(r => r.Reviews)
                .ThenInclude(rev => rev.NguoiDuyetNavigation)
            .Include(r => r.Lock)
                .ThenInclude(lck => lck!.NguoiKhoaNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(kyPhanBo))
            query = query.Where(r => r.KyPhanBo == kyPhanBo);

        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(r => r.TrangThai == trangThai);

        if (!string.IsNullOrWhiteSpace(maCostPool))
            query = query.Where(r => r.MaCostPool == maCostPool);

        var list = await query.OrderByDescending(r => r.ThoiGianChay).ToListAsync(ct);
        return list.Select(MapAllocationRunDto).ToList();
    }

    public async Task<AllocationRunDto> GetAllocationRunByIdAsync(string maRun, CancellationToken ct = default)
    {
        var runs = await GetAllocationRunsAsync(null, null, null, ct);
        return runs.FirstOrDefault(r => r.MaRun == maRun)
            ?? throw new DomainException($"Lần chạy phân bổ '{maRun}' không tồn tại.", "ALLOCATION_RUN_NOT_FOUND");
    }

    public async Task<AllocationRunDto> ExecuteAllocationRunAsync(ExecuteAllocationRunRequest request, CancellationToken ct = default)
    {
        PeriodValidator.ValidateAndParsePeriod(request.KyPhanBo, nameof(request.KyPhanBo));

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        try
        {
            var ruleVersion = await _context.RuleVersions
                .Include(rv => rv.RuleVersionTargets)
                .FirstOrDefaultAsync(rv => rv.MaRuleVersion == request.MaRuleVersion, ct)
                ?? throw new DomainException($"Phiên bản quy tắc '{request.MaRuleVersion}' không tồn tại.", "RULE_VERSION_NOT_FOUND");

            // Kiểm tra BR13: RuleVersion phải đang ACTIVE và bao phủ trọn kỳ phân bổ
            PeriodValidator.EnsureRuleVersionCoversPeriod(
                ruleVersion.MaRuleVersion,
                ruleVersion.TrangThai,
                ruleVersion.NgayHieuLuc,
                ruleVersion.NgayHetHieuLuc,
                request.KyPhanBo);

            var costPool = await _context.CostPools
                .FirstOrDefaultAsync(cp => cp.MaCostPool == ruleVersion.MaCostPool && cp.KyApDung == request.KyPhanBo, ct)
                ?? throw new DomainException(
                    $"Lỗi BR02/BR09: Cost Pool '{ruleVersion.MaCostPool}' không khớp với kỳ phân bổ '{request.KyPhanBo}'.",
                    "BR09_COSTPOOL_PERIOD_MISMATCH");

            if (!string.Equals(costPool.TrangThai, CommonStatus.Active, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainException($"Cost Pool '{costPool.MaCostPool}' đang ở trạng thái '{costPool.TrangThai}', phải là 'ACTIVE' để chạy phân bổ.", "COSTPOOL_NOT_ACTIVE");
            }

            var targetCodes = ruleVersion.RuleVersionTargets.Select(t => t.MaTarget).ToList();
            var driverValues = await _context.DriverValues
                .Where(dv => dv.MaDriver == ruleVersion.MaDriver && dv.KyPhanBo == request.KyPhanBo && targetCodes.Contains(dv.MaTarget))
                .ToListAsync(ct);

            var calcInputs = new List<CalculationTargetInput>();
            foreach (var rvt in ruleVersion.RuleVersionTargets.OrderBy(x => x.ThuTuTarget))
            {
                var dv = driverValues.FirstOrDefault(x => x.MaTarget == rvt.MaTarget)
                    ?? throw new DomainException(
                        $"Lỗi BR04: Thiếu giá trị Driver '{ruleVersion.MaDriver}' cho Target '{rvt.MaTarget}' trong kỳ '{request.KyPhanBo}'.",
                        "BR04_MISSING_DRIVER_VALUE");

                calcInputs.Add(new CalculationTargetInput(rvt.MaTarget, rvt.ThuTuTarget, dv.MaDriverValue, dv.GiaTriDriver));
            }

            var calculatedItems = AllocationCalculationEngine.Calculate(costPool.SoTien, calcInputs);

            var runId = string.IsNullOrWhiteSpace(request.MaRun)
                ? $"RUN_{DateTime.Now:yyMMddHHmmssfff}"
                : request.MaRun.Trim();

            var userId = string.IsNullOrWhiteSpace(_currentUser.MaNguoiDung) ? "USR_FINANCE" : _currentUser.MaNguoiDung;

            // Bước 1: Chèn AllocationRun ở trạng thái DRAFT (tuân thủ TRG_AllocationRun_ValidateAndStateMachine)
            var run = new AllocationRun
            {
                MaRun = runId,
                MaCostPool = ruleVersion.MaCostPool,
                MaRule = ruleVersion.MaRule,
                MaDriver = ruleVersion.MaDriver,
                MaRuleVersion = ruleVersion.MaRuleVersion,
                ThoiGianChay = DateTime.Now,
                NguoiThucHien = userId,
                KyPhanBo = request.KyPhanBo.Trim(),
                TongTienNguon = costPool.SoTien,
                TongTienPhanBo = 0m,
                TrangThai = AllocationRunStatus.Draft
            };

            _context.AllocationRuns.Add(run);
            await _context.SaveChangesAsync(ct);

            // Bước 2: Chèn toàn bộ AllocationResult khi Run đang ở trạng thái DRAFT (tuân thủ TRG_AllocationResult_ProtectImmutability)
            int idx = 1;
            foreach (var item in calculatedItems)
            {
                var resId = $"RES_{DateTime.Now:HHmmssfff}_{idx++:D2}";
                _context.AllocationResults.Add(new AllocationResult
                {
                    MaResult = resId,
                    MaRun = run.MaRun,
                    MaRule = run.MaRule,
                    MaRuleVersion = run.MaRuleVersion,
                    MaTarget = item.MaTarget,
                    MaDriver = run.MaDriver,
                    KyPhanBo = run.KyPhanBo,
                    MaDriverValue = item.MaDriverValue,
                    GiaTriDriver = item.GiaTriDriver,
                    TongDriver = item.TongDriver,
                    TyLePhanBo = item.TyLePhanBo,
                    SoTienNguon = item.SoTienNguon,
                    SoTienPhanBo = item.SoTienPhanBo,
                    TrangThai = AllocationRunStatus.Calculated
                });
            }

            await _context.SaveChangesAsync(ct);

            // Bước 3: Cập nhật AllocationRun sang CALCULATED và đối soát tổng tiền (Trigger kiểm tra SUM(TyLe)=1 & SUM(Tien)=TongTienNguon)
            run.TongTienPhanBo = costPool.SoTien;
            run.TrangThai = AllocationRunStatus.Calculated;
            await _context.SaveChangesAsync(ct);

            // Bước 4: Ghi AuditLog trong cùng Transaction
            await _auditLog.LogAsync(
                "RUN_ALLOCATION",
                "AllocationRun",
                run.MaRun,
                $"Thực hiện phân bổ chính thức {run.MaRun} với RuleVersion {run.MaRuleVersion} kỳ {run.KyPhanBo}",
                new { TrangThai = AllocationRunStatus.Draft },
                new { TrangThai = AllocationRunStatus.Calculated, run.TongTienNguon, run.TongTienPhanBo },
                ct);

            await transaction.CommitAsync(ct);
            return await GetAllocationRunByIdAsync(run.MaRun, ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
    public async Task<AllocationRunDto> SubmitForReviewAsync(string maRun, CancellationToken ct = default)
    {
        var run = await _context.AllocationRuns.FirstOrDefaultAsync(r => r.MaRun == maRun, ct)
            ?? throw new DomainException($"Lần chạy phân bổ '{maRun}' không tồn tại.", "ALLOCATION_RUN_NOT_FOUND");

        if (run.TrangThai != AllocationRunStatus.Calculated)
        {
            throw new DomainException(
                $"Chỉ lần chạy ở trạng thái 'CALCULATED' mới được gửi chờ duyệt (Trạng thái hiện tại: '{run.TrangThai}').",
                "INVALID_RUN_STATE_TRANSITION");
        }

        run.TrangThai = AllocationRunStatus.PendingReview;
        await _context.SaveChangesAsync(ct);

        await _auditLog.LogAsync(
            "SUBMIT_REVIEW",
            "AllocationRun",
            run.MaRun,
            $"Gửi yêu cầu kiểm duyệt cho lần chạy {run.MaRun}",
            new { TrangThai = AllocationRunStatus.Calculated },
            new { TrangThai = AllocationRunStatus.PendingReview },
            ct);

        return await GetAllocationRunByIdAsync(maRun, ct);
    }

    public async Task<AllocationRunDto> ReviewAllocationRunAsync(SubmitReviewDecisionRequest request, CancellationToken ct = default)
    {
        var run = await _context.AllocationRuns.FirstOrDefaultAsync(r => r.MaRun == request.MaRun, ct)
            ?? throw new DomainException($"Lần chạy phân bổ '{request.MaRun}' không tồn tại.", "ALLOCATION_RUN_NOT_FOUND");

        if (run.TrangThai != AllocationRunStatus.Calculated && run.TrangThai != AllocationRunStatus.PendingReview)
        {
            throw new DomainException(
                $"Chỉ được phép kiểm duyệt (Review) khi AllocationRun đang ở trạng thái 'CALCULATED' hoặc 'PENDING_REVIEW' (Hiện tại: '{run.TrangThai}').",
                "INVALID_REVIEW_STATE");
        }

        var decision = request.TrangThai?.Trim().ToUpperInvariant() ?? "";
        if (decision != ReviewDecisionStatus.Approved && decision != ReviewDecisionStatus.Rejected)
        {
            throw new DomainException("Quyết định kiểm duyệt chỉ nhận giá trị 'APPROVED' hoặc 'REJECTED'.", "INVALID_REVIEW_DECISION");
        }

        if (decision == ReviewDecisionStatus.Rejected && string.IsNullOrWhiteSpace(request.NhanXet))
        {
            throw new DomainException("Khi từ chối kết quả phân bổ ('REJECTED'), bắt buộc phải nhập lý do từ chối vào trường Nhận xét.", "MISSING_REJECTION_REASON");
        }

        var oldState = run.TrangThai;
        var maxLanDuyet = await _context.Reviews
            .Where(r => r.MaRun == request.MaRun)
            .Select(r => (int?)r.LanDuyet)
            .MaxAsync(ct) ?? 0;

        var reviewerId = string.IsNullOrWhiteSpace(_currentUser.MaNguoiDung) ? "USR_REVIEWER" : _currentUser.MaNguoiDung;
        var review = new Review
        {
            MaReview = $"REV_{DateTime.Now:yyMMddHHmmssfff}",
            MaRun = run.MaRun,
            LanDuyet = maxLanDuyet + 1,
            NguoiDuyet = reviewerId,
            ThoiGianDuyet = DateTime.Now,
            TrangThai = decision,
            NhanXet = request.NhanXet?.Trim()
        };

        // Khi thêm bản ghi Review, Trigger TRG_Review_ValidateAndSyncRun tự động cập nhật AllocationRun.TrangThai sang REVIEWED hoặc REJECTED
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(ct);
        await _context.Entry(run).ReloadAsync(ct);

        await _auditLog.LogAsync(
            decision == ReviewDecisionStatus.Approved ? "REVIEW_APPROVE" : "REVIEW_REJECT",
            "AllocationRun",
            run.MaRun,
            $"Kiểm duyệt ({decision}) lần chạy {run.MaRun}: {review.NhanXet}",
            new { TrangThai = oldState },
            new { TrangThai = run.TrangThai, review.MaReview, review.LanDuyet },
            ct);

        return await GetAllocationRunByIdAsync(run.MaRun, ct);
    }

    public async Task<AllocationRunDto> LockAllocationRunAsync(LockAllocationRunRequest request, CancellationToken ct = default)
    {
        var run = await _context.AllocationRuns.FirstOrDefaultAsync(r => r.MaRun == request.MaRun, ct)
            ?? throw new DomainException($"Lần chạy phân bổ '{request.MaRun}' không tồn tại.", "ALLOCATION_RUN_NOT_FOUND");

        if (run.TrangThai != AllocationRunStatus.Reviewed)
        {
            throw new DomainException(
                $"Lỗi BR11: Chỉ được phép khóa sổ (Lock) khi AllocationRun đã ở trạng thái 'REVIEWED' (Hiện tại: '{run.TrangThai}').",
                "BR11_LOCK_REQUIRES_REVIEWED");
        }

        var lockerId = string.IsNullOrWhiteSpace(_currentUser.MaNguoiDung) ? "USR_REVIEWER" : _currentUser.MaNguoiDung;
        var lockEntity = new Lock
        {
            MaLock = $"LCK_{DateTime.Now:yyMMddHHmmssfff}",
            MaRun = run.MaRun,
            NguoiKhoa = lockerId,
            ThoiGianKhoa = DateTime.Now,
            LyDoKhoa = string.IsNullOrWhiteSpace(request.LyDoKhoa) ? "Khóa sổ kết quả phân bổ chính thức" : request.LyDoKhoa.Trim()
        };

        // Khi thêm bản ghi Lock, Trigger TRG_Lock_ValidateAndSyncRun tự động chuyển AllocationRun.TrangThai sang LOCKED
        _context.Locks.Add(lockEntity);
        await _context.SaveChangesAsync(ct);
        await _context.Entry(run).ReloadAsync(ct);

        await _auditLog.LogAsync(
            "LOCK_RUN",
            "AllocationRun",
            run.MaRun,
            $"Khóa sổ vĩnh viễn lần chạy {run.MaRun}: {lockEntity.LyDoKhoa}",
            new { TrangThai = AllocationRunStatus.Reviewed },
            new { TrangThai = run.TrangThai, lockEntity.MaLock },
            ct);

        return await GetAllocationRunByIdAsync(run.MaRun, ct);
    }
    public async Task<TraceabilityDto> GetTraceabilityByResultAsync(string maResult, CancellationToken ct = default)
    {
        var ar = await _context.AllocationResults
            .AsNoTracking()
            .Include(x => x.AllocationRun)
                .ThenInclude(r => r!.CostPool)
            .Include(x => x.AllocationRun)
                .ThenInclude(r => r!.RuleVersion)
                    .ThenInclude(rv => rv!.AllocationRule)
            .Include(x => x.AllocationRun)
                .ThenInclude(r => r!.RuleVersion)
                    .ThenInclude(rv => rv!.Driver)
            .Include(x => x.AllocationRun)
                .ThenInclude(r => r!.Reviews)
                    .ThenInclude(rev => rev.NguoiDuyetNavigation)
            .Include(x => x.AllocationRun)
                .ThenInclude(r => r!.Lock)
                    .ThenInclude(lck => lck!.NguoiKhoaNavigation)
            .Include(x => x.RuleVersionTarget)
                .ThenInclude(rvt => rvt!.Target)
            .Include(x => x.DriverValue)
            .FirstOrDefaultAsync(x => x.MaResult == maResult, ct)
            ?? throw new DomainException($"Kết quả phân bổ '{maResult}' không tồn tại.", "RESULT_NOT_FOUND");

        return MapTraceabilityDto(ar);
    }

    public async Task<List<TraceabilityDto>> GetTraceabilityByRunAsync(string maRun, CancellationToken ct = default)
    {
        var results = await _context.AllocationResults
            .AsNoTracking()
            .Include(x => x.AllocationRun)
                .ThenInclude(r => r!.CostPool)
            .Include(x => x.AllocationRun)
                .ThenInclude(r => r!.RuleVersion)
                    .ThenInclude(rv => rv!.AllocationRule)
            .Include(x => x.AllocationRun)
                .ThenInclude(r => r!.RuleVersion)
                    .ThenInclude(rv => rv!.Driver)
            .Include(x => x.AllocationRun)
                .ThenInclude(r => r!.Reviews)
                    .ThenInclude(rev => rev.NguoiDuyetNavigation)
            .Include(x => x.AllocationRun)
                .ThenInclude(r => r!.Lock)
                    .ThenInclude(lck => lck!.NguoiKhoaNavigation)
            .Include(x => x.RuleVersionTarget)
                .ThenInclude(rvt => rvt!.Target)
            .Include(x => x.DriverValue)
            .Where(x => x.MaRun == maRun)
            .OrderBy(x => x.RuleVersionTarget != null ? x.RuleVersionTarget.ThuTuTarget : 1)
            .ThenBy(x => x.MaResult)
            .ToListAsync(ct);

        return results.Select(MapTraceabilityDto).ToList();
    }

    public async Task<DashboardSummaryDto> GetDashboardAndReportAsync(string? kyPhanBo = null, string? maCostPool = null, string? maTarget = null, CancellationToken ct = default)
    {
        var costPoolsQuery = _context.CostPools.AsNoTracking().AsQueryable();
        var runsQuery = _context.AllocationRuns.AsNoTracking().AsQueryable();
        var resultsQuery = _context.AllocationResults
            .AsNoTracking()
            .Include(ar => ar.AllocationRun)
            .Include(ar => ar.RuleVersionTarget)
                .ThenInclude(rvt => rvt!.Target)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(kyPhanBo))
        {
            costPoolsQuery = costPoolsQuery.Where(c => c.KyApDung == kyPhanBo);
            runsQuery = runsQuery.Where(r => r.KyPhanBo == kyPhanBo);
            resultsQuery = resultsQuery.Where(ar => ar.KyPhanBo == kyPhanBo);
        }

        if (!string.IsNullOrWhiteSpace(maCostPool))
        {
            costPoolsQuery = costPoolsQuery.Where(c => c.MaCostPool == maCostPool);
            runsQuery = runsQuery.Where(r => r.MaCostPool == maCostPool);
            resultsQuery = resultsQuery.Where(ar => ar.AllocationRun != null && ar.AllocationRun.MaCostPool == maCostPool);
        }

        if (!string.IsNullOrWhiteSpace(maTarget))
        {
            resultsQuery = resultsQuery.Where(ar => ar.MaTarget == maTarget);
        }

        var costPools = await costPoolsQuery.ToListAsync(ct);
        var runs = await runsQuery.ToListAsync(ct);
        var results = await resultsQuery.ToListAsync(ct);
        var activeRulesCount = await _context.AllocationRules.CountAsync(r => r.TrangThai == CommonStatus.Active, ct);
        var allTargets = await _context.Targets.AsNoTracking().ToListAsync(ct);

        var targetReport = allTargets
            .Where(t => string.IsNullOrWhiteSpace(maTarget) || t.MaTarget == maTarget)
            .Select(t =>
            {
                var targetResults = results.Where(r => r.MaTarget == t.MaTarget && r.AllocationRun?.TrangThai != AllocationRunStatus.Rejected).ToList();
                return new TargetReportItemDto(
                    t.MaTarget,
                    t.TenTarget,
                    t.LoaiTarget,
                    targetResults.Select(x => x.MaRun).Distinct().Count(),
                    targetResults.Sum(x => x.SoTienPhanBo));
            })
            .OrderByDescending(x => x.TongTienNhanPhanBo)
            .ToList();

        var costPoolReport = costPools
            .Select(cp =>
            {
                var cpRuns = runs.Where(r => r.MaCostPool == cp.MaCostPool).OrderByDescending(r => r.ThoiGianChay).ToList();
                var validRuns = cpRuns.Where(r => r.TrangThai != AllocationRunStatus.Rejected).ToList();
                return new CostPoolReportItemDto(
                    cp.MaCostPool,
                    cp.TenCostPool,
                    cp.KyApDung,
                    cp.SoTien,
                    validRuns.Sum(r => r.TongTienPhanBo),
                    cpRuns.FirstOrDefault()?.TrangThai ?? "CHƯA CHẠY");
            })
            .OrderByDescending(x => x.KyApDung)
            .ThenBy(x => x.MaCostPool)
            .ToList();

        return new DashboardSummaryDto(
            costPools.Count,
            costPools.Sum(c => c.SoTien),
            activeRulesCount,
            runs.Count,
            runs.Count(r => r.TrangThai == AllocationRunStatus.PendingReview || r.TrangThai == AllocationRunStatus.Calculated),
            runs.Count(r => r.TrangThai == AllocationRunStatus.Locked),
            targetReport,
            costPoolReport);
    }

    private static AllocationRunDto MapAllocationRunDto(AllocationRun r)
    {
        return new AllocationRunDto(
            r.MaRun,
            r.MaCostPool,
            r.CostPool?.TenCostPool ?? r.MaCostPool,
            r.MaRule,
            r.RuleVersion?.AllocationRule?.TenRule ?? r.MaRule,
            r.MaDriver,
            r.RuleVersion?.Driver?.TenDriver ?? r.MaDriver,
            r.MaRuleVersion,
            r.RuleVersion?.SoPhienBan ?? 1,
            r.ThoiGianChay,
            r.NguoiThucHien,
            r.KyPhanBo,
            r.TongTienNguon,
            r.TongTienPhanBo,
            r.TrangThai,
            r.AllocationResults
                .OrderBy(ar => ar.RuleVersionTarget?.ThuTuTarget ?? 1)
                .ThenBy(ar => ar.MaTarget)
                .Select(ar => new AllocationResultItemDto(
                    ar.MaResult,
                    ar.MaRun,
                    ar.MaTarget,
                    ar.RuleVersionTarget?.Target?.TenTarget ?? ar.MaTarget,
                    ar.MaDriver,
                    ar.MaDriverValue,
                    ar.KyPhanBo,
                    ar.GiaTriDriver,
                    ar.TongDriver,
                    ar.TyLePhanBo,
                    ar.SoTienNguon,
                    ar.SoTienPhanBo,
                    ar.TrangThai))
                .ToList(),
            r.Reviews
                .OrderBy(rev => rev.LanDuyet)
                .Select(rev => new ReviewDto(
                    rev.MaReview,
                    rev.MaRun,
                    rev.LanDuyet,
                    rev.NguoiDuyet,
                    rev.NguoiDuyetNavigation?.HoTen ?? rev.NguoiDuyet,
                    rev.ThoiGianDuyet,
                    rev.TrangThai,
                    rev.NhanXet))
                .ToList(),
            r.Lock == null
                ? null
                : new LockDto(
                    r.Lock.MaLock,
                    r.Lock.MaRun,
                    r.Lock.NguoiKhoa,
                    r.Lock.NguoiKhoaNavigation?.HoTen ?? r.Lock.NguoiKhoa,
                    r.Lock.ThoiGianKhoa,
                    r.Lock.LyDoKhoa));
    }

    private static TraceabilityDto MapTraceabilityDto(AllocationResult ar)
    {
        var run = ar.AllocationRun!;
        var rv = run.RuleVersion!;
        var cp = run.CostPool!;
        var drv = rv.Driver!;
        var dv = ar.DriverValue!;
        var rvt = ar.RuleVersionTarget!;
        var tgt = rvt.Target!;

        return new TraceabilityDto(
            ar.MaResult,
            run.MaRun,
            run.TrangThai,
            run.KyPhanBo,
            run.ThoiGianChay,
            run.NguoiThucHien,
            cp.MaCostPool,
            cp.TenCostPool,
            run.TongTienNguon,
            cp.NguonDuLieu,
            rv.MaRule,
            rv.AllocationRule?.TenRule ?? rv.MaRule,
            rv.MaRuleVersion,
            rv.SoPhienBan,
            rv.NgayHieuLuc,
            rv.NgayHetHieuLuc,
            rv.CongThuc,
            drv.MaDriver,
            drv.TenDriver,
            drv.DonViTinh,
            dv.MaDriverValue,
            ar.GiaTriDriver,
            ar.TongDriver,
            dv.NguonDuLieu,
            tgt.MaTarget,
            tgt.TenTarget,
            rvt.ThuTuTarget,
            ar.TyLePhanBo,
            ar.SoTienPhanBo,
            run.Reviews
                .OrderBy(rev => rev.LanDuyet)
                .Select(rev => new ReviewDto(
                    rev.MaReview,
                    rev.MaRun,
                    rev.LanDuyet,
                    rev.NguoiDuyet,
                    rev.NguoiDuyetNavigation?.HoTen ?? rev.NguoiDuyet,
                    rev.ThoiGianDuyet,
                    rev.TrangThai,
                    rev.NhanXet))
                .ToList(),
            run.Lock == null
                ? null
                : new LockDto(
                    run.Lock.MaLock,
                    run.Lock.MaRun,
                    run.Lock.NguoiKhoa,
                    run.Lock.NguoiKhoaNavigation?.HoTen ?? run.Lock.NguoiKhoa,
                    run.Lock.ThoiGianKhoa,
                    run.Lock.LyDoKhoa));
    }
}
