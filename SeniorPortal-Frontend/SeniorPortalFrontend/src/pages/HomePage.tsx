import useAuth from '@/hooks/useAuth';
import useRuntimeConfig from '@/hooks/useRuntimeConfig';

// Placeholder autenticado — o catálogo de módulos em si (GET /api/v1/me/modules)
// é escopo de §5. Esta página só prova que a base da aplicação (§4: sessão,
// contexto institucional, logout) funciona ponta a ponta.
export default function HomePage() {
  const { identity, logout } = useAuth();
  const { config } = useRuntimeConfig();

  return (
    <div className='flex flex-col items-center justify-center h-screen w-full gap-4'>
      <h1 className='text-2xl font-bold text-secondary'>{config.publicName}</h1>
      <p className='text-textSecondary'>
        {identity?.displayName} — {identity?.institutionName}
      </p>
      <button
        type='button'
        onClick={() => logout()}
        className='text-danger underline'
      >
        Sair
      </button>
    </div>
  );
}
