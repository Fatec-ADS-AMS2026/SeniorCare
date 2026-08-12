import useAuth from '@/hooks/useAuth';

// §5.4 — /profile é rota do próprio Senior Portal
// (senior-portal-contracts.md §4); assistência e estoque não duplicam esta
// tela, só linkam de volta pra navegação global.
export default function ProfilePage() {
  const { identity } = useAuth();

  if (!identity) return null;

  return (
    <div className='max-w-2xl mx-auto p-4 md:p-8 w-full'>
      <h1 className='text-2xl font-bold text-secondary mb-6'>Perfil</h1>
      <dl className='flex flex-col gap-4'>
        <div>
          <dt className='text-sm font-semibold text-textSecondary'>Nome</dt>
          <dd className='text-textPrimary'>{identity.displayName}</dd>
        </div>
        <div>
          <dt className='text-sm font-semibold text-textSecondary'>E-mail</dt>
          <dd className='text-textPrimary'>{identity.email}</dd>
        </div>
        <div>
          <dt className='text-sm font-semibold text-textSecondary'>Instituição</dt>
          <dd className='text-textPrimary'>{identity.institutionName}</dd>
        </div>
        {identity.roles.length > 0 && (
          <div>
            <dt className='text-sm font-semibold text-textSecondary'>Papéis</dt>
            <dd className='text-textPrimary'>{identity.roles.join(', ')}</dd>
          </div>
        )}
      </dl>
    </div>
  );
}
