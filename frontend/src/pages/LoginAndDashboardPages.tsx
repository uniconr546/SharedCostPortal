import React, { useEffect, useState } from 'react';
import { ShieldCheck, Lock, AlertTriangle, HelpCircle, ArrowRight } from 'lucide-react';
import { apiClient, extractErrorMessage, formatVND, translateRole } from '../api';
import { StatusBadge } from '../components/Layout';
import { useAuth } from '../context/AuthContext';
import { AllocationRunDto, DashboardSummaryDto } from '../types';

export const LoginPage: React.FC = () => {
  const { demoAccounts, loginWithOidc } = useAuth();
  const [accountInput, setAccountInput] = useState<string>('');
  const [error, setError] = useState<string>('');
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [showDemoPanel, setShowDemoPanel] = useState<boolean>(false);
  const [showSupportInfo, setShowSupportInfo] = useState<boolean>(false);
  const isDevMode = import.meta.env.DEV;

  const handleEnterpriseLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    const trimmed = accountInput.trim();
    if (!trimmed) {
      setError('Vui lòng nhập Tên đăng nhập hoặc Email công vụ của bạn.');
      return;
    }

    setSubmitting(true);
    try {
      const normalized = trimmed.includes('@') ? trimmed.split('@')[0] : trimmed;
      await loginWithOidc(normalized);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  };

  const handleDemoQuickLogin = async (tenDangNhap: string) => {
    setError('');
    setSubmitting(true);
    try {
      await loginWithOidc(tenDangNhap);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="login-backdrop">
      <div className="login-center-wrap">
        <div className="login-card">
          <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 16 }}>
            <div className="brand-logo" style={{ width: 44, height: 44, fontSize: 18 }}>SC</div>
            <div>
              <h1 style={{ fontSize: 20, fontWeight: 800, color: '#0b192c' }}>SharedCost Portal</h1>
              <div style={{ fontSize: 12.5, color: '#64748b', fontWeight: 500 }}>
                Hệ thống Quản lý và Phân bổ Chi phí Dùng chung
              </div>
            </div>
          </div>

          <p style={{ color: '#4b5563', fontSize: 13.5, marginBottom: 22, lineHeight: 1.5 }}>
            Nền tảng quản trị tập trung các khoản chi phí dùng chung, thiết lập tiêu thức phân bổ, mô phỏng phương án và kiểm soát khóa sổ kế toán theo kỳ.
          </p>

          {error && <div className="alert alert-error">{error}</div>}

          <form onSubmit={handleEnterpriseLogin}>
            <div className="form-group">
              <label>Tài khoản tổ chức (Tên đăng nhập hoặc Email công vụ)</label>
              <input
                className="input"
                placeholder="Ví dụ: finance@sharedcost.local hoặc finance"
                value={accountInput}
                onChange={(e) => setAccountInput(e.target.value)}
                autoFocus
              />
            </div>

            <button
              type="submit"
              className="btn btn-primary"
              style={{ width: '100%', padding: '11px 16px', fontSize: 14, marginTop: 6 }}
              disabled={submitting}
            >
              <Lock size={16} />
              <span>{submitting ? 'Đang xác thực tài khoản...' : 'Đăng nhập bằng tài khoản tổ chức'}</span>
              <ArrowRight size={16} />
            </button>
          </form>

          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              marginTop: 18,
              paddingTop: 14,
              borderTop: '1px solid #e5e7eb',
              fontSize: 12.5,
              color: '#4b5563'
            }}
          >
            <span style={{ display: 'inline-flex', alignItems: 'center', gap: 6, color: '#15803d', fontWeight: 600 }}>
              <ShieldCheck size={16} />
              Kết nối xác thực bảo mật
            </span>

            <button
              type="button"
              onClick={() => setShowSupportInfo(!showSupportInfo)}
              style={{
                background: 'none',
                border: 'none',
                color: '#005fb8',
                cursor: 'pointer',
                fontWeight: 600,
                display: 'inline-flex',
                alignItems: 'center',
                gap: 4
              }}
            >
              <HelpCircle size={14} />
              Hỗ trợ đăng nhập
            </button>
          </div>

          {showSupportInfo && (
            <div style={{ marginTop: 12, padding: 12, background: '#f8fafc', borderRadius: 6, fontSize: 12, color: '#475569' }}>
              Nếu bạn chưa được cấp quyền truy cập hoặc tài khoản bị tạm khóa, vui lòng liên hệ Bộ phận Quản trị Hệ thống Tài chính & CNTT nội bộ để được hỗ trợ kích hoạt tài khoản.
            </div>
          )}

          {isDevMode && (
            <div style={{ marginTop: 14, paddingTop: 12, borderTop: '1px dashed #cbd5e1' }}>
              <button
                type="button"
                onClick={() => setShowDemoPanel(!showDemoPanel)}
                style={{
                  background: 'none',
                  border: 'none',
                  color: '#64748b',
                  fontSize: 12,
                  cursor: 'pointer',
                  fontWeight: 600
                }}
              >
                {showDemoPanel ? '▼ Ẩn chế độ Demo kiểm thử (Development)' : '▶ Chế độ Demo dành cho giảng viên kiểm thử'}
              </button>

              {showDemoPanel && (
                <div style={{ marginTop: 10, display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 6 }}>
                  {demoAccounts.map((acc) => (
                    <button
                      key={acc.maNguoiDung}
                      type="button"
                      className="btn btn-secondary btn-sm"
                      style={{ justifyContent: 'flex-start', textAlign: 'left' }}
                      onClick={() => handleDemoQuickLogin(acc.tenDangNhap)}
                    >
                      <div>
                        <div style={{ fontWeight: 700, fontSize: 11.5 }}>{acc.hoTen}</div>
                        <div style={{ fontSize: 10.5, color: '#64748b' }}>{translateRole(acc.maRole)}</div>
                      </div>
                    </button>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>
      </div>

      <footer className="login-footer">
        © 2026 SharedCost Portal – Hệ thống Quản lý và Phân bổ Chi phí Dùng chung. Bảo lưu mọi quyền.
      </footer>
    </div>
  );
};

export const DashboardAndReportPage: React.FC = () => {
  const [kyPhanBo, setKyPhanBo] = useState<string>('');
  const [summary, setSummary] = useState<DashboardSummaryDto | null>(null);
  const [recentRuns, setRecentRuns] = useState<AllocationRunDto[]>([]);
  const [error, setError] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);

  const loadData = async (period?: string) => {
    setLoading(true);
    setError('');
    try {
      const [sumRes, runsRes] = await Promise.all([
        apiClient.get<DashboardSummaryDto>('/reports/dashboard', {
          params: { kyPhanBo: period || undefined }
        }),
        apiClient.get<AllocationRunDto[]>('/allocation-runs', {
          params: { kyPhanBo: period || undefined }
        })
      ]);
      setSummary(sumRes.data);
      setRecentRuns(runsRes.data.slice(0, 6));
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData('');
  }, []);

  const maxTargetAmount = summary
    ? Math.max(...summary.phanBoTheoTarget.map((x) => x.tongTienNhanPhanBo), 1)
    : 1;

  // Tổng hợp theo kỳ từ dữ liệu thật của phanBoTheoCostPool
  const periodGroups = React.useMemo(() => {
    if (!summary) return [];
    const map = new Map<string, { ky: string; nguon: number; daPhanBo: number }>();
    for (const cp of summary.phanBoTheoCostPool) {
      const current = map.get(cp.kyApDung) || { ky: cp.kyApDung, nguon: 0, daPhanBo: 0 };
      current.nguon += cp.soTienNguon;
      current.daPhanBo += cp.tongTienDaPhanBo;
      map.set(cp.kyApDung, current);
    }
    return Array.from(map.values()).sort((a, b) => a.ky.localeCompare(b.ky));
  }, [summary]);

  const maxPeriodAmount = Math.max(...periodGroups.map((p) => Math.max(p.nguon, p.daPhanBo)), 1);

  // Cảnh báo nghiệp vụ từ dữ liệu thật
  const warnings = React.useMemo(() => {
    if (!summary) return [];
    const list: string[] = [];
    if (summary.soRunChoDuyet > 0) {
      list.push(`Có ${summary.soRunChoDuyet} phiên phân bổ đã tính toán đang chờ kiểm duyệt và phê duyệt.`);
    }
    const unallocatedPools = summary.phanBoTheoCostPool.filter((c) => c.trangThaiRunMoiNhat === 'CHƯA CHẠY');
    if (unallocatedPools.length > 0) {
      list.push(`Có ${unallocatedPools.length} khoản chi phí (${unallocatedPools.map((x) => x.maCostPool).join(', ')}) chưa thực hiện chạy phiên phân bổ chính thức.`);
    }
    const rejectedRuns = recentRuns.filter((r) => r.trangThai === 'REJECTED');
    if (rejectedRuns.length > 0) {
      list.push(`Có ${rejectedRuns.length} phiên phân bổ bị từ chối cần kiểm tra lại số liệu và tạo phiên chạy mới.`);
    }
    return list;
  }, [summary, recentRuns]);

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Tổng quan và báo cáo</h1>
          <div className="page-subtitle">Theo dõi toàn diện nguồn chi phí dùng chung, tiến độ phân bổ và kết quả nhận chi phí theo từng đơn vị</div>
        </div>

        <div className="toolbar">
          <input
            className="input"
            placeholder="Kỳ kế toán (VD: 2026-10)"
            value={kyPhanBo}
            onChange={(e) => setKyPhanBo(e.target.value)}
            style={{ width: 200 }}
          />
          <button className="btn btn-primary" onClick={() => loadData(kyPhanBo)} disabled={loading}>
            Lọc dữ liệu
          </button>
          <button
            className="btn btn-secondary"
            onClick={() => {
              setKyPhanBo('');
              loadData('');
            }}
          >
            Tất cả các kỳ
          </button>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      {warnings.length > 0 && (
        <div className="alert alert-warning" style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontWeight: 700 }}>
            <AlertTriangle size={16} />
            <span>Cảnh báo nghiệp vụ cần lưu ý</span>
          </div>
          {warnings.map((w, idx) => (
            <div key={idx} style={{ paddingLeft: 24, fontSize: 13 }}>• {w}</div>
          ))}
        </div>
      )}

      {summary && (
        <>
          <div className="kpi-grid-6">
            <div className="kpi-card">
              <div className="kpi-label">Tổng số khoản chi phí</div>
              <div className="kpi-value">{summary.tongSoCostPool}</div>
              <div className="kpi-sub">Khoản chi phí đã khai báo</div>
            </div>
            <div className="kpi-card">
              <div className="kpi-label">Tổng nguồn chi phí</div>
              <div className="kpi-value" style={{ color: '#005fb8', fontSize: 17 }}>{formatVND(summary.tongGiaTriCostPool)}</div>
              <div className="kpi-sub">Tổng giá trị nguồn cần phân bổ</div>
            </div>
            <div className="kpi-card">
              <div className="kpi-label">Đối tượng nhận chi phí</div>
              <div className="kpi-value">{summary.phanBoTheoTarget.length}</div>
              <div className="kpi-sub">Đơn vị / Phòng ban thụ hưởng</div>
            </div>
            <div className="kpi-card">
              <div className="kpi-label">Số phiên phân bổ</div>
              <div className="kpi-value">{summary.tongSoAllocationRun}</div>
              <div className="kpi-sub">Phiên phân bổ đã khởi tạo</div>
            </div>
            <div className="kpi-card" style={{ borderTopColor: '#d97706' }}>
              <div className="kpi-label">Phiên chờ kiểm duyệt</div>
              <div className="kpi-value" style={{ color: '#d97706' }}>{summary.soRunChoDuyet}</div>
              <div className="kpi-sub">Đang chờ rà soát & phê duyệt</div>
            </div>
            <div className="kpi-card" style={{ borderTopColor: '#15803d' }}>
              <div className="kpi-label">Số phiên đã khóa</div>
              <div className="kpi-value" style={{ color: '#15803d' }}>{summary.soRunDaKhoa}</div>
              <div className="kpi-sub">Đã chốt sổ kế toán</div>
            </div>
          </div>

          <div className="grid-2">
            <div className="card">
              <div className="card-title">Biểu đồ tổng hợp chi phí theo đối tượng nhận</div>
              {summary.phanBoTheoTarget.map((item) => {
                const widthPercent = Math.min(100, Math.round((item.tongTienNhanPhanBo / maxTargetAmount) * 100));
                return (
                  <div key={item.maTarget} className="chart-bar-row">
                    <div className="chart-bar-header">
                      <span>
                        <strong>{item.tenTarget}</strong> ({item.maTarget})
                      </span>
                      <strong style={{ color: '#005fb8' }}>{formatVND(item.tongTienNhanPhanBo)}</strong>
                    </div>
                    <div className="chart-bar-track">
                      <div className="chart-bar-fill" style={{ width: `${widthPercent}%` }} />
                    </div>
                  </div>
                );
              })}
            </div>

            <div className="card">
              <div className="card-title">Biểu đồ phân bổ chi phí theo kỳ kế toán</div>
              {periodGroups.map((pg) => {
                const nguonPct = Math.min(100, Math.round((pg.nguon / maxPeriodAmount) * 100));
                const phanBoPct = Math.min(100, Math.round((pg.daPhanBo / maxPeriodAmount) * 100));
                return (
                  <div key={pg.ky} style={{ marginBottom: 16 }}>
                    <div className="chart-bar-header">
                      <strong>Kỳ kế toán {pg.ky}</strong>
                      <span style={{ fontSize: 12, color: '#475569' }}>
                        Nguồn: <strong>{formatVND(pg.nguon)}</strong> | Đã phân bổ: <strong style={{ color: '#15803d' }}>{formatVND(pg.daPhanBo)}</strong>
                      </span>
                    </div>
                    <div className="chart-bar-track" style={{ marginBottom: 5 }}>
                      <div className="chart-bar-fill" style={{ width: `${nguonPct}%`, background: '#64748b' }} />
                    </div>
                    <div className="chart-bar-track">
                      <div className="chart-bar-fill" style={{ width: `${phanBoPct}%`, background: '#16a34a' }} />
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          <div className="grid-2">
            <div className="card">
              <div className="card-title">Tình hình thực hiện theo khoản chi phí</div>
              <div className="table-responsive">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>Mã chi phí</th>
                      <th>Tên khoản chi phí</th>
                      <th>Kỳ áp dụng</th>
                      <th>Số tiền nguồn</th>
                      <th>Tổng đã phân bổ</th>
                      <th>Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    {summary.phanBoTheoCostPool.map((cp) => (
                      <tr key={cp.maCostPool}>
                        <td><strong>{cp.maCostPool}</strong></td>
                        <td>{cp.tenCostPool}</td>
                        <td>{cp.kyApDung}</td>
                        <td>{formatVND(cp.soTienNguon)}</td>
                        <td>{formatVND(cp.tongTienDaPhanBo)}</td>
                        <td><StatusBadge status={cp.trangThaiRunMoiNhat} /></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="card">
              <div className="card-title">Các phiên phân bổ thực hiện gần đây</div>
              <div className="table-responsive">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>Mã phiên chạy</th>
                      <th>Khoản chi phí</th>
                      <th>Kỳ</th>
                      <th>Số tiền phân bổ</th>
                      <th>Người thực hiện</th>
                      <th>Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    {recentRuns.map((r) => (
                      <tr key={r.maRun}>
                        <td><strong>{r.maRun}</strong></td>
                        <td>{r.tenCostPool}</td>
                        <td>{r.kyPhanBo}</td>
                        <td style={{ fontWeight: 700, color: '#005fb8' }}>{formatVND(r.tongTienPhanBo)}</td>
                        <td>{r.nguoiThucHien}</td>
                        <td><StatusBadge status={r.trangThai} /></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
};
