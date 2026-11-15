using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SharedCost.API.Middleware;
using SharedCost.API.Security;
using SharedCost.Application;
using SharedCost.Application.Common.Interfaces;
using SharedCost.Domain.Common;
using SharedCost.Domain.Entities;
using SharedCost.Infrastructure;
using SharedCost.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IOidcTokenService, OidcTokenService>();

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services
    .AddAuthentication(OidcAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, OidcAuthenticationHandler>(OidcAuthenticationHandler.SchemeName, null);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(RoleCodes.Admin));

    options.AddPolicy("AdminOrAuditor", policy =>
        policy.RequireRole(RoleCodes.Admin, RoleCodes.Auditor));

    options.AddPolicy("CanManageCatalog", policy =>
        policy.RequireRole(RoleCodes.Admin, RoleCodes.Finance));

    options.AddPolicy("CanReviewAndLock", policy =>
        policy.RequireRole(RoleCodes.Admin, RoleCodes.Reviewer));

    options.AddPolicy("CanViewAudit", policy =>
        policy.RequireRole(RoleCodes.Admin, RoleCodes.Auditor, RoleCodes.Reviewer, RoleCodes.Finance));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SharedCost Portal API",
        Version = "v1",
        Description = "Hệ thống quản lý và phân bổ chi phí dùng chung (OIDC + RBAC)"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập OIDC Access Token theo cú pháp: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Đảm bảo 5 Role và 5 tài khoản kiểm thử mặc định luôn có sẵn trong CSDL
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PhanBoChiPhiDbContext>();
    if (await db.Database.CanConnectAsync())
    {
        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Role { MaRole = "ADMIN", TenRole = "Quản trị hệ thống", MoTa = "Quản lý người dùng và cấu hình hệ thống", TrangThai = "ACTIVE" },
                new Role { MaRole = "FINANCE", TenRole = "Nhân viên tài chính", MoTa = "Quản lý Cost Pool và dữ liệu phân bổ", TrangThai = "ACTIVE" },
                new Role { MaRole = "REVIEWER", TenRole = "Người phê duyệt", MoTa = "Kiểm tra và phê duyệt kết quả phân bổ", TrangThai = "ACTIVE" },
                new Role { MaRole = "VIEWER", TenRole = "Người xem", MoTa = "Chỉ xem dữ liệu và báo cáo được cấp quyền", TrangThai = "ACTIVE" },
                new Role { MaRole = "AUDITOR", TenRole = "Kiểm toán viên", MoTa = "Tra cứu lịch sử và nhật ký kiểm toán", TrangThai = "ACTIVE" }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.NguoiDungs.AnyAsync())
        {
            db.NguoiDungs.AddRange(
                new NguoiDung { MaNguoiDung = "USR_ADMIN", HoTen = "Nguyễn Quản Trị", TenDangNhap = "admin", Email = "admin@sharedcost.local", OidcIssuer = "https://oidc.sharedcost.local", OidcSubject = "sub-admin-001", MaRole = "ADMIN", TrangThai = "ACTIVE" },
                new NguoiDung { MaNguoiDung = "USR_FINANCE", HoTen = "Trần Tài Chính", TenDangNhap = "finance", Email = "finance@sharedcost.local", OidcIssuer = "https://oidc.sharedcost.local", OidcSubject = "sub-finance-001", MaRole = "FINANCE", TrangThai = "ACTIVE" },
                new NguoiDung { MaNguoiDung = "USR_REVIEWER", HoTen = "Lê Kiểm Duyệt", TenDangNhap = "reviewer", Email = "reviewer@sharedcost.local", OidcIssuer = "https://oidc.sharedcost.local", OidcSubject = "sub-reviewer-001", MaRole = "REVIEWER", TrangThai = "ACTIVE" },
                new NguoiDung { MaNguoiDung = "USR_AUDITOR", HoTen = "Phạm Kiểm Toán", TenDangNhap = "auditor", Email = "auditor@sharedcost.local", OidcIssuer = "https://oidc.sharedcost.local", OidcSubject = "sub-auditor-001", MaRole = "AUDITOR", TrangThai = "ACTIVE" },
                new NguoiDung { MaNguoiDung = "USR_VIEWER", HoTen = "Hoàng Quản Lý", TenDangNhap = "viewer", Email = "viewer@sharedcost.local", OidcIssuer = "https://oidc.sharedcost.local", OidcSubject = "sub-viewer-001", MaRole = "VIEWER", TrangThai = "ACTIVE" }
            );
            await db.SaveChangesAsync();
        }
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SharedCost Portal API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
