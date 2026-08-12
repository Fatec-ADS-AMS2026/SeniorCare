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
  hasPermission: (resource: string, action: string, feature?: string) => boolean;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [status, setStatus] = useState<AuthStatus>('loading');
  const [identity, setIdentity] = useState<CurrentIdentity | null>(null);

  // §4.2 — restauração de sessão: chama GET /auth/me a partir do único cookie
  // HttpOnly de sessão (nunca há token separado a restaurar em memória — ver
  // comentário em features/api/api.ts). "Compartilhamento" entre os três
  // front-ends é automático: todos observam o mesmo cookie, sem mecanismo extra.
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

  // §4.3 — "expiração não renovável, limpeza completa do contexto local": o
  // cookie de sessão usa SlidingExpiration=false com rotação só via
  // OnValidatePrincipal no backend — o front-end nunca tenta renovar por conta
  // própria. Qualquer 401 fora do bootstrap de login/restauração (ex.: sessão
  // expirou/foi revogada no meio da navegação) limpa TODO o estado local
  // (identity=null) e manda pro /login; não há renovação silenciosa aqui.
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

  // §4.3 — logout limpa o contexto local mesmo se a chamada ao backend falhar
  // (ex.: rede indisponível): o objetivo é nunca deixar o portal "logado" na UI
  // depois de uma ação explícita de logout, mesmo que a revogação do lado do
  // servidor não seja confirmada.
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
