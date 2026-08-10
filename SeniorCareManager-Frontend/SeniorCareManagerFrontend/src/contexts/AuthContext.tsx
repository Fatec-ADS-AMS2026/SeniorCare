import { ReactNode, createContext, useCallback, useEffect, useState } from 'react';
import authService from '@/features/auth/services/authService';
import { registerUnauthorizedHandler } from '@/features/api';
import CurrentIdentity from '@/types/models/CurrentIdentity';

export type AuthStatus = 'loading' | 'authenticated' | 'anonymous';

interface AuthContextType {
  status: AuthStatus;
  identity: CurrentIdentity | null;
  login: (identity: CurrentIdentity) => void;
  logout: () => Promise<void>;
  refresh: () => Promise<void>;
  // §10.5: mesma tripla Resource/Action/Feature usada por [RequirePermission] no
  // backend — sem `feature`, casa qualquer permissão com o Resource/Action pedido
  // (nenhum uso atual do projeto qualifica por Feature).
  hasPermission: (resource: string, action: string, feature?: string) => boolean;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [status, setStatus] = useState<AuthStatus>('loading');
  const [identity, setIdentity] = useState<CurrentIdentity | null>(null);

  // §10.4: restauração de sessão — chama /auth/me a partir do cookie existente (se
  // houver). "Compartilhamento" entre os dois front-ends é automático: os dois
  // observam o mesmo cookie de sessão, sem mecanismo extra.
  const refresh = useCallback(async () => {
    const res = await authService.me();
    if (res.success && res.data) {
      setIdentity(res.data);
      setStatus('authenticated');
    } else {
      setIdentity(null);
      setStatus('anonymous');
    }
  }, []);

  useEffect(() => {
    refresh();
  }, [refresh]);

  // §10.4: 401 fora do fluxo de login/restauração limpa a sessão em memória e manda
  // pro /login (guarda contra loop: só navega se ainda não estiver lá).
  useEffect(() => {
    registerUnauthorizedHandler(() => {
      setIdentity(null);
      setStatus('anonymous');
      if (window.location.pathname !== '/login') {
        window.location.assign('/login');
      }
    });
    return () => registerUnauthorizedHandler(null);
  }, []);

  const login = (newIdentity: CurrentIdentity) => {
    setIdentity(newIdentity);
    setStatus('authenticated');
  };

  const logout = async () => {
    await authService.logout();
    setIdentity(null);
    setStatus('anonymous');
  };

  const hasPermission = (resource: string, action: string, feature?: string) => {
    if (!identity) return false;
    return identity.effectivePermissions.some(
      (permission) =>
        permission.resource === resource &&
        permission.action === action &&
        (feature === undefined || permission.feature === feature)
    );
  };

  return (
    <AuthContext.Provider value={{ status, identity, login, logout, refresh, hasPermission }}>
      {children}
    </AuthContext.Provider>
  );
}
