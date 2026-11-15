using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedCost.Application.Common.Interfaces;
using SharedCost.Domain.Common;
using SharedCost.Domain.Entities;

namespace SharedCost.API.Security;

public class OidcTokenPayload
{
    public string iss { get; set; } = string.Empty;
    public string sub { get; set; } = string.Empty;
    public string preferred_username { get; set; } = string.Empty;
    public string ma_nguoi_dung { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string role { get; set; } = string.Empty;
    public long iat { get; set; }
    public long exp { get; set; }
}

public interface IOidcTokenService
{
    string IssueOidcJwtToken(NguoiDung user);
    bool TryValidateOidcJwtToken(string token, out OidcTokenPayload? payload, out string error);
}

public class OidcTokenService : IOidcTokenService
{
    private readonly byte[] _signingKey;

    public OidcTokenService(IConfiguration configuration)
    {
        var secret = configuration["OidcSettings:SigningSecret"]
            ?? "SharedCostPortal_OIDC_HmacSha256_SigningSecretKey_2026_Secure";
        _signingKey = Encoding.UTF8.GetBytes(secret);
    }

    public string IssueOidcJwtToken(NguoiDung user)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var headerJson = JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" });
        var payloadJson = JsonSerializer.Serialize(new OidcTokenPayload
        {
            iss = user.OidcIssuer,
            sub = user.OidcSubject,
            preferred_username = user.TenDangNhap,
            ma_nguoi_dung = user.MaNguoiDung,
            name = user.HoTen,
            role = user.MaRole,
            iat = now,
            exp = now + 8 * 3600 // 8 giờ
        });

        var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var unsignedToken = $"{headerBase64}.{payloadBase64}";
        var signature = ComputeSignature(unsignedToken);

        return $"{unsignedToken}.{signature}";
    }

    public bool TryValidateOidcJwtToken(string token, out OidcTokenPayload? payload, out string error)
    {
        payload = null;
        error = string.Empty;

        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            error = "Định dạng OIDC JWT Token không hợp lệ.";
            return false;
        }

        var unsignedToken = $"{parts[0]}.{parts[1]}";
        var expectedSignature = ComputeSignature(unsignedToken);

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(parts[2]),
                Encoding.UTF8.GetBytes(expectedSignature)))
        {
            error = "Chữ ký số OIDC Token không hợp lệ.";
            return false;
        }

        try
        {
            var payloadBytes = Base64UrlDecode(parts[1]);
            payload = JsonSerializer.Deserialize<OidcTokenPayload>(payloadBytes);
            if (payload == null)
            {
                error = "Không đọc được nội dung OIDC Token.";
                return false;
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (payload.exp < now)
            {
                error = "OIDC Access Token đã hết hạn.";
                return false;
            }

            return true;
        }
        catch
        {
            error = "Lỗi giải mã OIDC Token.";
            return false;
        }
    }

    private string ComputeSignature(string unsignedToken)
    {
        using var hmac = new HMACSHA256(_signingKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] input) =>
        Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }
}

public class OidcAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "OIDC_Bearer";
    private readonly IOidcTokenService _tokenService;
    private readonly IPhanBoChiPhiDbContext _dbContext;

    public OidcAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOidcTokenService tokenService,
        IPhanBoChiPhiDbContext dbContext)
        : base(options, logger, encoder)
    {
        _tokenService = tokenService;
        _dbContext = dbContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var authHeader = authHeaderValues.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return AuthenticateResult.Fail("Thiếu OIDC Access Token.");
        }

        if (!_tokenService.TryValidateOidcJwtToken(token, out var payload, out var error) || payload == null)
        {
            return AuthenticateResult.Fail(error);
        }

        // Đối chiếu cặp khóa (OidcIssuer, OidcSubject) và trạng thái hoạt động của tài khoản (BR14)
        var user = await _dbContext.NguoiDungs
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.OidcIssuer == payload.iss && u.OidcSubject == payload.sub);

        if (user == null)
        {
            return AuthenticateResult.Fail("Không tìm thấy chủ thể OIDC (OidcIssuer, OidcSubject) trong hệ thống.");
        }

        if (!string.Equals(user.TrangThai, CommonStatus.Active, StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail($"Tài khoản '{user.TenDangNhap}' hiện đang ở trạng thái '{user.TrangThai}', không được phép truy cập.");
        }

        if (user.Role == null || !string.Equals(user.Role.TrangThai, CommonStatus.Active, StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail($"Vai trò '{user.MaRole}' của tài khoản đã bị vô hiệu hóa.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.MaNguoiDung),
            new("ma_nguoi_dung", user.MaNguoiDung),
            new("preferred_username", user.TenDangNhap),
            new(ClaimTypes.Name, user.HoTen),
            new("iss", user.OidcIssuer),
            new("sub", user.OidcSubject),
            new(ClaimTypes.Role, user.MaRole),
            new("role", user.MaRole)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
