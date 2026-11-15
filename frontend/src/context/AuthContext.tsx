import React, { createContext, useContext, useEffect, useState } from 'react';
import { apiClient } from '../api';
import { AuthSessionDto, NguoiDungDto } from '../types';

interface AuthContextValue {
  user: NguoiDungDto | null;
  demoAccounts: NguoiDungDto[];
  isDemoModeAvailable: boolean;
  loading: boolean;
  loginWithOidc: (tenDangNhap: string, oidcIssuer?: string, oidcSubject?: string) => Promise<void>;
  logout: () => void;
  canManageCatalog: boolean;
  canReviewAndLock: boolean;
  canViewAudit: boolean;
  isAdmin: boolean;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<NguoiDungDto | null>(null);
  const [demoAccounts, setDemoAccounts] = useState<NguoiDungDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);

  const isDevEnv = import.meta.env.DEV || import.meta.env.VITE_ENABLE_DEMO === 'true';

  const loadDemoAccounts = async () => {
    if (!isDevEnv) return;
    try {
      const res = await apiClient.get<NguoiDungDto[]>('/auth/demo-accounts');
      setDemoAccounts(res.data);
    } catch {
      setDemoAccounts([]);
    }
  };

  useEffect(() => {
    const initAuth = async () => {
      await loadDemoAccounts();
      const token = localStorage.getItem('oidc_access_token');
      if (token) {
        try {
          const res = await apiClient.get<NguoiDungDto>('/auth/me');
          setUser(res.data);
        } catch {
          localStorage.removeItem('oidc_access_token');
          setUser(null);
        }
      }
      setLoading(false);
    };
    initAuth();
  }, []);

  const loginWithOidc = async (tenDangNhap: string, oidcIssuer?: string, oidcSubject?: string) => {
    const res = await apiClient.post<AuthSessionDto>('/auth/oidc-login', {
      tenDangNhap,
      oidcIssuer,
      oidcSubject
    });
    localStorage.setItem('oidc_access_token', res.data.accessToken);
    setUser(res.data.user);
    await loadDemoAccounts();
  };

  const logout = () => {
    localStorage.removeItem('oidc_access_token');
    setUser(null);
  };

  const role = user?.maRole || '';
  const isAdmin = role === 'ADMIN';
  const canManageCatalog = role === 'ADMIN' || role === 'FINANCE';
  const canReviewAndLock = role === 'ADMIN' || role === 'REVIEWER';
  const canViewAudit = role === 'ADMIN' || role === 'AUDITOR' || role === 'REVIEWER' || role === 'FINANCE';

  return (
    <AuthContext.Provider
      value={{
        user,
        demoAccounts,
        isDemoModeAvailable: isDevEnv && demoAccounts.length > 0,
        loading,
        loginWithOidc,
        logout,
        canManageCatalog,
        canReviewAndLock,
        canViewAudit,
        isAdmin
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextValue => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth phải được sử dụng bên trong AuthProvider');
  return ctx;
};
