import { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import useAuth from '@/hooks/useAuth';

interface RequireAuthProps {
  children: ReactNode;
}

// §4.4 — guarda de rota: sessão em restauração aguarda; anônima redireciona pro
// /login preservando o destino em `returnTo` (validado por resolveReturnPath só
// no momento de navegar de volta, nunca aqui); autenticada mas sem instituição
// explícita e consistente (identity nulo/institutionId vazio — spec.md "Contexto
// institucional divergente") bloqueia a renderização do catálogo com uma
// mensagem seguríssima, sem detalhes internos, em vez de arriscar renderizar
// algo institucionalmente incoerente.
export default function RequireAuth({ children }: RequireAuthProps) {
  const { status, identity } = useAuth();
  const location = useLocation();

  if (status === 'loading') {
    return (
      <div className='flex items-center justify-center h-screen w-full'>
        <span className='text-textSecondary'>Carregando sessão...</span>
      </div>
    );
  }

  if (status === 'anonymous') {
    const returnTo = encodeURIComponent(location.pathname + location.search);
    return <Navigate to={`/login?returnTo=${returnTo}`} replace />;
  }

  if (!identity || !identity.institutionId) {
    return (
      <div className='flex flex-col items-center justify-center h-screen w-full gap-2'>
        <h1 className='text-2xl text-danger font-bold'>Contexto institucional inválido</h1>
        <p className='text-textSecondary'>
          Não foi possível confirmar sua instituição. Faça login novamente.
        </p>
      </div>
    );
  }

  return <>{children}</>;
}
