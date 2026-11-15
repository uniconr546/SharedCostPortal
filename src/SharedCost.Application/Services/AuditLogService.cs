using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SharedCost.Application.Common.Interfaces;
using SharedCost.Application.DTOs;
using SharedCost.Domain.Entities;

namespace SharedCost.Application.Services;

public interface IAuditLogService
{
    Task LogAsync(
        string hanhDong,
        string doiTuong,
        string? maDoiTuong,
        string? noiDung,
        object? giaTriCu = null,
        object? giaTriMoi = null,
        CancellationToken cancellationToken = default);

    Task<List<AuditLogDto>> GetLogsAsync(
        string? doiTuong = null,
        string? hanhDong = null,
        string? maNguoiDung = null,
        CancellationToken cancellationToken = default);
}

public class AuditLogService : IAuditLogService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly IPhanBoChiPhiDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AuditLogService(IPhanBoChiPhiDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task LogAsync(
        string hanhDong,
        string doiTuong,
        string? maDoiTuong,
        string? noiDung,
        object? giaTriCu = null,
        object? giaTriMoi = null,
        CancellationToken cancellationToken = default)
    {
        var userId = string.IsNullOrWhiteSpace(_currentUser.MaNguoiDung)
            ? "USR_ADMIN"
            : _currentUser.MaNguoiDung;

        string? oldJson = giaTriCu switch
        {
            null => null,
            string s => s,
            _ => JsonSerializer.Serialize(giaTriCu, JsonOptions)
        };

        string? newJson = giaTriMoi switch
        {
            null => null,
            string s => s,
            _ => JsonSerializer.Serialize(giaTriMoi, JsonOptions)
        };

        var log = new AuditLog
        {
            MaNguoiDung = userId,
            ThoiGian = DateTime.Now,
            HanhDong = hanhDong,
            DoiTuong = doiTuong,
            MaDoiTuong = maDoiTuong,
            NoiDung = noiDung,
            GiaTriCu = oldJson,
            GiaTriMoi = newJson,
            DiaChiIP = string.IsNullOrWhiteSpace(_currentUser.IpAddress) ? "127.0.0.1" : _currentUser.IpAddress
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AuditLogDto>> GetLogsAsync(
        string? doiTuong = null,
        string? hanhDong = null,
        string? maNguoiDung = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Include(x => x.NguoiDung)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(doiTuong))
            query = query.Where(x => x.DoiTuong == doiTuong);

        if (!string.IsNullOrWhiteSpace(hanhDong))
            query = query.Where(x => x.HanhDong == hanhDong);

        if (!string.IsNullOrWhiteSpace(maNguoiDung))
            query = query.Where(x => x.MaNguoiDung == maNguoiDung);

        return await query
            .OrderByDescending(x => x.ThoiGian)
            .ThenByDescending(x => x.MaAuditLog)
            .Take(500)
            .Select(x => new AuditLogDto(
                x.MaAuditLog,
                x.MaNguoiDung,
                x.NguoiDung != null ? x.NguoiDung.HoTen : x.MaNguoiDung,
                x.ThoiGian,
                x.HanhDong,
                x.DoiTuong,
                x.MaDoiTuong,
                x.NoiDung,
                x.GiaTriCu,
                x.GiaTriMoi,
                x.DiaChiIP))
            .ToListAsync(cancellationToken);
    }
}
