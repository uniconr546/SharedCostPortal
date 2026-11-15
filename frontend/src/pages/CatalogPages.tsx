import React, { useEffect, useState } from 'react';
import { apiClient, extractErrorMessage, formatNumber, formatVND } from '../api';
import { StatusBadge } from '../components/Layout';
import { useAuth } from '../context/AuthContext';
import { CostPoolDto, DriverDto, DriverValueDto, TargetDto } from '../types';
export const CostPoolPage: React.FC = () => {
  const { canManageCatalog } = useAuth();
  const [items, setItems] = useState<CostPoolDto[]>([]);
  const [filterPeriod, setFilterPeriod] = useState<string>('');
  const [keyword, setKeyword] = useState<string>('');
  const [error, setError] = useState<string>('');
  const [message, setMessage] = useState<string>('');

  const [form, setForm] = useState({
    maCostPool: '',
    tenCostPool: '',
    kyApDung: '2026-10',
    soTien: 0,
    nguonDuLieu: '',
    moTa: '',
    trangThai: 'ACTIVE'
  });

  const loadCostPools = async () => {
    try {
      const res = await apiClient.get<CostPoolDto[]>('/cost-pools', {
        params: { kyApDung: filterPeriod || undefined, keyword: keyword || undefined }
      });
      setItems(res.data);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  useEffect(() => {
    loadCostPools();
  }, []);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    try {
      await apiClient.post('/cost-pools', form);
      setMessage(`Đã lưu thông tin khoản chi phí "${form.tenCostPool}" (${form.maCostPool}) thành công.`);
      setForm({ maCostPool: '', tenCostPool: '', kyApDung: '2026-10', soTien: 0, nguonDuLieu: '', moTa: '', trangThai: 'ACTIVE' });
      await loadCostPools();
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Quản lý khoản chi phí</h1>
          <div className="page-subtitle">Khai báo và quản lý các nhóm chi phí dùng chung phát sinh theo từng kỳ kế toán</div>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {message && <div className="alert alert-success">{message}</div>}

      {canManageCatalog && (
        <div className="card">
          <div className="card-title">Thông tin khoản chi phí dùng chung</div>
          <form onSubmit={handleSave}>
            <div className="grid-3">
              <div className="form-group">
                <label>Mã khoản chi phí (*)</label>
                <input
                  className="input"
                  placeholder="VD: CP_THUEVP_2610"
                  value={form.maCostPool}
                  onChange={(e) => setForm({ ...form, maCostPool: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Tên khoản chi phí (*)</label>
                <input
                  className="input"
                  placeholder="VD: Chi phí thuê văn phòng T10/2026"
                  value={form.tenCostPool}
                  onChange={(e) => setForm({ ...form, tenCostPool: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Kỳ áp dụng (YYYY-MM) (*)</label>
                <input
                  className="input"
                  placeholder="2026-10"
                  value={form.kyApDung}
                  onChange={(e) => setForm({ ...form, kyApDung: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Số tiền chi phí nguồn (VNĐ) (*)</label>
                <input
                  type="number"
                  step="0.01"
                  className="input"
                  value={form.soTien}
                  onChange={(e) => setForm({ ...form, soTien: Number(e.target.value) })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Chứng từ tham chiếu</label>
                <input
                  className="input"
                  placeholder="VD: Hóa đơn HĐ-EVN-202610"
                  value={form.nguonDuLieu}
                  onChange={(e) => setForm({ ...form, nguonDuLieu: e.target.value })}
                />
              </div>
              <div className="form-group">
                <label>Trạng thái</label>
                <select className="select" value={form.trangThai} onChange={(e) => setForm({ ...form, trangThai: e.target.value })}>
                  <option value="ACTIVE">Đang áp dụng</option>
                  <option value="DRAFT">Bản nháp</option>
                  <option value="INACTIVE">Ngừng sử dụng</option>
                </select>
              </div>
            </div>
            <div className="form-group">
              <label>Diễn giải chi tiết</label>
              <input
                className="input"
                placeholder="Ghi chú nội dung hoặc phạm vi khoản chi phí"
                value={form.moTa}
                onChange={(e) => setForm({ ...form, moTa: e.target.value })}
              />
            </div>
            <div className="toolbar">
              <button type="submit" className="btn btn-primary">Lưu khoản chi phí</button>
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => setForm({ maCostPool: '', tenCostPool: '', kyApDung: '2026-10', soTien: 0, nguonDuLieu: '', moTa: '', trangThai: 'ACTIVE' })}
              >
                Làm mới biểu mẫu
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="card">
        <div className="card-title">
          <span>Danh sách khoản chi phí ({items.length})</span>
          <div className="toolbar">
            <input
              className="input"
              placeholder="Kỳ (VD: 2026-10)"
              value={filterPeriod}
              onChange={(e) => setFilterPeriod(e.target.value)}
              style={{ width: 150 }}
            />
            <input
              className="input"
              placeholder="Tìm theo mã, tên, chứng từ..."
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              style={{ width: 230 }}
            />
            <button className="btn btn-secondary btn-sm" onClick={loadCostPools}>Tìm kiếm</button>
          </div>
        </div>

        <div className="table-responsive">
          <table className="data-table">
            <thead>
              <tr>
                <th>Mã khoản chi phí</th>
                <th>Tên khoản chi phí</th>
                <th>Kỳ áp dụng</th>
                <th>Số tiền nguồn</th>
                <th>Chứng từ tham chiếu</th>
                <th>Trạng thái</th>
                {canManageCatalog && <th>Thao tác</th>}
              </tr>
            </thead>
            <tbody>
              {items.map((cp) => (
                <tr key={cp.maCostPool}>
                  <td><strong>{cp.maCostPool}</strong></td>
                  <td>{cp.tenCostPool}</td>
                  <td><strong>{cp.kyApDung}</strong></td>
                  <td style={{ color: '#005fb8', fontWeight: 700 }}>{formatVND(cp.soTien)}</td>
                  <td>{cp.nguonDuLieu || '-'}</td>
                  <td><StatusBadge status={cp.trangThai} /></td>
                  {canManageCatalog && (
                    <td>
                      <button
                        className="btn btn-secondary btn-sm"
                        onClick={() =>
                          setForm({
                            maCostPool: cp.maCostPool,
                            tenCostPool: cp.tenCostPool,
                            kyApDung: cp.kyApDung,
                            soTien: cp.soTien,
                            nguonDuLieu: cp.nguonDuLieu || '',
                            moTa: cp.moTa || '',
                            trangThai: cp.trangThai
                          })
                        }
                      >
                        Chỉnh sửa
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
export const DriverAndValuePage: React.FC = () => {
  const { canManageCatalog } = useAuth();
  const [drivers, setDrivers] = useState<DriverDto[]>([]);
  const [targets, setTargets] = useState<TargetDto[]>([]);
  const [driverValues, setDriverValues] = useState<DriverValueDto[]>([]);
  const [filterKy, setFilterKy] = useState<string>('2026-10');
  const [filterDriver, setFilterDriver] = useState<string>('');
  const [error, setError] = useState<string>('');
  const [message, setMessage] = useState<string>('');

  const [driverForm, setDriverForm] = useState({
    maDriver: '',
    tenDriver: '',
    donViTinh: '',
    moTa: '',
    trangThai: 'ACTIVE'
  });

  const [valForm, setValForm] = useState({
    maDriverValue: '',
    maDriver: 'DRV_DIENTICH',
    maTarget: 'TGT_PBA',
    kyPhanBo: '2026-10',
    giaTriDriver: 0,
    donViTinh: '',
    nguonDuLieu: ''
  });

  const loadAll = async () => {
    try {
      const [drvRes, tgtRes, dvRes] = await Promise.all([
        apiClient.get<DriverDto[]>('/drivers'),
        apiClient.get<TargetDto[]>('/targets'),
        apiClient.get<DriverValueDto[]>('/driver-values', {
          params: { kyPhanBo: filterKy || undefined, maDriver: filterDriver || undefined }
        })
      ]);
      setDrivers(drvRes.data);
      setTargets(tgtRes.data);
      setDriverValues(dvRes.data);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  useEffect(() => {
    loadAll();
  }, []);

  const handleSaveDriver = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    try {
      await apiClient.post('/drivers', driverForm);
      setMessage(`Đã lưu tiêu thức phân bổ "${driverForm.tenDriver}" thành công.`);
      setDriverForm({ maDriver: '', tenDriver: '', donViTinh: '', moTa: '', trangThai: 'ACTIVE' });
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  const handleSaveDriverValue = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    try {
      await apiClient.post('/driver-values', valForm);
      setMessage(`Đã cập nhật chỉ số tiêu thức cho đối tượng ${valForm.maTarget} kỳ ${valForm.kyPhanBo}.`);
      setValForm({ ...valForm, maDriverValue: '', giaTriDriver: 0 });
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Quản lý tiêu thức phân bổ</h1>
          <div className="page-subtitle">Thiết lập danh mục tiêu thức và khai báo giá trị đo lường thực tế theo từng đơn vị trong kỳ</div>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {message && <div className="alert alert-success">{message}</div>}

      {canManageCatalog && (
        <div className="grid-2">
          <div className="card">
            <div className="card-title">Danh mục tiêu thức phân bổ</div>
            <form onSubmit={handleSaveDriver}>
              <div className="grid-2">
                <div className="form-group">
                  <label>Mã tiêu thức (*)</label>
                  <input
                    className="input"
                    placeholder="VD: DRV_SOGIOMAY"
                    value={driverForm.maDriver}
                    onChange={(e) => setDriverForm({ ...driverForm, maDriver: e.target.value })}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Đơn vị tính (*)</label>
                  <input
                    className="input"
                    placeholder="VD: m2, Người, Giờ..."
                    value={driverForm.donViTinh}
                    onChange={(e) => setDriverForm({ ...driverForm, donViTinh: e.target.value })}
                    required
                  />
                </div>
              </div>
              <div className="form-group">
                <label>Tên tiêu thức phân bổ (*)</label>
                <input
                  className="input"
                  placeholder="VD: Số giờ vận hành thiết bị"
                  value={driverForm.tenDriver}
                  onChange={(e) => setDriverForm({ ...driverForm, tenDriver: e.target.value })}
                  required
                />
              </div>
              <div className="grid-2">
                <div className="form-group">
                  <label>Mô tả phương pháp thu thập</label>
                  <input
                    className="input"
                    placeholder="Mô tả ý nghĩa tiêu thức"
                    value={driverForm.moTa}
                    onChange={(e) => setDriverForm({ ...driverForm, moTa: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label>Trạng thái</label>
                  <select className="select" value={driverForm.trangThai} onChange={(e) => setDriverForm({ ...driverForm, trangThai: e.target.value })}>
                    <option value="ACTIVE">Đang áp dụng</option>
                    <option value="DRAFT">Bản nháp</option>
                    <option value="INACTIVE">Ngừng sử dụng</option>
                  </select>
                </div>
              </div>
              <button type="submit" className="btn btn-primary">Lưu tiêu thức</button>
            </form>
          </div>

          <div className="card">
            <div className="card-title">Cập nhật giá trị tiêu thức theo kỳ kế toán</div>
            <form onSubmit={handleSaveDriverValue}>
              <div className="grid-2">
                <div className="form-group">
                  <label>Tiêu thức phân bổ (*)</label>
                  <select className="select" value={valForm.maDriver} onChange={(e) => setValForm({ ...valForm, maDriver: e.target.value })}>
                    {drivers.map((d) => (
                      <option key={d.maDriver} value={d.maDriver}>
                        {d.tenDriver} ({d.donViTinh})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Đối tượng nhận chi phí (*)</label>
                  <select className="select" value={valForm.maTarget} onChange={(e) => setValForm({ ...valForm, maTarget: e.target.value })}>
                    {targets.map((t) => (
                      <option key={t.maTarget} value={t.maTarget}>
                        {t.tenTarget} ({t.maTarget})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Kỳ phân bổ (YYYY-MM) (*)</label>
                  <input
                    className="input"
                    value={valForm.kyPhanBo}
                    onChange={(e) => setValForm({ ...valForm, kyPhanBo: e.target.value })}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Giá trị đo lường (*)</label>
                  <input
                    type="number"
                    step="0.0001"
                    className="input"
                    value={valForm.giaTriDriver}
                    onChange={(e) => setValForm({ ...valForm, giaTriDriver: Number(e.target.value) })}
                    required
                  />
                </div>
              </div>
              <div className="form-group">
                <label>Tài liệu / Chứng từ thu thập số liệu</label>
                <input
                  className="input"
                  placeholder="VD: Biên bản đo đạc diện tích / Bảng tổng hợp nhân sự..."
                  value={valForm.nguonDuLieu}
                  onChange={(e) => setValForm({ ...valForm, nguonDuLieu: e.target.value })}
                />
              </div>
              <button type="submit" className="btn btn-success">Lưu giá trị tiêu thức</button>
            </form>
          </div>
        </div>
      )}

      <div className="card">
        <div className="card-title">Danh mục tiêu thức phân bổ hiện có</div>
        <div className="table-responsive">
          <table className="data-table">
            <thead>
              <tr>
                <th>Mã tiêu thức</th>
                <th>Tên tiêu thức phân bổ</th>
                <th>Đơn vị tính</th>
                <th>Mô tả</th>
                <th>Trạng thái</th>
                {canManageCatalog && <th>Thao tác</th>}
              </tr>
            </thead>
            <tbody>
              {drivers.map((d) => (
                <tr key={d.maDriver}>
                  <td><strong>{d.maDriver}</strong></td>
                  <td>{d.tenDriver}</td>
                  <td>{d.donViTinh}</td>
                  <td>{d.moTa || '-'}</td>
                  <td><StatusBadge status={d.trangThai} /></td>
                  {canManageCatalog && (
                    <td>
                      <button
                        className="btn btn-secondary btn-sm"
                        onClick={() =>
                          setDriverForm({
                            maDriver: d.maDriver,
                            tenDriver: d.tenDriver,
                            donViTinh: d.donViTinh,
                            moTa: d.moTa || '',
                            trangThai: d.trangThai
                          })
                        }
                      >
                        Chỉnh sửa
                      </button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <div className="card">
        <div className="card-title">
          <span>Bảng giá trị tiêu thức theo đối tượng và kỳ ({driverValues.length} bản ghi)</span>
          <div className="toolbar">
            <input
              className="input"
              placeholder="Kỳ YYYY-MM"
              value={filterKy}
              onChange={(e) => setFilterKy(e.target.value)}
              style={{ width: 140 }}
            />
            <select className="select" value={filterDriver} onChange={(e) => setFilterDriver(e.target.value)} style={{ width: 220 }}>
              <option value="">-- Tất cả tiêu thức --</option>
              {drivers.map((d) => (
                <option key={d.maDriver} value={d.maDriver}>{d.tenDriver}</option>
              ))}
            </select>
            <button className="btn btn-secondary btn-sm" onClick={loadAll}>Lọc dữ liệu</button>
          </div>
        </div>

        <div className="table-responsive">
          <table className="data-table">
            <thead>
              <tr>
                <th>Mã bản ghi</th>
                <th>Kỳ kế toán</th>
                <th>Tiêu thức phân bổ</th>
                <th>Đối tượng nhận chi phí</th>
                <th>Giá trị đo lường</th>
                <th>Đơn vị</th>
                <th>Chứng từ tham chiếu</th>
                {canManageCatalog && <th>Thao tác</th>}
              </tr>
            </thead>
            <tbody>
              {driverValues.map((dv) => (
                <tr key={dv.maDriverValue}>
                  <td><strong>{dv.maDriverValue}</strong></td>
                  <td><strong>{dv.kyPhanBo}</strong></td>
                  <td>{dv.tenDriver}</td>
                  <td>{dv.tenTarget} ({dv.maTarget})</td>
                  <td style={{ fontWeight: 700, color: '#005fb8' }}>{formatNumber(dv.giaTriDriver)}</td>
                  <td>{dv.donViTinh}</td>
                  <td>{dv.nguonDuLieu || '-'}</td>
                  {canManageCatalog && (
                    <td>
                      <button
                        className="btn btn-secondary btn-sm"
                        onClick={() =>
                          setValForm({
                            maDriverValue: dv.maDriverValue,
                            maDriver: dv.maDriver,
                            maTarget: dv.maTarget,
                            kyPhanBo: dv.kyPhanBo,
                            giaTriDriver: dv.giaTriDriver,
                            donViTinh: dv.donViTinh || '',
                            nguonDuLieu: dv.nguonDuLieu || ''
                          })
                        }
                      >
                        Cập nhật số liệu
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
export const TargetPage: React.FC = () => {
  const { canManageCatalog } = useAuth();
  const [targets, setTargets] = useState<TargetDto[]>([]);
  const [keyword, setKeyword] = useState<string>('');
  const [error, setError] = useState<string>('');
  const [message, setMessage] = useState<string>('');

  const [form, setForm] = useState({
    maTarget: '',
    tenTarget: '',
    loaiTarget: 'Phòng ban',
    moTa: '',
    trangThai: 'ACTIVE'
  });

  const loadTargets = async () => {
    try {
      const res = await apiClient.get<TargetDto[]>('/targets', {
        params: { keyword: keyword || undefined }
      });
      setTargets(res.data);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  useEffect(() => {
    loadTargets();
  }, []);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    try {
      await apiClient.post('/targets', form);
      setMessage(`Đã lưu đối tượng nhận chi phí "${form.tenTarget}" thành công.`);
      setForm({ maTarget: '', tenTarget: '', loaiTarget: 'Phòng ban', moTa: '', trangThai: 'ACTIVE' });
      await loadTargets();
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Quản lý đối tượng nhận chi phí</h1>
          <div className="page-subtitle">Khai báo các phòng ban, trung tâm chi phí hoặc đơn vị kinh doanh nhận phân bổ</div>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {message && <div className="alert alert-success">{message}</div>}

      {canManageCatalog && (
        <div className="card">
          <div className="card-title">Thông tin đối tượng nhận chi phí</div>
          <form onSubmit={handleSave}>
            <div className="grid-3">
              <div className="form-group">
                <label>Mã đối tượng (*)</label>
                <input
                  className="input"
                  placeholder="VD: TGT_PBD"
                  value={form.maTarget}
                  onChange={(e) => setForm({ ...form, maTarget: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Tên đơn vị / Phòng ban (*)</label>
                <input
                  className="input"
                  placeholder="VD: Phòng Vận hành Hệ thống"
                  value={form.tenTarget}
                  onChange={(e) => setForm({ ...form, tenTarget: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Phân loại đơn vị (*)</label>
                <select className="select" value={form.loaiTarget} onChange={(e) => setForm({ ...form, loaiTarget: e.target.value })}>
                  <option value="Phòng ban">Phòng ban</option>
                  <option value="Trung tâm chi phí">Trung tâm chi phí</option>
                  <option value="Đơn vị kinh doanh">Đơn vị kinh doanh</option>
                </select>
              </div>
            </div>
            <div className="grid-2">
              <div className="form-group">
                <label>Mô tả chức năng đơn vị</label>
                <input className="input" value={form.moTa} onChange={(e) => setForm({ ...form, moTa: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Trạng thái</label>
                <select className="select" value={form.trangThai} onChange={(e) => setForm({ ...form, trangThai: e.target.value })}>
                  <option value="ACTIVE">Đang áp dụng</option>
                  <option value="DRAFT">Bản nháp</option>
                  <option value="INACTIVE">Ngừng sử dụng</option>
                </select>
              </div>
            </div>
            <button type="submit" className="btn btn-primary">Lưu đối tượng nhận chi phí</button>
          </form>
        </div>
      )}

      <div className="card">
        <div className="card-title">
          <span>Danh sách đơn vị nhận chi phí ({targets.length})</span>
          <div className="toolbar">
            <input
              className="input"
              placeholder="Tìm mã hoặc tên đơn vị..."
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              style={{ width: 220 }}
            />
            <button className="btn btn-secondary btn-sm" onClick={loadTargets}>Tìm kiếm</button>
          </div>
        </div>

        <div className="table-responsive">
          <table className="data-table">
            <thead>
              <tr>
                <th>Mã đối tượng</th>
                <th>Tên đơn vị nhận chi phí</th>
                <th>Phân loại</th>
                <th>Mô tả</th>
                <th>Trạng thái</th>
                {canManageCatalog && <th>Thao tác</th>}
              </tr>
            </thead>
            <tbody>
              {targets.map((t) => (
                <tr key={t.maTarget}>
                  <td><strong>{t.maTarget}</strong></td>
                  <td>{t.tenTarget}</td>
                  <td>{t.loaiTarget}</td>
                  <td>{t.moTa || '-'}</td>
                  <td><StatusBadge status={t.trangThai} /></td>
                  {canManageCatalog && (
                    <td>
                      <button
                        className="btn btn-secondary btn-sm"
                        onClick={() =>
                          setForm({
                            maTarget: t.maTarget,
                            tenTarget: t.tenTarget,
                            loaiTarget: t.loaiTarget,
                            moTa: t.moTa || '',
                            trangThai: t.trangThai
                          })
                        }
                      >
                        Chỉnh sửa
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
