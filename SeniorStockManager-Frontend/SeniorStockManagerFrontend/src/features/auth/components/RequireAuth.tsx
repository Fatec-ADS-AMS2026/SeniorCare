import { ReactNode } from 'react';
import { Navigate, matchPath, useLocation } from 'react-router-dom';
import useAuth from '@/hooks/useAuth';
import { routes } from '@/routes/routes';
import { RouteDefinition } from '@/types/app/RouteDefinition';

interface RequireAuthProps {
  children: ReactNode;
}

// §10.4/§10.5: guarda de rota por componente (o projeto não usa loaders do React
// Router). Sessão em restauração -> aguarda; anônimo -> redireciona pro /login
// (sem loop: Navigate só troca a rota, não recarrega a página, e AuthProvider
// nunca fica 'anonymous' enquanto já está em /login); autenticado sem a permissão
// exigida pela rota atual (achada via matchPath, mesma técnica de
// useBreadcrumbs.ts) -> mostra "acesso negado" inline, não redireciona (§10.4:
// distinção explícita entre 401 e 403).
export default function RequireAuth({ children }: RequireAuthProps) {
  const { status, hasPermission } = useAuth();
  const location = useLocation();

  if (status === 'loading') {
    return (
      <div className='flex items-center justify-center h-screen w-full'>
        <span className='text-textSecondary'>Carregando sessão...</span>
      </div>
    );
  }

  if (status === 'anonymous') {
    return <Navigate to='/login' state={{ from: location }} replace />;
  }

  const matchedRoute = (Object.values(routes) as RouteDefinition[]).find((route) =>
    matchPath(route.path, location.pathname)
  );
  const requiredPermission = matchedRoute?.requiredPermission;

  if (
    requiredPermission &&
    !hasPermission(requiredPermission.resource, requiredPermission.action, requiredPermission.feature)
  ) {
    return (
      <div className='flex flex-col items-center justify-center h-screen w-full gap-2'>
        <h1 className='text-2xl text-danger font-bold'>Acesso negado</h1>
        <p className='text-textSecondary'>
          Você não tem permissão para acessar esta página.
        </p>
      </div>
    );
  }

  return <>{children}</>;
}
