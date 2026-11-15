using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedCost.API.Security;
using SharedCost.Application.Common.Interfaces;
using SharedCost.Application.DTOs;
using SharedCost.Application.Services;
using SharedCost.Domain.Common;
using SharedCost.Domain.Exceptions;

namespace SharedCost.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IPhanBoChiPhiDbContext _context;
    private readonly IOidcTokenService _tokenService;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLog;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public AuthController(
        IPhanBoChiPhiDbContext context,
        IOidcTokenService tokenService,
        ICurrentUserService currentUser,
        IAuditLogService auditLog,
        IWebHostEnvironment env,
        IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _currentUser = currentUser;
        _auditLog = auditLog;
        _env = env;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("oidc-login")]
    public async Task<ActionResult<AuthSessionDto>> OidcLogin([FromBody] OidcLoginRequest request, CancellationToken ct)
    {
        var query = _context.NguoiDungs
            .Include(u => u.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.OidcIssuer) && !string.IsNullOrWhiteSpace(request.OidcSubject))
        {
            query = query.Where(u => u.OidcIssuer == request.OidcIssuer && u.OidcSubject == request.OidcSubject);
        }
        else if (!string.IsNullOrWhiteSpace(request.TenDangNhap))
        {
            var loginName = request.TenDangNhap.Trim();
            query = query.Where(u => u.TenDangNhap == loginName || u.MaNguoiDung == loginName || u.Email == loginName);
        }
        else
        {
            throw new DomainException("Vui lòng nhập tên đăng nhập hoặc email tài khoản tổ chức.", "INVALID_LOGIN_REQUEST");
        }

        var user = await query.FirstOrDefaultAsync(ct)
            ?? throw new DomainException("Không tìm thấy tài khoản trong hệ thống. Vui lòng kiểm tra lại thông tin đăng nhập.", "USER_NOT_FOUND");

        if (!string.Equals(user.TrangThai, CommonStatus.Active, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException($"Tài khoản '{user.TenDangNhap}' hiện đang ở trạng thái '{user.TrangThai}', không thể truy cập hệ thống.", "USER_INACTIVE_OR_LOCKED");
        }

        var token = _tokenService.IssueOidcJwtToken(user);

        var userDto = new NguoiDungDto(
            user.MaNguoiDung,
            user.HoTen,
            user.TenDangNhap,
            user.Email,
            user.OidcIssuer,
            user.OidcSubject,
            user.MaRole,
            user.Role?.TenRole ?? user.MaRole,
            user.TrangThai);

        await _auditLog.LogAsync(
            "USER_LOGIN",
            "NguoiDung",
            user.MaNguoiDung,
            $"Người dùng {user.HoTen} ({user.TenDangNhap}) đăng nhập hệ thống",
            null,
            new { user.TenDangNhap, user.MaRole },
            ct);

        return Ok(new AuthSessionDto(token, userDto));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<NguoiDungDto>> GetCurrentUser(CancellationToken ct)
    {
        var user = await _context.NguoiDungs
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.MaNguoiDung == _currentUser.MaNguoiDung, ct)
            ?? throw new DomainException("Không tìm thấy thông tin phiên làm việc hiện tại.", "USER_NOT_FOUND");

        return Ok(new NguoiDungDto(
            user.MaNguoiDung,
            user.HoTen,
            user.TenDangNhap,
            user.Email,
            user.OidcIssuer,
            user.OidcSubject,
            user.MaRole,
            user.Role?.TenRole ?? user.MaRole,
            user.TrangThai));
    }

    [AllowAnonymous]
    [HttpGet("demo-accounts")]
    public async Task<ActionResult<List<NguoiDungDto>>> GetDemoAccounts(CancellationToken ct)
    {
        bool allowDemo = _env.IsDevelopment() || _configuration.GetValue<bool>("OidcSettings:EnableDemoMode");
        if (!allowDemo)
        {
            return NotFound();
        }

        var list = await _context.NguoiDungs
            .AsNoTracking()
            .Include(u => u.Role)
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

        return Ok(list);
    }
}

[ApiController]
[Route("api")]
[Authorize]
public class UsersAndRolesController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public UsersAndRolesController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("roles")]
    public async Task<ActionResult<List<RoleDto>>> GetRoles(CancellationToken ct) =>
        Ok(await _catalogService.GetRolesAsync(ct));

    [Authorize(Policy = "AdminOrAuditor")]
    [HttpGet("users")]
    public async Task<ActionResult<List<NguoiDungDto>>> GetUsers(
        [FromQuery] string? maRole,
        [FromQuery] string? trangThai,
        [FromQuery] string? keyword,
        CancellationToken ct) =>
        Ok(await _catalogService.GetUsersAsync(maRole, trangThai, keyword, ct));

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("users")]
    public async Task<ActionResult<NguoiDungDto>> SaveUser([FromBody] SaveNguoiDungRequest request, CancellationToken ct) =>
        Ok(await _catalogService.SaveUserAsync(request, ct));
}
