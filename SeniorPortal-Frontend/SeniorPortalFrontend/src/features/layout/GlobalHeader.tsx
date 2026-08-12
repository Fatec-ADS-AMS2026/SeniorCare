import { NavLink } from 'react-router-dom';
import { SignOut } from '@phosphor-icons/react';
import useAuth from '@/hooks/useAuth';
import useRuntimeConfig from '@/hooks/useRuntimeConfig';
import PreferencesControls from '@/features/preferences/components/PreferencesControls';

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  `text-sm font-semibold transition-colors ${isActive ? 'text-primary' : 'text-textSecondary hover:text-primary'}`;

// §5.4/§5.5 — navegação global presente em todas as rotas autenticadas do
// portal: catálogo, perfil, segurança, preferências e logout compartilhado
// (docs/architecture/senior-portal-contracts.md §4-§5). Tokens visuais
// reusados de §4 (mesmas cores/fonte de care-web/stock-web) — sem design
// system novo, conforme design.md decisão 8.
export default function GlobalHeader() {
  const { logout } = useAuth();
  const { config } = useRuntimeConfig();

  return (
    <header className='flex flex-wrap items-center justify-between gap-4 px-4 md:px-8 py-4 bg-neutralWhite border-b border-neutralLight'>
      <NavLink to='/' className='font-bold text-lg text-secondary' end>
        {config.publicName}
      </NavLink>

      <nav className='flex items-center gap-4' aria-label='Navegação global'>
        <NavLink to='/' className={navLinkClass} end>
          Catálogo
        </NavLink>
        <NavLink to='/profile' className={navLinkClass}>
          Perfil
        </NavLink>
        <NavLink to='/security' className={navLinkClass}>
          Segurança
        </NavLink>
      </nav>

      <div className='flex items-center gap-4'>
        <PreferencesControls />
        <button
          type='button'
          onClick={() => logout()}
          className='flex items-center gap-1 text-sm text-danger hover:text-hoverDanger transition-colors'
        >
          <SignOut size={20} aria-hidden='true' />
          Sair
        </button>
      </div>
    </header>
  );
}
