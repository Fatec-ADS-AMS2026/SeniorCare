import { useContext, useEffect } from 'react';
import fotoEntradaSistema from '@/assets/images/fotoEntradaSistema.jpg';
import { ThemeContext } from '@/contexts/ThemeContext';
import { useNavigate } from 'react-router-dom';
import useAppRoutes from '@/hooks/useAppRoutes';

// §9.2 — landing page legada: sob roteamento por caminho (VITE_BASE_PATH
// ativo, §8.2/§9.7) a raiz do Senior Portal é que recebe quem chega "frio"
// (design.md: "a raiz apontará para o portal e logins legados
// redirecionarão para ele"). Mesmo sinal de ativação usado por
// AppRoutes.tsx (basename) e LoginForm.tsx.
const isPathBasedRouting = Boolean(
  import.meta.env.VITE_BASE_PATH && import.meta.env.VITE_BASE_PATH !== '/'
);
const PORTAL_ROOT_PATH = '/';

export default function LandingPage() {
  const { theme } = useContext(ThemeContext);
  const navigate = useNavigate();
  const routes = useAppRoutes();

  useEffect(() => {
    if (isPathBasedRouting) {
      window.location.assign(PORTAL_ROOT_PATH);
    }
  }, []);

  const handleLoginClick = () => {
    navigate(routes.LOGIN.path);
  };

  if (isPathBasedRouting) {
    return (
      <p className='text-textSecondary text-center' role='status'>
        Redirecionando para o portal...
      </p>
    );
  }

  return (
    <div className='flex'>
      <div className='w-[40%] h-full bg-neutralWhite flex flex-col justify-center'>
        <h1 className='text-secondary font-bold text-5xl mx-16'>
          Plataforma de ferramentas para gerenciamento da parte administrativa
        </h1>
        <p className='text-textSecondary font-semibold text-2xl mt-2 mx-16'>
          Plataforma de recursos para administrar o estoque, compras e doações
          da instituição
        </p>

        <button
          onClick={handleLoginClick}
          className='bg-primary h-14 w-52 mt-5 rounded text-neutralWhite font-semibold hover:bg-hoverButton hover:scale-105 transition-colors mx-16 text-lg'
        >
          Fazer login
        </button>
      </div>
      <img
        src={fotoEntradaSistema}
        alt='Foto Entrada do Sistema'
        className={`w-[60%] h-full object-cover ${
          theme === 'high-contrast' ? 'grayscale' : 'grayscale-0'
        }`}
      />
    </div>
  );
}
