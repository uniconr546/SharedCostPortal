import React from 'react';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { AuthProvider, useAuth } from './context/AuthContext';
import { CostPoolPage, DriverAndValuePage, TargetPage } from './pages/CatalogPages';
import { DashboardAndReportPage, LoginPage } from './pages/LoginAndDashboardPages';
import {
  AllocationRunPage,
  AuditLogPage,
  RuleAndVersionPage,
  SimulationPage,
  TraceabilityPage,
  UserManagementPage
} from './pages/WorkflowPages';

const AppContent: React.FC = () => {
  const { user, loading } = useAuth();

  if (loading) {
    return (
      <div style={{ height: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#0b192c', color: '#f8fafc' }}>
        <h3>Đang tải ứng dụng SharedCost Portal...</h3>
      </div>
    );
  }

  if (!user) {
    return <LoginPage />;
  }

  return (
    <Layout>
      <Routes>
        <Route path="/" element={<DashboardAndReportPage />} />
        <Route path="/cost-pools" element={<CostPoolPage />} />
        <Route path="/drivers" element={<DriverAndValuePage />} />
        <Route path="/targets" element={<TargetPage />} />
        <Route path="/rules" element={<RuleAndVersionPage />} />
        <Route path="/simulations" element={<SimulationPage />} />
        <Route path="/allocation-runs" element={<AllocationRunPage mode="run" />} />
        <Route path="/reviews-locks" element={<AllocationRunPage mode="review" />} />
        <Route path="/traceability" element={<TraceabilityPage />} />
        <Route path="/audit-logs" element={<AuditLogPage />} />
        <Route path="/users" element={<UserManagementPage />} />
      </Routes>
    </Layout>
  );
};

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppContent />
      </AuthProvider>
    </BrowserRouter>
  );
}
