import React, { useEffect, useState } from 'react';
import { apiClient, extractErrorMessage, formatNumber, formatPercent, formatVND, translateRole } from '../api';
import { StatusBadge } from '../components/Layout';
import { useAuth } from '../context/AuthContext';
import {
  AllocationRuleDto,
  AllocationRunDto,
  AuditLogDto,
  CostPoolDto,
  DriverDto,
  NguoiDungDto,
  RoleDto,
  RuleVersionDto,
  SimulationDto,
  TargetDto,
  TraceabilityDto
} from '../types';
export const RuleAndVersionPage: React.FC = () => {
  const { canManageCatalog } = useAuth();
  const [rules, setRules] = useState<AllocationRuleDto[]>([]);
  const [versions, setVersions] = useState<RuleVersionDto[]>([]);
  const [costPools, setCostPools] = useState<CostPoolDto[]>([]);
  const [drivers, setDrivers] = useState<DriverDto[]>([]);
  const [targets, setTargets] = useState<TargetDto[]>([]);
  const [error, setError] = useState<string>('');
  const [message, setMessage] = useState<string>('');

  const [ruleForm, setRuleForm] = useState({
    maRule: '',
    tenRule: '',
    maCostPool: '',
    maDriver: '',
    congThuc: 'Số tiền phân bổ = Tổng chi phí nguồn × (Giá trị tiêu thức đơn vị / Tổng giá trị tiêu thức)',
    thuTuThucHien: 1,
    trangThai: 'ACTIVE',
    moTa: '',
    ngayHieuLucVersionDau: '2026-01-01',
    targets: [] as { maTarget: string; thuTuTarget: number; tyLeMacDinh?: number }[]
  });

  const [verForm, setVerForm] = useState({
    maRuleVersion: '',
    maRule: '',
    maCostPool: '',
    maDriver: '',
    ngayHieuLuc: '2026-01-01',
    ngayHetHieuLuc: '2026-12-31',
    congThuc: 'Số tiền phân bổ = Tổng chi phí nguồn × (Giá trị tiêu thức đơn vị / Tổng giá trị tiêu thức)',
    thuTuThucHien: 1,
    thamSoJson: '{"roundingScale": 2, "ratioScale": 6}',
    kichHoatNgay: true,
    targets: [] as { maTarget: string; thuTuTarget: number; tyLeMacDinh?: number }[]
  });

  const loadAll = async () => {
    try {
      const [rRes, vRes, cpRes, dRes, tRes] = await Promise.all([
        apiClient.get<AllocationRuleDto[]>('/allocation-rules'),
        apiClient.get<RuleVersionDto[]>('/rule-versions'),
        apiClient.get<CostPoolDto[]>('/cost-pools'),
        apiClient.get<DriverDto[]>('/drivers'),
        apiClient.get<TargetDto[]>('/targets')
      ]);
      setRules(rRes.data);
      setVersions(vRes.data);
      setCostPools(cpRes.data);
      setDrivers(dRes.data);
      setTargets(tRes.data);

      const defaultTargets = tRes.data.map((t, idx) => ({
        maTarget: t.maTarget,
        thuTuTarget: idx + 1,
        tyLeMacDinh: Number((1 / Math.max(tRes.data.length, 1)).toFixed(6))
      }));

      if (tRes.data.length > 0 && ruleForm.targets.length === 0) {
        setRuleForm((prev) => ({
          ...prev,
          maCostPool: cpRes.data[0]?.maCostPool || '',
          maDriver: dRes.data[0]?.maDriver || '',
          targets: defaultTargets
        }));
      }

      if (rRes.data.length > 0 && !verForm.maRule) {
        const firstRule = rRes.data[0];
        setVerForm((prev) => ({
          ...prev,
          maRule: firstRule.maRule,
          maCostPool: firstRule.maCostPool,
          maDriver: firstRule.maDriver,
          congThuc: firstRule.congThuc,
          thuTuThucHien: firstRule.thuTuThucHien,
          targets: firstRule.targets.map((x) => ({
            maTarget: x.maTarget,
            thuTuTarget: x.thuTuTarget,
            tyLeMacDinh: x.tyLeMacDinh ?? undefined
          }))
        }));
      }
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  useEffect(() => {
    loadAll();
  }, []);

  const handleSaveRule = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    try {
      await apiClient.post('/allocation-rules', ruleForm);
      setMessage(`Đã lưu quy tắc phân bổ "${ruleForm.tenRule}" thành công.`);
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  const handleCreateVersion = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    try {
      const res = await apiClient.post<RuleVersionDto>('/rule-versions', {
        ...verForm,
        ngayHetHieuLuc: verForm.ngayHetHieuLuc ? verForm.ngayHetHieuLuc : null
      });
      setMessage(`Đã phát hành phiên bản quy tắc mới ${res.data.maRuleVersion} (Phiên bản ${res.data.soPhienBan}) thành công.`);
      setVerForm({ ...verForm, maRuleVersion: '' });
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  const handleActivateVersion = async (maRuleVersion: string) => {
    if (!window.confirm(`Xác nhận kích hoạt phiên bản quy tắc ${maRuleVersion} để áp dụng cho các kỳ phân bổ?`)) return;
    setError('');
    setMessage('');
    try {
      await apiClient.patch(`/rule-versions/${maRuleVersion}/status`, null, {
        params: { trangThaiMoi: 'ACTIVE' }
      });
      setMessage(`Đã chuyển phiên bản ${maRuleVersion} sang trạng thái Đang áp dụng.`);
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Quản lý quy tắc và phiên bản phân bổ</h1>
          <div className="page-subtitle">Thiết lập quy tắc liên kết khoản chi phí, tiêu thức phân bổ và quản lý lịch sử phiên bản theo kỳ hiệu lực</div>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {message && <div className="alert alert-success">{message}</div>}

      {canManageCatalog && (
        <div className="grid-2">
          <div className="card">
            <div className="card-title">Thiết lập quy tắc phân bổ</div>
            <form onSubmit={handleSaveRule}>
              <div className="grid-2">
                <div className="form-group">
                  <label>Mã quy tắc (*)</label>
                  <input
                    className="input"
                    placeholder="VD: RULE_THUEVP"
                    value={ruleForm.maRule}
                    onChange={(e) => setRuleForm({ ...ruleForm, maRule: e.target.value })}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Tên quy tắc phân bổ (*)</label>
                  <input
                    className="input"
                    placeholder="VD: Phân bổ chi phí thuê văn phòng"
                    value={ruleForm.tenRule}
                    onChange={(e) => setRuleForm({ ...ruleForm, tenRule: e.target.value })}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Khoản chi phí mặc định (*)</label>
                  <select className="select" value={ruleForm.maCostPool} onChange={(e) => setRuleForm({ ...ruleForm, maCostPool: e.target.value })}>
                    {costPools.map((cp) => (
                      <option key={cp.maCostPool} value={cp.maCostPool}>
                        {cp.tenCostPool} ({cp.kyApDung})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Tiêu thức phân bổ mặc định (*)</label>
                  <select className="select" value={ruleForm.maDriver} onChange={(e) => setRuleForm({ ...ruleForm, maDriver: e.target.value })}>
                    {drivers.map((d) => (
                      <option key={d.maDriver} value={d.maDriver}>
                        {d.tenDriver} ({d.donViTinh})
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="form-group">
                <label>Phương pháp / Công thức tính (*)</label>
                <input className="input" value={ruleForm.congThuc} onChange={(e) => setRuleForm({ ...ruleForm, congThuc: e.target.value })} required />
              </div>
              <div className="form-group">
                <label>Đơn vị nhận chi phí thuộc phạm vi quy tắc</label>
                <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap', background: '#f8fafc', padding: 10, borderRadius: 6, border: '1px solid #e2e8f0' }}>
                  {targets.map((t) => {
                    const isSelected = ruleForm.targets.some((x) => x.maTarget === t.maTarget);
                    return (
                      <label key={t.maTarget} style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 12.5 }}>
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={(e) => {
                            if (e.target.checked) {
                              setRuleForm({
                                ...ruleForm,
                                targets: [...ruleForm.targets, { maTarget: t.maTarget, thuTuTarget: ruleForm.targets.length + 1 }]
                              });
                            } else {
                              setRuleForm({
                                ...ruleForm,
                                targets: ruleForm.targets.filter((x) => x.maTarget !== t.maTarget)
                              });
                            }
                          }}
                        />
                        {t.tenTarget}
                      </label>
                    );
                  })}
                </div>
              </div>
              <button type="submit" className="btn btn-primary">Lưu quy tắc phân bổ</button>
            </form>
          </div>

          <div className="card">
            <div className="card-title">Phát hành phiên bản quy tắc mới</div>
            <form onSubmit={handleCreateVersion}>
              <div className="grid-2">
                <div className="form-group">
                  <label>Thuộc quy tắc phân bổ (*)</label>
                  <select
                    className="select"
                    value={verForm.maRule}
                    onChange={(e) => {
                      const r = rules.find((x) => x.maRule === e.target.value);
                      setVerForm({
                        ...verForm,
                        maRule: e.target.value,
                        maCostPool: r?.maCostPool || verForm.maCostPool,
                        maDriver: r?.maDriver || verForm.maDriver,
                        congThuc: r?.congThuc || verForm.congThuc,
                        targets: r?.targets.map((t) => ({ maTarget: t.maTarget, thuTuTarget: t.thuTuTarget })) || verForm.targets
                      });
                    }}
                  >
                    {rules.map((r) => (
                      <option key={r.maRule} value={r.maRule}>{r.tenRule} ({r.maRule})</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Mã phiên bản (Bỏ trống để tự động cấp)</label>
                  <input
                    className="input"
                    placeholder="VD: RV_DIEN_V2"
                    value={verForm.maRuleVersion}
                    onChange={(e) => setVerForm({ ...verForm, maRuleVersion: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label>Khoản chi phí áp dụng (*)</label>
                  <select className="select" value={verForm.maCostPool} onChange={(e) => setVerForm({ ...verForm, maCostPool: e.target.value })}>
                    {costPools.map((cp) => (
                      <option key={cp.maCostPool} value={cp.maCostPool}>{cp.tenCostPool} ({cp.kyApDung})</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Tiêu thức áp dụng (*)</label>
                  <select className="select" value={verForm.maDriver} onChange={(e) => setVerForm({ ...verForm, maDriver: e.target.value })}>
                    {drivers.map((d) => (
                      <option key={d.maDriver} value={d.maDriver}>{d.tenDriver}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Ngày bắt đầu hiệu lực (*)</label>
                  <input type="date" className="input" value={verForm.ngayHieuLuc} onChange={(e) => setVerForm({ ...verForm, ngayHieuLuc: e.target.value })} required />
                </div>
                <div className="form-group">
                  <label>Ngày kết thúc hiệu lực</label>
                  <input type="date" className="input" value={verForm.ngayHetHieuLuc} onChange={(e) => setVerForm({ ...verForm, ngayHetHieuLuc: e.target.value })} />
                </div>
              </div>
              <div className="form-group">
                <label style={{ display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer' }}>
                  <input
                    type="checkbox"
                    checked={verForm.kichHoatNgay}
                    onChange={(e) => setVerForm({ ...verForm, kichHoatNgay: e.target.checked })}
                  />
                  Áp dụng ngay phiên bản này làm phiên bản hiện hành của quy tắc
                </label>
              </div>
              <button type="submit" className="btn btn-success">Phát hành phiên bản mới</button>
            </form>
          </div>
        </div>
      )}

      <div className="card">
        <div className="card-title">Danh mục quy tắc phân bổ</div>
        <div className="table-responsive">
          <table className="data-table">
            <thead>
              <tr>
                <th>Mã quy tắc</th>
                <th>Tên quy tắc phân bổ</th>
                <th>Khoản chi phí mặc định</th>
                <th>Tiêu thức phân bổ</th>
                <th>Đơn vị nhận chi phí</th>
                <th>Trạng thái</th>
              </tr>
            </thead>
            <tbody>
              {rules.map((r) => (
                <tr key={r.maRule}>
                  <td><strong>{r.maRule}</strong></td>
                  <td>{r.tenRule}</td>
                  <td>{r.tenCostPool}</td>
                  <td>{r.tenDriver}</td>
                  <td>
                    {r.targets.map((t) => (
                      <span key={t.maTarget} className="badge badge-draft" style={{ marginRight: 4 }}>
                        {t.tenTarget || t.maTarget}
                      </span>
                    ))}
                  </td>
                  <td><StatusBadge status={r.trangThai} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <div className="card">
        <div className="card-title">Lịch sử phiên bản quy tắc phân bổ</div>
        <div className="table-responsive">
          <table className="data-table">
            <thead>
              <tr>
                <th>Mã phiên bản</th>
                <th>Thuộc quy tắc</th>
                <th>Số phiên bản</th>
                <th>Thời gian hiệu lực</th>
                <th>Khoản chi phí</th>
                <th>Tiêu thức</th>
                <th>Trạng thái</th>
                {canManageCatalog && <th>Thao tác</th>}
              </tr>
            </thead>
            <tbody>
              {versions.map((v) => (
                <tr key={v.maRuleVersion}>
                  <td><strong>{v.maRuleVersion}</strong></td>
                  <td>{v.tenRule}</td>
                  <td><strong>Phiên bản {v.soPhienBan}</strong></td>
                  <td>{v.ngayHieuLuc.substring(0, 10)} → {v.ngayHetHieuLuc ? v.ngayHetHieuLuc.substring(0, 10) : 'Không thời hạn'}</td>
                  <td>{v.tenCostPool}</td>
                  <td>{v.tenDriver}</td>
                  <td><StatusBadge status={v.trangThai} /></td>
                  {canManageCatalog && (
                    <td>
                      {v.trangThai !== 'ACTIVE' && (
                        <button className="btn btn-success btn-sm" onClick={() => handleActivateVersion(v.maRuleVersion)}>
                          Áp dụng phiên bản
                        </button>
                      )}
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};
export const SimulationPage: React.FC = () => {
  const { canManageCatalog } = useAuth();
  const [simulations, setSimulations] = useState<SimulationDto[]>([]);
  const [ruleVersions, setRuleVersions] = useState<RuleVersionDto[]>([]);
  const [selectedVersion, setSelectedVersion] = useState<string>('');
  const [kyPhanBo, setKyPhanBo] = useState<string>('2026-10');
  const [ghiChu, setGhiChu] = useState<string>('');
  const [activeSim, setActiveSim] = useState<SimulationDto | null>(null);
  const [error, setError] = useState<string>('');
  const [message, setMessage] = useState<string>('');

  const loadData = async () => {
    try {
      const [sRes, rvRes] = await Promise.all([
        apiClient.get<SimulationDto[]>('/simulations'),
        apiClient.get<RuleVersionDto[]>('/rule-versions')
      ]);
      setSimulations(sRes.data);
      setRuleVersions(rvRes.data);
      if (rvRes.data.length > 0 && !selectedVersion) {
        setSelectedVersion(rvRes.data[0].maRuleVersion);
      }
      if (sRes.data.length > 0 && !activeSim) {
        setActiveSim(sRes.data[0]);
      }
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleRunSimulation = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    try {
      const res = await apiClient.post<SimulationDto>('/simulations', {
        maRuleVersion: selectedVersion,
        kyPhanBo,
        ghiChu
      });
      setMessage(`Đã hoàn tất tính toán mô phỏng (${res.data.maSimulation}).`);
      setActiveSim(res.data);
      await loadData();
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Mô phỏng phân bổ</h1>
          <div className="page-subtitle">Tính thử tỷ lệ và số tiền phân bổ dự kiến để kiểm tra phương án trước khi tạo phiên phân bổ chính thức</div>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {message && <div className="alert alert-success">{message}</div>}

      {canManageCatalog && (
        <div className="card">
          <div className="card-title">Khởi tạo kịch bản mô phỏng mới</div>
          <form onSubmit={handleRunSimulation}>
            <div className="grid-3">
              <div className="form-group">
                <label>Phiên bản quy tắc áp dụng (*)</label>
                <select className="select" value={selectedVersion} onChange={(e) => setSelectedVersion(e.target.value)}>
                  {ruleVersions.map((rv) => (
                    <option key={rv.maRuleVersion} value={rv.maRuleVersion}>
                      {rv.tenRule} - Phiên bản {rv.soPhienBan} ({rv.maRuleVersion})
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Kỳ kế toán (YYYY-MM) (*)</label>
                <input className="input" value={kyPhanBo} onChange={(e) => setKyPhanBo(e.target.value)} required />
              </div>
              <div className="form-group">
                <label>Ghi chú phương án mô phỏng</label>
                <input className="input" placeholder="VD: Phương án phân bổ tháng 10/2026..." value={ghiChu} onChange={(e) => setGhiChu(e.target.value)} />
              </div>
            </div>
            <button type="submit" className="btn btn-primary">Thực hiện mô phỏng</button>
          </form>
        </div>
      )}

      <div className="grid-2">
        <div className="card">
          <div className="card-title">Danh sách các lần mô phỏng ({simulations.length})</div>
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Mã mô phỏng</th>
                  <th>Kỳ kế toán</th>
                  <th>Phiên bản quy tắc</th>
                  <th>Tổng tiền nguồn</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {simulations.map((s) => (
                  <tr key={s.maSimulation} style={{ background: activeSim?.maSimulation === s.maSimulation ? '#eff6ff' : undefined }}>
                    <td><strong>{s.maSimulation}</strong></td>
                    <td><strong>{s.kyPhanBo}</strong></td>
                    <td>{s.maRuleVersion}</td>
                    <td style={{ color: '#005fb8', fontWeight: 700 }}>{formatVND(s.tongTienNguon)}</td>
                    <td>
                      <button className="btn btn-secondary btn-sm" onClick={() => setActiveSim(s)}>
                        Xem kết quả
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <div className="card">
          <div className="card-title">
            Kết quả mô phỏng chi tiết: {activeSim ? activeSim.maSimulation : '(Chưa chọn)'}
          </div>
          {activeSim ? (
            <>
              <div style={{ marginBottom: 12, fontSize: 13, color: '#475569' }}>
                Khoản chi phí: <strong>{activeSim.tenCostPool}</strong> | Tiêu thức: <strong>{activeSim.tenDriver}</strong> | Ghi chú: {activeSim.ghiChu || 'Không có'}
              </div>
              <div className="table-responsive">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>Đơn vị nhận chi phí</th>
                      <th>Giá trị tiêu thức</th>
                      <th>Tỷ lệ phân bổ</th>
                      <th>Số tiền dự kiến</th>
                    </tr>
                  </thead>
                  <tbody>
                    {activeSim.results.map((r) => (
                      <tr key={r.maSimulationResult}>
                        <td><strong>{r.tenTarget}</strong></td>
                        <td>{formatNumber(r.giaTriDriver)} / {formatNumber(r.tongDriver)}</td>
                        <td>{formatPercent(r.tyLePhanBo)}</td>
                        <td><strong style={{ color: '#15803d' }}>{formatVND(r.soTienPhanBo)}</strong></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </>
          ) : (
            <p style={{ color: '#64748b' }}>Vui lòng chọn một kịch bản mô phỏng ở bảng bên trái để xem chi tiết.</p>
          )}
        </div>
      </div>
    </div>
  );
};
export const AllocationRunPage: React.FC<{ mode?: 'run' | 'review' }> = ({ mode = 'run' }) => {
  const { canManageCatalog, canReviewAndLock } = useAuth();
  const [runs, setRuns] = useState<AllocationRunDto[]>([]);
  const [ruleVersions, setRuleVersions] = useState<RuleVersionDto[]>([]);
  const [selectedVersion, setSelectedVersion] = useState<string>('');
  const [kyPhanBo, setKyPhanBo] = useState<string>('2026-10');
  const [filterStatus, setFilterStatus] = useState<string>('');
  const [activeRun, setActiveRun] = useState<AllocationRunDto | null>(null);

  const [reviewModal, setReviewModal] = useState<{ open: boolean; maRun: string; trangThai: string; nhanXet: string }>({
    open: false,
    maRun: '',
    trangThai: 'APPROVED',
    nhanXet: ''
  });

  const [lockModal, setLockModal] = useState<{ open: boolean; maRun: string; lyDoKhoa: string }>({
    open: false,
    maRun: '',
    lyDoKhoa: ''
  });

  const [error, setError] = useState<string>('');
  const [message, setMessage] = useState<string>('');

  const loadData = async () => {
    try {
      const [rRes, rvRes] = await Promise.all([
        apiClient.get<AllocationRunDto[]>('/allocation-runs', {
          params: { trangThai: filterStatus || undefined }
        }),
        apiClient.get<RuleVersionDto[]>('/rule-versions', { params: { trangThai: 'ACTIVE' } })
      ]);
      setRuns(rRes.data);
      setRuleVersions(rvRes.data);
      if (rvRes.data.length > 0 && !selectedVersion) {
        setSelectedVersion(rvRes.data[0].maRuleVersion);
      }
      if (rRes.data.length > 0) {
        setActiveRun((prev) => rRes.data.find((x) => x.maRun === prev?.maRun) || rRes.data[0]);
      }
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  useEffect(() => {
    loadData();
  }, [filterStatus]);

  const handleExecuteRun = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!window.confirm(`Xác nhận thực hiện tính toán phiên phân bổ chính thức cho kỳ ${kyPhanBo}?`)) return;
    setError('');
    setMessage('');
    try {
      const res = await apiClient.post<AllocationRunDto>('/allocation-runs', {
        maRuleVersion: selectedVersion,
        kyPhanBo
      });
      setMessage(`Đã tạo và tính toán thành công phiên phân bổ chính thức ${res.data.maRun}.`);
      setActiveRun(res.data);
      await loadData();
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  const handleSubmitReview = async (maRun: string) => {
    if (!window.confirm(`Xác nhận gửi phiên phân bổ ${maRun} sang bộ phận kiểm duyệt?`)) return;
    setError('');
    setMessage('');
    try {
      const res = await apiClient.post<AllocationRunDto>(`/allocation-runs/${maRun}/submit-review`);
      setMessage(`Đã gửi phiên phân bổ ${maRun} sang trạng thái Chờ kiểm duyệt.`);
      setActiveRun(res.data);
      await loadData();
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  const handleReviewAction = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    try {
      const res = await apiClient.post<AllocationRunDto>('/allocation-runs/review', {
        maRun: reviewModal.maRun,
        trangThai: reviewModal.trangThai,
        nhanXet: reviewModal.nhanXet
      });
      setMessage(`Đã ghi nhận quyết định kiểm duyệt cho phiên phân bổ ${reviewModal.maRun}.`);
      setReviewModal({ open: false, maRun: '', trangThai: 'APPROVED', nhanXet: '' });
      setActiveRun(res.data);
      await loadData();
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  const handleLockAction = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    try {
      const res = await apiClient.post<AllocationRunDto>('/allocation-runs/lock', {
        maRun: lockModal.maRun,
        lyDoKhoa: lockModal.lyDoKhoa
      });
      setMessage(`Đã khóa sổ chính thức phiên phân bổ ${lockModal.maRun}.`);
      setLockModal({ open: false, maRun: '', lyDoKhoa: '' });
      setActiveRun(res.data);
      await loadData();
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">
            {mode === 'review' ? 'Kiểm duyệt và khóa kết quả phân bổ' : 'Phiên phân bổ chính thức'}
          </h1>
          <div className="page-subtitle">
            {mode === 'review'
              ? 'Rà soát đối chiếu tổng chi phí nguồn, phê duyệt kết quả và khóa sổ kế toán'
              : 'Khởi tạo phiên tính toán phân bổ chính thức và theo dõi kết quả phân bổ cho từng đơn vị'}
          </div>
        </div>

        <div className="toolbar">
          <select className="select" value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)} style={{ width: 200 }}>
            <option value="">-- Tất cả trạng thái --</option>
            <option value="CALCULATED">Đã tính toán</option>
            <option value="PENDING_REVIEW">Chờ kiểm duyệt</option>
            <option value="REVIEWED">Đã phê duyệt</option>
            <option value="LOCKED">Đã khóa sổ</option>
            <option value="REJECTED">Từ chối</option>
          </select>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {message && <div className="alert alert-success">{message}</div>}

      {mode === 'run' && canManageCatalog && (
        <div className="card">
          <div className="card-title">Khởi tạo phiên phân bổ chính thức mới</div>
          <form onSubmit={handleExecuteRun}>
            <div className="grid-3">
              <div className="form-group">
                <label>Phiên bản quy tắc đang áp dụng (*)</label>
                <select className="select" value={selectedVersion} onChange={(e) => setSelectedVersion(e.target.value)}>
                  {ruleVersions.map((rv) => (
                    <option key={rv.maRuleVersion} value={rv.maRuleVersion}>
                      {rv.tenRule} - Phiên bản {rv.soPhienBan} ({rv.tenCostPool})
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Kỳ phân bổ (YYYY-MM) (*)</label>
                <input className="input" value={kyPhanBo} onChange={(e) => setKyPhanBo(e.target.value)} required />
              </div>
              <div className="form-group" style={{ display: 'flex', alignItems: 'flex-end' }}>
                <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>
                  Chạy phiên phân bổ chính thức
                </button>
              </div>
            </div>
          </form>
        </div>
      )}

      <div className="card">
        <div className="card-title">Danh sách các phiên phân bổ ({runs.length})</div>
        <div className="table-responsive">
          <table className="data-table">
            <thead>
              <tr>
                <th>Mã phiên chạy</th>
                <th>Khoản chi phí</th>
                <th>Quy tắc áp dụng</th>
                <th>Kỳ kế toán</th>
                <th>Tổng tiền nguồn</th>
                <th>Tổng tiền phân bổ</th>
                <th>Trạng thái</th>
                <th>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {runs.map((r) => (
                <tr key={r.maRun} style={{ background: activeRun?.maRun === r.maRun ? '#eff6ff' : undefined }}>
                  <td><strong>{r.maRun}</strong></td>
                  <td>{r.tenCostPool}</td>
                  <td>{r.tenRule} (V{r.soPhienBan})</td>
                  <td><strong>{r.kyPhanBo}</strong></td>
                  <td>{formatVND(r.tongTienNguon)}</td>
                  <td style={{ color: '#15803d', fontWeight: 700 }}>{formatVND(r.tongTienPhanBo)}</td>
                  <td><StatusBadge status={r.trangThai} /></td>
                  <td>
                    <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap' }}>
                      <button className="btn btn-secondary btn-sm" onClick={() => setActiveRun(r)}>
                        Xem kết quả
                      </button>
                      {r.trangThai === 'CALCULATED' && canManageCatalog && (
                        <button className="btn btn-warning btn-sm" onClick={() => handleSubmitReview(r.maRun)}>
                          Gửi kiểm duyệt
                        </button>
                      )}
                      {['CALCULATED', 'PENDING_REVIEW'].includes(r.trangThai) && canReviewAndLock && (
                        <button
                          className="btn btn-primary btn-sm"
                          onClick={() => setReviewModal({ open: true, maRun: r.maRun, trangThai: 'APPROVED', nhanXet: '' })}
                        >
                          Phê duyệt / Từ chối
                        </button>
                      )}
                      {r.trangThai === 'REVIEWED' && canReviewAndLock && (
                        <button
                          className="btn btn-success btn-sm"
                          onClick={() => setLockModal({ open: true, maRun: r.maRun, lyDoKhoa: '' })}
                        >
                          Khóa kết quả
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {activeRun && (
        <div className="card">
          <div className="card-title">
            <span>Bảng kết quả phân bổ chi tiết – Phiên {activeRun.maRun} (<StatusBadge status={activeRun.trangThai} />)</span>
            <span style={{ fontSize: 12.5, color: '#475569' }}>
              Tổng tỷ lệ: <strong>{formatPercent(activeRun.results.reduce((acc, x) => acc + x.tyLePhanBo, 0))}</strong> | Tổng tiền: <strong>{formatVND(activeRun.tongTienPhanBo)}</strong>
            </span>
          </div>
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Mã kết quả</th>
                  <th>Đơn vị nhận chi phí</th>
                  <th>Giá trị tiêu thức</th>
                  <th>Tổng tiêu thức</th>
                  <th>Tỷ lệ phân bổ</th>
                  <th>Số tiền phân bổ</th>
                </tr>
              </thead>
              <tbody>
                {activeRun.results.map((res) => (
                  <tr key={res.maResult}>
                    <td><strong>{res.maResult}</strong></td>
                    <td>{res.tenTarget} ({res.maTarget})</td>
                    <td>{formatNumber(res.giaTriDriver)}</td>
                    <td>{formatNumber(res.tongDriver)}</td>
                    <td>{formatPercent(res.tyLePhanBo)}</td>
                    <td><strong style={{ color: '#15803d' }}>{formatVND(res.soTienPhanBo)}</strong></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="grid-2" style={{ marginTop: 16 }}>
            <div style={{ background: '#f8fafc', padding: 14, borderRadius: 6, border: '1px solid #e2e8f0' }}>
              <h4 style={{ marginBottom: 6, color: '#0b192c' }}>Lịch sử kiểm duyệt</h4>
              {activeRun.reviews.length === 0 ? (
                <p style={{ color: '#64748b', fontSize: 12.5 }}>Phiên phân bổ này chưa có bản ghi kiểm duyệt.</p>
              ) : (
                activeRun.reviews.map((rev) => (
                  <div key={rev.maReview} style={{ fontSize: 12.5, marginBottom: 6 }}>
                    Lần {rev.lanDuyet}: <StatusBadge status={rev.trangThai} /> bởi <strong>{rev.tenNguoiDuyet}</strong> ({rev.thoiGianDuyet.substring(0, 16).replace('T', ' ')}) – Ý kiến: <em>{rev.nhanXet || 'Không có'}</em>
                  </div>
                ))
              )}
            </div>

            <div style={{ background: '#f8fafc', padding: 14, borderRadius: 6, border: '1px solid #e2e8f0' }}>
              <h4 style={{ marginBottom: 6, color: '#0b192c' }}>Thông tin khóa kết quả</h4>
              {activeRun.lockInfo ? (
                <div style={{ fontSize: 12.5 }}>
                  Mã khóa sổ: <strong>{activeRun.lockInfo.maLock}</strong> | Người khóa: <strong>{activeRun.lockInfo.tenNguoiKhoa}</strong> | Thời điểm: {activeRun.lockInfo.thoiGianKhoa.substring(0, 16).replace('T', ' ')} | Ghi chú: <em>{activeRun.lockInfo.lyDoKhoa}</em>
                </div>
              ) : (
                <p style={{ color: '#64748b', fontSize: 12.5 }}>Kết quả phân bổ chưa khóa sổ.</p>
              )}
            </div>
          </div>
        </div>
      )}

      {reviewModal.open && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(15, 23, 42, 0.55)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div className="card" style={{ width: 480, marginBottom: 0 }}>
            <div className="card-title">Kiểm duyệt phiên phân bổ: {reviewModal.maRun}</div>
            <form onSubmit={handleReviewAction}>
              <div className="form-group">
                <label>Quyết định kiểm duyệt (*)</label>
                <select className="select" value={reviewModal.trangThai} onChange={(e) => setReviewModal({ ...reviewModal, trangThai: e.target.value })}>
                  <option value="APPROVED">Chấp thuận kết quả phân bổ</option>
                  <option value="REJECTED">Từ chối kết quả (Yêu cầu nhập lý do)</option>
                </select>
              </div>
              <div className="form-group">
                <label>Ý kiến nhận xét / Lý do từ chối</label>
                <textarea
                  className="textarea"
                  rows={3}
                  value={reviewModal.nhanXet}
                  onChange={(e) => setReviewModal({ ...reviewModal, nhanXet: e.target.value })}
                  placeholder="Nhập ý kiến đánh giá hoặc lý do từ chối..."
                />
              </div>
              <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 14 }}>
                <button type="button" className="btn btn-secondary" onClick={() => setReviewModal({ open: false, maRun: '', trangThai: 'APPROVED', nhanXet: '' })}>
                  Hủy bỏ
                </button>
                <button type="submit" className="btn btn-primary">Xác nhận kiểm duyệt</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {lockModal.open && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(15, 23, 42, 0.55)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div className="card" style={{ width: 480, marginBottom: 0 }}>
            <div className="card-title">Khóa kết quả phân bổ: {lockModal.maRun}</div>
            <form onSubmit={handleLockAction}>
              <div className="alert alert-warning" style={{ fontSize: 12.5 }}>
                Xác nhận khóa sổ: Sau khi khóa, kết quả của phiên phân bổ này sẽ được cố định để phục vụ báo cáo và kiểm toán, không thể chỉnh sửa trực tiếp.
              </div>
              <div className="form-group">
                <label>Ghi chú / Lý do khóa kết quả</label>
                <input
                  className="input"
                  value={lockModal.lyDoKhoa}
                  onChange={(e) => setLockModal({ ...lockModal, lyDoKhoa: e.target.value })}
                  placeholder="VD: Chốt số liệu kế toán kỳ 2026-10"
                />
              </div>
              <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 14 }}>
                <button type="button" className="btn btn-secondary" onClick={() => setLockModal({ open: false, maRun: '', lyDoKhoa: '' })}>
                  Hủy bỏ
                </button>
                <button type="submit" className="btn btn-success">Xác nhận khóa kết quả</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
export const TraceabilityPage: React.FC = () => {
  const [runs, setRuns] = useState<AllocationRunDto[]>([]);
  const [selectedRunId, setSelectedRunId] = useState<string>('');
  const [maResult, setMaResult] = useState<string>('RES_DIEN_2610_PBA');
  const [traceData, setTraceData] = useState<TraceabilityDto | null>(null);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    apiClient.get<AllocationRunDto[]>('/allocation-runs').then((res) => {
      setRuns(res.data);
      if (res.data.length > 0) {
        setSelectedRunId(res.data[0].maRun);
        if (res.data[0].results.length > 0) {
          const defaultResId = res.data[0].results[0].maResult;
          setMaResult(defaultResId);
          apiClient.get<TraceabilityDto>(`/traceability/result/${defaultResId}`).then((t) => setTraceData(t.data)).catch(() => {});
        }
      }
    }).catch(() => {});
  }, []);

  const currentRun = runs.find((r) => r.maRun === selectedRunId);

  const handleTrace = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setTraceData(null);
    try {
      const res = await apiClient.get<TraceabilityDto>(`/traceability/result/${maResult.trim()}`);
      setTraceData(res.data);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Tra cứu nguồn gốc dữ liệu</h1>
          <div className="page-subtitle">Đối chiếu chi tiết từ một kết quả phân bổ về khoản chi phí nguồn, chứng từ tham chiếu, tiêu thức và phiên bản quy tắc</div>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      <div className="card">
        <div className="card-title">Chọn kết quả phân bổ cần tra cứu nguồn gốc</div>
        <form onSubmit={handleTrace} className="grid-3">
          <div className="form-group">
            <label>Phiên phân bổ</label>
            <select
              className="select"
              value={selectedRunId}
              onChange={(e) => {
                const runId = e.target.value;
                setSelectedRunId(runId);
                const found = runs.find((r) => r.maRun === runId);
                if (found && found.results.length > 0) {
                  setMaResult(found.results[0].maResult);
                }
              }}
            >
              {runs.map((r) => (
                <option key={r.maRun} value={r.maRun}>
                  {r.maRun} - {r.tenCostPool} ({r.kyPhanBo})
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label>Đơn vị nhận chi phí trong phiên</label>
            <select className="select" value={maResult} onChange={(e) => setMaResult(e.target.value)}>
              {currentRun?.results.map((res) => (
                <option key={res.maResult} value={res.maResult}>
                  {res.tenTarget} - {formatVND(res.soTienPhanBo)} ({res.maResult})
                </option>
              ))}
            </select>
          </div>

          <div className="form-group" style={{ display: 'flex', alignItems: 'flex-end', gap: 8 }}>
            <input
              className="input"
              placeholder="Nhập mã kết quả..."
              value={maResult}
              onChange={(e) => setMaResult(e.target.value)}
            />
            <button type="submit" className="btn btn-primary">
              Tra cứu nguồn gốc
            </button>
          </div>
        </form>
      </div>

      {traceData && (
        <div className="card">
          <div className="card-title">
            <span>Hồ sơ nguồn gốc số liệu – Mã kết quả: {traceData.maResult}</span>
            <StatusBadge status={traceData.trangThaiRun} />
          </div>
          <div className="grid-2">
            <div style={{ background: '#f8fafc', padding: 16, borderRadius: 6, border: '1px solid #e2e8f0' }}>
              <h4 style={{ marginBottom: 10, color: '#005fb8' }}>1. Kết quả nhận chi phí của đơn vị</h4>
              <p><strong>Đơn vị thụ hưởng:</strong> {traceData.tenTarget} ({traceData.maTarget})</p>
              <p><strong>Tỷ lệ phân bổ:</strong> {formatPercent(traceData.tyLePhanBo)}</p>
              <p><strong>Số tiền được phân bổ:</strong> <strong style={{ color: '#15803d', fontSize: 15 }}>{formatVND(traceData.soTienPhanBo)}</strong></p>
            </div>

            <div style={{ background: '#f8fafc', padding: 16, borderRadius: 6, border: '1px solid #e2e8f0' }}>
              <h4 style={{ marginBottom: 10, color: '#005fb8' }}>2. Khoản chi phí nguồn & Chứng từ tham chiếu</h4>
              <p><strong>Khoản chi phí:</strong> {traceData.tenCostPool} ({traceData.maCostPool})</p>
              <p><strong>Kỳ kế toán:</strong> {traceData.kyPhanBo}</p>
              <p><strong>Tổng chi phí nguồn:</strong> <strong>{formatVND(traceData.tongTienNguon)}</strong></p>
              <p><strong>Chứng từ tham chiếu:</strong> <strong>{traceData.nguonDuLieuChiPhi || 'Không có'}</strong></p>
            </div>

            <div style={{ background: '#f8fafc', padding: 16, borderRadius: 6, border: '1px solid #e2e8f0' }}>
              <h4 style={{ marginBottom: 10, color: '#005fb8' }}>3. Quy tắc & Phiên bản áp dụng</h4>
              <p><strong>Tên quy tắc:</strong> {traceData.tenRule} ({traceData.maRule})</p>
              <p><strong>Phiên bản quy tắc:</strong> {traceData.maRuleVersion} (Phiên bản {traceData.soPhienBan})</p>
              <p><strong>Thời hạn hiệu lực:</strong> {traceData.ngayHieuLucRule.substring(0, 10)} → {traceData.ngayHetHieuLucRule ? traceData.ngayHetHieuLucRule.substring(0, 10) : 'Không thời hạn'}</p>
              <p><strong>Công thức tính:</strong> {traceData.congThuc}</p>
            </div>

            <div style={{ background: '#f8fafc', padding: 16, borderRadius: 6, border: '1px solid #e2e8f0' }}>
              <h4 style={{ marginBottom: 10, color: '#005fb8' }}>4. Cơ sở tiêu thức & Thông tin phê duyệt</h4>
              <p><strong>Tiêu thức phân bổ:</strong> {traceData.tenDriver} ({traceData.donViTinhDriver})</p>
              <p><strong>Chỉ số của đơn vị / Tổng chỉ số:</strong> {formatNumber(traceData.giaTriDriverTarget)} / {formatNumber(traceData.tongDriver)} {traceData.donViTinhDriver}</p>
              <p><strong>Tài liệu thu thập tiêu thức:</strong> {traceData.nguonDuLieuDriver || 'Không có'}</p>
              <p><strong>Phiên phân bổ:</strong> {traceData.maRun} – Thực hiện bởi {traceData.nguoiThucHien}</p>
              {traceData.thongTinKhoa && (
                <p><strong>Khóa sổ kế toán:</strong> {traceData.thongTinKhoa.tenNguoiKhoa} ({traceData.thongTinKhoa.lyDoKhoa})</p>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
export const AuditLogPage: React.FC = () => {
  const [logs, setLogs] = useState<AuditLogDto[]>([]);
  const [filterDoiTuong, setFilterDoiTuong] = useState<string>('');
  const [filterHanhDong, setFilterHanhDong] = useState<string>('');
  const [selectedLog, setSelectedLog] = useState<AuditLogDto | null>(null);
  const [error, setError] = useState<string>('');

  const loadLogs = async () => {
    try {
      const res = await apiClient.get<AuditLogDto[]>('/audit-logs', {
        params: {
          doiTuong: filterDoiTuong || undefined,
          hanhDong: filterHanhDong || undefined
        }
      });
      setLogs(res.data);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  useEffect(() => {
    loadLogs();
  }, []);

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Nhật ký hệ thống</h1>
          <div className="page-subtitle">Theo dõi lịch sử các thao tác nghiệp vụ, người thực hiện và thời điểm thay đổi dữ liệu</div>
        </div>
        <div className="toolbar">
          <input
            className="input"
            placeholder="Phân mục (VD: CostPool)"
            value={filterDoiTuong}
            onChange={(e) => setFilterDoiTuong(e.target.value)}
            style={{ width: 180 }}
          />
          <input
            className="input"
            placeholder="Loại thao tác..."
            value={filterHanhDong}
            onChange={(e) => setFilterHanhDong(e.target.value)}
            style={{ width: 180 }}
          />
          <button className="btn btn-primary btn-sm" onClick={loadLogs}>Lọc nhật ký</button>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      {selectedLog && (
        <div className="card" style={{ borderColor: '#005fb8' }}>
          <div className="card-title">
            <span>Chi tiết dữ liệu thay đổi – Bản ghi #{selectedLog.maAuditLog}</span>
            <button className="btn btn-secondary btn-sm" onClick={() => setSelectedLog(null)}>Đóng</button>
          </div>
          <div className="grid-2">
            <div>
              <strong>Dữ liệu trước thao tác:</strong>
              <pre style={{ background: '#f8fafc', padding: 10, borderRadius: 6, marginTop: 6, fontSize: 12, overflowX: 'auto' }}>
                {selectedLog.giaTriCu || 'Không có'}
              </pre>
            </div>
            <div>
              <strong>Dữ liệu sau thao tác:</strong>
              <pre style={{ background: '#f8fafc', padding: 10, borderRadius: 6, marginTop: 6, fontSize: 12, overflowX: 'auto' }}>
                {selectedLog.giaTriMoi || 'Không có'}
              </pre>
            </div>
          </div>
        </div>
      )}

      <div className="card">
        <div className="table-responsive">
          <table className="data-table">
            <thead>
              <tr>
                <th>Số TT</th>
                <th>Thời điểm thực hiện</th>
                <th>Người thực hiện</th>
                <th>Thao tác</th>
                <th>Phân mục</th>
                <th>Mã bản ghi</th>
                <th>Nội dung chi tiết</th>
                <th>Chi tiết</th>
              </tr>
            </thead>
            <tbody>
              {logs.map((l) => (
                <tr key={l.maAuditLog}>
                  <td><strong>#{l.maAuditLog}</strong></td>
                  <td>{l.thoiGian.substring(0, 19).replace('T', ' ')}</td>
                  <td>{l.hoTenNguoiDung} ({l.maNguoiDung})</td>
                  <td><span className="badge badge-locked">{l.hanhDong}</span></td>
                  <td>{l.doiTuong}</td>
                  <td><strong>{l.maDoiTuong || '-'}</strong></td>
                  <td>{l.noiDung || '-'}</td>
                  <td>
                    <button className="btn btn-secondary btn-sm" onClick={() => setSelectedLog(l)}>
                      Xem thay đổi
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};
export const UserManagementPage: React.FC = () => {
  const { isAdmin } = useAuth();
  const [users, setUsers] = useState<NguoiDungDto[]>([]);
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [error, setError] = useState<string>('');
  const [message, setMessage] = useState<string>('');

  const [form, setForm] = useState({
    maNguoiDung: '',
    hoTen: '',
    tenDangNhap: '',
    email: '',
    oidcIssuer: 'https://oidc.sharedcost.local',
    oidcSubject: '',
    maRole: 'FINANCE',
    trangThai: 'ACTIVE'
  });

  const loadUsersAndRoles = async () => {
    try {
      const [uRes, rRes] = await Promise.all([
        apiClient.get<NguoiDungDto[]>('/users'),
        apiClient.get<RoleDto[]>('/roles')
      ]);
      setUsers(uRes.data);
      setRoles(rRes.data);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  useEffect(() => {
    loadUsersAndRoles();
  }, []);

  const handleSaveUser = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    try {
      await apiClient.post('/users', form);
      setMessage(`Đã cập nhật thông tin người dùng "${form.hoTen}" (${form.maNguoiDung}) thành công.`);
      setForm({
        maNguoiDung: '',
        hoTen: '',
        tenDangNhap: '',
        email: '',
        oidcIssuer: 'https://oidc.sharedcost.local',
        oidcSubject: '',
        maRole: 'FINANCE',
        trangThai: 'ACTIVE'
      });
      await loadUsersAndRoles();
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Quản lý người dùng và quyền hạn</h1>
          <div className="page-subtitle">Quản lý hồ sơ tài khoản, phân công vai trò nghiệp vụ và kiểm soát trạng thái truy cập hệ thống</div>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {message && <div className="alert alert-success">{message}</div>}

      {isAdmin && (
        <div className="card">
          <div className="card-title">Thông tin tài khoản và phân quyền</div>
          <form onSubmit={handleSaveUser}>
            <div className="grid-3">
              <div className="form-group">
                <label>Mã người dùng (*)</label>
                <input
                  className="input"
                  placeholder="VD: USR_FINANCE_02"
                  value={form.maNguoiDung}
                  onChange={(e) => setForm({ ...form, maNguoiDung: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Họ và tên (*)</label>
                <input
                  className="input"
                  placeholder="VD: Nguyễn Văn An"
                  value={form.hoTen}
                  onChange={(e) => setForm({ ...form, hoTen: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Tên đăng nhập (*)</label>
                <input
                  className="input"
                  placeholder="VD: finance02"
                  value={form.tenDangNhap}
                  onChange={(e) => setForm({ ...form, tenDangNhap: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Địa chỉ Email công vụ</label>
                <input
                  className="input"
                  placeholder="finance02@sharedcost.local"
                  value={form.email}
                  onChange={(e) => setForm({ ...form, email: e.target.value })}
                />
              </div>
              <div className="form-group">
                <label>Vai trò nghiệp vụ (*)</label>
                <select className="select" value={form.maRole} onChange={(e) => setForm({ ...form, maRole: e.target.value })}>
                  {roles.map((r) => (
                    <option key={r.maRole} value={r.maRole}>
                      {r.maRole} – {r.tenRole}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Trạng thái tài khoản</label>
                <select className="select" value={form.trangThai} onChange={(e) => setForm({ ...form, trangThai: e.target.value })}>
                  <option value="ACTIVE">Đang hoạt động</option>
                  <option value="INACTIVE">Ngừng hoạt động</option>
                  <option value="LOCKED">Tạm khóa</option>
                </select>
              </div>
            </div>
            <button type="submit" className="btn btn-primary">Lưu thông tin người dùng</button>
          </form>
        </div>
      )}

      <div className="card">
        <div className="card-title">Danh sách người dùng hệ thống ({users.length})</div>
        <div className="table-responsive">
          <table className="data-table">
            <thead>
              <tr>
                <th>Mã người dùng</th>
                <th>Họ và tên</th>
                <th>Tên đăng nhập</th>
                <th>Email</th>
                <th>Vai trò</th>
                <th>Trạng thái</th>
                {isAdmin && <th>Thao tác quản lý</th>}
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.maNguoiDung}>
                  <td><strong>{u.maNguoiDung}</strong></td>
                  <td>{u.hoTen}</td>
                  <td>{u.tenDangNhap}</td>
                  <td>{u.email || '-'}</td>
                  <td><span className="badge badge-locked">{u.maRole} – {translateRole(u.maRole)}</span></td>
                  <td><StatusBadge status={u.trangThai} /></td>
                  {isAdmin && (
                    <td>
                      <button
                        className="btn btn-secondary btn-sm"
                        onClick={() =>
                          setForm({
                            maNguoiDung: u.maNguoiDung,
                            hoTen: u.hoTen,
                            tenDangNhap: u.tenDangNhap,
                            email: u.email || '',
                            oidcIssuer: u.oidcIssuer,
                            oidcSubject: u.oidcSubject,
                            maRole: u.maRole,
                            trangThai: u.trangThai
                          })
                        }
                      >
                        Chỉnh sửa quyền
                      </button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};
