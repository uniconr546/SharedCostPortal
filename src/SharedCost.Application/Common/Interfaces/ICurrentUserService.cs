namespace SharedCost.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string MaNguoiDung { get; }
    string TenDangNhap { get; }
    string MaRole { get; }
    string IpAddress { get; }
    bool IsAuthenticated { get; }
}
