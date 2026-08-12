import { Outlet } from 'react-router-dom';
import RequireAuth from '@/features/auth/components/RequireAuth';
import GlobalHeader from './GlobalHeader';

// §5.4/§5.5 — chrome comum a toda rota autenticada do portal (catálogo,
// perfil, segurança): guarda de sessão/instituição (§4.4) + navegação global.
export default function AuthenticatedLayout() {
  return (
    <RequireAuth>
      <div className='flex flex-col min-h-screen w-full bg-background'>
        <GlobalHeader />
        <main className='flex-1 flex flex-col'>
          <Outlet />
        </main>
      </div>
    </RequireAuth>
  );
}
