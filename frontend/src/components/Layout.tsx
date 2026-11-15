import React, { useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import {
  BarChart3,
  Building2,
  Calculator,
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  FileSearch,
  FlaskConical,
  History,
  Layers,
  LogOut,
  Menu,
  Sliders,
  Users,
  Wallet
} from 'lucide-react';
import { translateRole, translateStatus } from '../api';
import { useAuth } from '../context/AuthContext';

export const StatusBadge: React.FC<{ status: string }> = ({ status }) => {
  const s = (status || '').toUpperCase();
  let cls = 'badge-draft';
  if (['ACTIVE', 'APPROVED', 'REVIEWED', 'COMPLETED'].includes(s)) cls = 'badge-active';
  else if (s === 'LOCKED') cls = 'badge-locked';
  else if (['PENDING_REVIEW', 'CALCULATED', 'RUNNING'].includes(s)) cls = 'badge-pending';
  else if (['REJECTED', 'INACTIVE', 'EXPIRED', 'FAILED'].includes(s)) cls = 'badge-rejected';

  return <span className={`badge ${cls}`}>{translateStatus(status)}</span>;
};

const WORKFLOW_STEPS = [
  { step: 1, label: 'Nhập dữ liệu', path: '/cost-pools' },
  { step: 2, label: 'Thiết lập quy tắc', path: '/rules' },
  { step: 3, label: 'Mô phỏng', path: '/simulations' },
  { step: 4, label: 'Tạo phiên phân bổ', path: '/allocation-runs' },
  { step: 5, label: 'Kiểm duyệt & Khóa sổ', path: '/reviews-locks' },
  { step: 6, label: 'Tra cứu nguồn gốc', path: '/traceability' }
];

export const Layout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user, demoAccounts, loginWithOidc, logout, canViewAudit, isAdmin } = useAuth();
  const [collapsed, setCollapsed] = useState<boolean>(false);
  const [mobileOpen, setMobileOpen] = useState<boolean>(false);
  const [showDevSwitcher, setShowDevSwitcher] = useState<boolean>(false);
  const location = useLocation();
  const isDevMode = import.meta.env.DEV;

  const handleDevRoleSwitch = async (e: React.ChangeEvent<HTMLSelectElement>) => {
    const selectedUsername = e.target.value;
    if (!selectedUsername) return;
    const acc = demoAccounts.find((a) => a.tenDangNhap === selectedUsername);
    if (acc) {
      await loginWithOidc(acc.tenDangNhap, acc.oidcIssuer, acc.oidcSubject);
    }
  };

  return (
    <div className="app-shell">
      <aside className={`sidebar ${collapsed ? 'collapsed' : ''} ${mobileOpen ? 'mobile-open' : ''}`}>
        <div className="sidebar-header">
          <div className="brand-box">
            <div className="brand-logo">SC</div>
            <div className="brand-text">
              <div className="brand-title">SharedCost Portal</div>
              <div className="brand-subtitle">Quản trị Chi phí Doanh nghiệp</div>
            </div>
          </div>
          <button
            className="sidebar-toggle-btn"
            onClick={() => setCollapsed(!collapsed)}
            title={collapsed ? 'Mở rộng thanh điều hướng' : 'Thu gọn thanh điều hướng'}
          >
            {collapsed ? <ChevronRight size={16} /> : <ChevronLeft size={16} />}
          </button>
        </div>

        <nav className="sidebar-nav" onClick={() => setMobileOpen(false)}>
          <div className="nav-section-label">1. Tổng quan và báo cáo</div>
          <NavLink to="/" end className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <BarChart3 size={18} />
            <span className="nav-link-text">Tổng quan và báo cáo</span>
          </NavLink>

          <div className="nav-section-label">2. Quản lý dữ liệu đầu vào</div>
          <NavLink to="/cost-pools" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <Wallet size={18} />
            <span className="nav-link-text">Quản lý khoản chi phí</span>
          </NavLink>
          <NavLink to="/drivers" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <Sliders size={18} />
            <span className="nav-link-text">Quản lý tiêu thức phân bổ</span>
          </NavLink>
          <NavLink to="/targets" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <Building2 size={18} />
            <span className="nav-link-text">Quản lý đối tượng nhận chi phí</span>
          </NavLink>

          <div className="nav-section-label">3. Quản lý quy tắc và phiên bản</div>
          <NavLink to="/rules" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <Layers size={18} />
            <span className="nav-link-text">Quy tắc & Phiên bản phân bổ</span>
          </NavLink>

          <div className="nav-section-label">4. Mô phỏng và phân bổ chi phí</div>
          <NavLink to="/simulations" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <FlaskConical size={18} />
            <span className="nav-link-text">Mô phỏng phân bổ</span>
          </NavLink>
          <NavLink to="/allocation-runs" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <Calculator size={18} />
            <span className="nav-link-text">Phiên phân bổ chính thức</span>
          </NavLink>

          <div className="nav-section-label">5. Kiểm duyệt và khóa kết quả</div>
          <NavLink to="/reviews-locks" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <CheckCircle2 size={18} />
            <span className="nav-link-text">Kiểm duyệt và khóa kết quả</span>
          </NavLink>

          <div className="nav-section-label">6. Tra cứu và kiểm toán</div>
          <NavLink to="/traceability" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <FileSearch size={18} />
            <span className="nav-link-text">Tra cứu nguồn gốc dữ liệu</span>
          </NavLink>
          {canViewAudit && (
            <NavLink to="/audit-logs" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
              <History size={18} />
              <span className="nav-link-text">Nhật ký hệ thống</span>
            </NavLink>
          )}

          {(isAdmin || user?.maRole === 'AUDITOR') && (
            <>
              <div className="nav-section-label">7. Quản trị người dùng</div>
              <NavLink to="/users" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
                <Users size={18} />
                <span className="nav-link-text">Quản lý người dùng và quyền hạn</span>
              </NavLink>
            </>
          )}
        </nav>

        <div className="sidebar-footer">
          <span className="sidebar-footer-text">© 2026 SharedCost Portal</span>
        </div>
      </aside>

      <div className="main-area">
        <header className="topbar">
          <div className="topbar-left">
            <button className="mobile-menu-btn" onClick={() => setMobileOpen(!mobileOpen)}>
              <Menu size={18} />
            </button>
            <span className="system-title">Hệ thống Quản lý và Phân bổ Chi phí Dùng chung</span>
          </div>

          <div className="topbar-right">
            {isDevMode && (
              <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                <button
                  type="button"
                  className="btn btn-secondary btn-sm"
                  onClick={() => setShowDevSwitcher(!showDevSwitcher)}
                  title="Chế độ hỗ trợ giảng viên kiểm thử nhanh các vai trò trong môi trường Development"
                >
                  Chế độ Demo
                </button>
                {showDevSwitcher && (
                  <select
                    className="select"
                    value={user?.tenDangNhap || ''}
                    onChange={handleDevRoleSwitch}
                    style={{ width: 210, padding: '5px 8px', fontSize: 12 }}
                  >
                    {demoAccounts.map((acc) => (
                      <option key={acc.maNguoiDung} value={acc.tenDangNhap}>
                        {acc.hoTen} ({translateRole(acc.maRole)})
                      </option>
                    ))}
                  </select>
                )}
              </div>
            )}

            {user && (
              <>
                <div className="user-profile-pill">
                  <div className="user-avatar">{user.hoTen.charAt(0).toUpperCase()}</div>
                  <div className="user-meta">
                    <span className="user-name">{user.hoTen}</span>
                    <span className="user-role">{translateRole(user.maRole)}</span>
                  </div>
                </div>
                <button className="btn btn-secondary btn-sm" onClick={logout} title="Đăng xuất khỏi hệ thống">
                  <LogOut size={15} />
                  <span>Đăng xuất</span>
                </button>
              </>
            )}
          </div>
        </header>

        <div className="workflow-stepper">
          <span style={{ fontSize: 12, fontWeight: 700, color: '#475569', marginRight: 4 }}>Quy trình nghiệp vụ:</span>
          {WORKFLOW_STEPS.map((s, idx) => (
            <React.Fragment key={s.path}>
              <NavLink to={s.path} className={`workflow-step ${location.pathname === s.path ? 'active' : ''}`}>
                <span className="workflow-step-num">{s.step}</span>
                <span>{s.label}</span>
              </NavLink>
              {idx < WORKFLOW_STEPS.length - 1 && <span className="workflow-arrow">→</span>}
            </React.Fragment>
          ))}
        </div>

        <main className="content-wrapper">{children}</main>
      </div>
    </div>
  );
};
