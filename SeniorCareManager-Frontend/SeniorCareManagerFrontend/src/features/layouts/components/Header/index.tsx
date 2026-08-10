import { useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import { SignOut } from '@phosphor-icons/react';
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
