import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080/api';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('oidc_access_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

function cleanTechnicalPrefix(rawMessage: string): string {
  return rawMessage
    .replace(/^\[[A-Z0-9_]+\]\s*/g, '')
    .replace(/^Lỗi\s+BR[0-9/]+:\s*/gi, '')
    .replace(/\s*\(BR[0-9/]+\)/gi, '')
    .trim();
}

export function extractErrorMessage(err: unknown): string {
  if (axios.isAxiosError(err)) {
    if (!err.response) {
      return 'Không thể kết nối đến máy chủ dịch vụ (http://localhost:5080). Vui lòng kiểm tra cửa sổ chạy Backend API.';
    }
    const data = err.response.data as {
      message?: string;
      title?: string;
      errors?: Record<string, string[]>;
    } | undefined;

    if (data?.message) {
      return cleanTechnicalPrefix(data.message);
    }
    if (data?.errors) {
      const firstErr = Object.values(data.errors).flat()[0];
      if (firstErr) return cleanTechnicalPrefix(firstErr);
    }
    if (data?.title) {
      return cleanTechnicalPrefix(data.title);
    }
    if (err.response.status === 401) {
      return 'Phiên làm việc đã hết hạn hoặc chưa xác thực. Vui lòng đăng nhập lại.';
    }
    if (err.response.status === 403) {
      return 'Tài khoản của bạn không có quyền thực hiện thao tác này.';
    }
    return `Máy chủ trả về mã lỗi HTTP ${err.response.status}.`;
  }
  return err instanceof Error ? cleanTechnicalPrefix(err.message) : 'Đã xảy ra lỗi trong quá trình xử lý.';
}

export const formatVND = (amount: number): string =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount || 0);

export const formatNumber = (val: number, decimals = 4): string =>
  new Intl.NumberFormat('vi-VN', { minimumFractionDigits: 0, maximumFractionDigits: decimals }).format(val || 0);

export const formatPercent = (ratio: number): string =>
  `${((ratio || 0) * 100).toFixed(2)}%`;

export const translateStatus = (status: string): string => {
  const map: Record<string, string> = {
    ACTIVE: 'Đang áp dụng',
    DRAFT: 'Bản nháp',
    INACTIVE: 'Ngừng sử dụng',
    EXPIRED: 'Hết hiệu lực',
    CALCULATED: 'Đã tính toán',
    PENDING_REVIEW: 'Chờ kiểm duyệt',
    REVIEWED: 'Đã phê duyệt',
    REJECTED: 'Từ chối',
    LOCKED: 'Đã khóa sổ',
    APPROVED: 'Chấp thuận',
    COMPLETED: 'Hoàn tất',
    RUNNING: 'Đang xử lý',
    FAILED: 'Thất bại'
  };
  return map[(status || '').toUpperCase()] || status;
};

export const translateRole = (role: string): string => {
  const map: Record<string, string> = {
    ADMIN: 'Quản trị hệ thống',
    FINANCE: 'Chuyên viên tài chính',
    REVIEWER: 'Người kiểm duyệt',
    VIEWER: 'Người xem báo cáo',
    AUDITOR: 'Kiểm toán nội bộ'
  };
  return map[(role || '').toUpperCase()] || role;
};
