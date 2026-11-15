using System.Security.Claims;
using SharedCost.Application.Common.Interfaces;

namespace SharedCost.API.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string MaNguoiDung =>
        User?.FindFirstValue("ma_nguoi_dung")
        ?? User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? "USR_ADMIN";

    public string TenDangNhap =>
        User?.FindFirstValue("preferred_username")
        ?? User?.FindFirstValue(ClaimTypes.Name)
        ?? "admin";

    public string MaRole =>
        User?.FindFirstValue(ClaimTypes.Role)
        ?? User?.FindFirstValue("role")
        ?? "ADMIN";

    public string IpAddress =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
        ?? "127.0.0.1";
}
