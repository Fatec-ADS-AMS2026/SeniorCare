import { useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, ShieldCheck, SignOut, UserCircle } from '@phosphor-icons/react';
import { ThemeContext } from '@/contexts/ThemeContext';
import logo from '@/assets/images/logo.png';
import AccessibilityBar from './AccessibilityBar';
import useAuth from '@/hooks/useAuth';
import useAppRoutes from '@/hooks/useAppRoutes';

export default function Header() {
  const { theme } = useContext(ThemeContext);
  const { status, identity, logout } = useAuth();
  const navigate = useNavigate();
  const routes = useAppRoutes();

  const handleLogout = async () => {
    await logout();
    navigate(routes.LOGIN.path, { replace: true });
  };

  return (
    <header className='bg-neutralWhite flex flex-col overflow-hidden items-center justify-center flex-shrink-0 shadow z-20 sticky top-0 left-0 right-0'>
      <AccessibilityBar />

      <div className='h-16 w-full flex items-center justify-between overflow-hidden'>
        <img
          src={logo}
          alt='logo do sistema'
          className={`aspect-square size-14 object-cover ml-2 ${
            theme === 'high-contrast' ? 'grayscale' : 'grayscale-0'
          }`}
        />
        {status === 'authenticated' && identity && (
          <div className='flex items-center gap-3 mr-4'>
            {/* §7.3 — retorno ao portal e links pra perfil/segurança (rotas do
                próprio Senior Portal, senior-portal-contracts.md §4) — navegação
                de mesma origem, sem duplicar nenhuma regra de credencial/sessão
                aqui: são só links, a sessão continua sendo o único cookie
                HttpOnly compartilhado (§7.2). */}
            <a
              href='/'
              className='hidden md:flex items-center gap-1 text-sm text-textSecondary hover:text-secondary transition-colors'
            >
              <ArrowLeft size={20} />
              Portal
            </a>
            <a
              href='/profile'
              className='hidden md:flex items-center gap-1 text-sm text-textSecondary hover:text-secondary transition-colors'
            >
              <UserCircle size={20} />
              Perfil
            </a>
            <a
              href='/security'
              className='hidden md:flex items-center gap-1 text-sm text-textSecondary hover:text-secondary transition-colors'
            >
              <ShieldCheck size={20} />
              Segurança
            </a>
            <span className='text-sm text-textSecondary hidden sm:inline'>
              {identity.displayName}
            </span>
            <button
              onClick={handleLogout}
              className='flex items-center gap-1 text-sm text-primary hover:text-secondary transition-colors'
            >
              <SignOut size={20} weight='fill' />
              Sair
            </button>
          </div>
        )}
      </div>
    </header>
  );
}
