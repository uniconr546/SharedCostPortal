using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCost.Application.DTOs;
using SharedCost.Application.Services;

namespace SharedCost.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class RulesAndVersionsController : ControllerBase
{
    private readonly IRuleManagementService _ruleService;

    public RulesAndVersionsController(IRuleManagementService ruleService)
    {
        _ruleService = ruleService;
    }

    [HttpGet("allocation-rules")]
    public async Task<ActionResult<List<AllocationRuleDto>>> GetRules(
        [FromQuery] string? trangThai,
        [FromQuery] string? keyword,
        CancellationToken ct) =>
        Ok(await _ruleService.GetRulesAsync(trangThai, keyword, ct));

    [Authorize(Policy = "CanManageCatalog")]
    [HttpPost("allocation-rules")]
    public async Task<ActionResult<AllocationRuleDto>> SaveRule(
        [FromBody] SaveAllocationRuleRequest request,
        CancellationToken ct) =>
        Ok(await _ruleService.SaveAllocationRuleAsync(request, ct));

    [HttpGet("rule-versions")]
    public async Task<ActionResult<List<RuleVersionDto>>> GetRuleVersions(
        [FromQuery] string? maRule,
        [FromQuery] string? trangThai,
        CancellationToken ct) =>
        Ok(await _ruleService.GetRuleVersionsAsync(maRule, trangThai, ct));

    [Authorize(Policy = "CanManageCatalog")]
    [HttpPost("rule-versions")]
    public async Task<ActionResult<RuleVersionDto>> CreateRuleVersion(
        [FromBody] CreateRuleVersionRequest request,
        CancellationToken ct) =>
        Ok(await _ruleService.CreateRuleVersionAsync(request, ct));

    [Authorize(Policy = "CanManageCatalog")]
    [HttpPatch("rule-versions/{maRuleVersion}/status")]
    public async Task<ActionResult<RuleVersionDto>> ChangeVersionStatus(
        string maRuleVersion,
        [FromQuery] string trangThaiMoi,
        CancellationToken ct) =>
        Ok(await _ruleService.ChangeRuleVersionStatusAsync(maRuleVersion, trangThaiMoi, ct));
}

[ApiController]
[Route("api")]
[Authorize]
public class AllocationWorkflowController : ControllerBase
{
    private readonly IAllocationWorkflowService _workflowService;
    private readonly IAuditLogService _auditLogService;

    public AllocationWorkflowController(
        IAllocationWorkflowService workflowService,
        IAuditLogService auditLogService)
    {
        _workflowService = workflowService;
        _auditLogService = auditLogService;
    }


    [HttpGet("simulations")]
    public async Task<ActionResult<List<SimulationDto>>> GetSimulations(
        [FromQuery] string? maRuleVersion,
        [FromQuery] string? kyPhanBo,
        CancellationToken ct) =>
        Ok(await _workflowService.GetSimulationsAsync(maRuleVersion, kyPhanBo, ct));

    [Authorize(Policy = "CanManageCatalog")]
    [HttpPost("simulations")]
    public async Task<ActionResult<SimulationDto>> RunSimulation(
        [FromBody] RunSimulationRequest request,
        CancellationToken ct) =>
        Ok(await _workflowService.RunSimulationAsync(request, ct));


    [HttpGet("allocation-runs")]
    public async Task<ActionResult<List<AllocationRunDto>>> GetAllocationRuns(
        [FromQuery] string? kyPhanBo,
        [FromQuery] string? trangThai,
        [FromQuery] string? maCostPool,
        CancellationToken ct) =>
        Ok(await _workflowService.GetAllocationRunsAsync(kyPhanBo, trangThai, maCostPool, ct));

    [HttpGet("allocation-runs/{maRun}")]
    public async Task<ActionResult<AllocationRunDto>> GetAllocationRunById(string maRun, CancellationToken ct) =>
        Ok(await _workflowService.GetAllocationRunByIdAsync(maRun, ct));

    [Authorize(Policy = "CanManageCatalog")]
    [HttpPost("allocation-runs")]
    public async Task<ActionResult<AllocationRunDto>> ExecuteAllocationRun(
        [FromBody] ExecuteAllocationRunRequest request,
        CancellationToken ct)
    {
        var result = await _workflowService.ExecuteAllocationRunAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

  
    [Authorize(Policy = "CanManageCatalog")]
    [HttpPost("allocation-runs/{maRun}/submit-review")]
    public async Task<ActionResult<AllocationRunDto>> SubmitForReview(string maRun, CancellationToken ct) =>
        Ok(await _workflowService.SubmitForReviewAsync(maRun, ct));

    [Authorize(Policy = "CanReviewAndLock")]
    [HttpPost("allocation-runs/review")]
    public async Task<ActionResult<AllocationRunDto>> ReviewRun(
        [FromBody] SubmitReviewDecisionRequest request,
        CancellationToken ct) =>
        Ok(await _workflowService.ReviewAllocationRunAsync(request, ct));

    [Authorize(Policy = "CanReviewAndLock")]
    [HttpPost("allocation-runs/lock")]
    public async Task<ActionResult<AllocationRunDto>> LockRun(
        [FromBody] LockAllocationRunRequest request,
        CancellationToken ct) =>
        Ok(await _workflowService.LockAllocationRunAsync(request, ct));


    [HttpGet("traceability/result/{maResult}")]
    public async Task<ActionResult<TraceabilityDto>> GetTraceabilityByResult(string maResult, CancellationToken ct) =>
        Ok(await _workflowService.GetTraceabilityByResultAsync(maResult, ct));

    [HttpGet("traceability/run/{maRun}")]
    public async Task<ActionResult<List<TraceabilityDto>>> GetTraceabilityByRun(string maRun, CancellationToken ct) =>
        Ok(await _workflowService.GetTraceabilityByRunAsync(maRun, ct));

    [Authorize(Policy = "CanViewAudit")]
    [HttpGet("audit-logs")]
    public async Task<ActionResult<List<AuditLogDto>>> GetAuditLogs(
        [FromQuery] string? doiTuong,
        [FromQuery] string? hanhDong,
        [FromQuery] string? maNguoiDung,
        CancellationToken ct) =>
        Ok(await _auditLogService.GetLogsAsync(doiTuong, hanhDong, maNguoiDung, ct));

    [HttpGet("reports/dashboard")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(
        [FromQuery] string? kyPhanBo,
        [FromQuery] string? maCostPool,
        [FromQuery] string? maTarget,
        CancellationToken ct) =>
        Ok(await _workflowService.GetDashboardAndReportAsync(kyPhanBo, maCostPool, maTarget, ct));
}
