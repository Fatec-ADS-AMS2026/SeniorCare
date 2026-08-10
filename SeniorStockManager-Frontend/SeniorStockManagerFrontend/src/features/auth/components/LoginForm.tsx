import { Envelope, Lock, Eye, EyeSlash } from '@phosphor-icons/react';
import { FormEvent, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import authService from '../services/authService';
import useAuth from '@/hooks/useAuth';
import useAppRoutes from '@/hooks/useAppRoutes';

export default function LoginForm() {
  const [showPassword, setShowPassword] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const routes = useAppRoutes();

  const togglePasswordVisibility = () => {
    setShowPassword((prevState) => !prevState);
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);

    const res = await authService.login({ email, password });
    setIsSubmitting(false);

    if (!res.success || !res.data) {
      setError(res.message || 'Não foi possível entrar.');
      return;
    }

    const { status, identity, challengeToken } = res.data;

    if (status === 'ok' && identity) {
      login(identity);
      const from = (location.state as { from?: { pathname?: string } } | null)
        ?.from?.pathname;
      navigate(from || routes.ADMIN_OVERVIEW.path, { replace: true });
      return;
    }

    if (status === 'mfa_required' && challengeToken) {
      navigate(routes.MFA_CHALLENGE.path, { state: { challengeToken } });
      return;
    }

    if (status === 'mfa_enrollment_required' && challengeToken) {
      navigate(routes.MFA_ENROLL.path, { state: { challengeToken } });
      return;
    }

    setError('Resposta inesperada do servidor.');
  };

  return (
    <form
      className='flex flex-col justify-center items-center'
      onSubmit={handleSubmit}
    >
      <h1 className='font-bold text-4xl md:text-5xl mb-4 text-secondary'>
        Login
      </h1>
      {error && (
        <p className='mb-4 w-full max-w-md text-danger text-sm text-center'>
          {error}
        </p>
      )}
      <div className='flex flex-col w-full max-w-md mb-4'>
        <label
          htmlFor='email'
          className='text-lg font-semibold mb-2 tracking-wide text-textSecondary'
        >
          Email
        </label>
        <div className='flex items-center border border-textSecondary rounded'>
          <Envelope size={24} className='mx-2 text-textSecondary shrink-0' />
          <input
            id='email'
            type='email'
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            className='flex-1 h-12 px-4 focus:outline-none'
            placeholder='Digite seu email'
          />
        </div>
      </div>
      <div className='flex flex-col w-full max-w-md mb-2'>
        <label
          htmlFor='password'
          className='text-lg font-semibold mb-2 tracking-wide text-textSecondary'
        >
          Senha
        </label>
        <div className='flex items-center border border-textSecondary rounded'>
          <Lock size={24} className='mx-2 text-textSecondary shrink-0' />
          <input
            id='password'
            type={showPassword ? 'text' : 'password'}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            className='flex-1 h-12 px-4 focus:outline-none w-full'
            placeholder='Digite sua senha'
          />
          <button
            type='button'
            onClick={togglePasswordVisibility}
            className='text-textSecondary mx-2 shrink-0'
          >
            {showPassword ? <EyeSlash size={24} /> : <Eye size={24} />}
          </button>
        </div>
        <p
          onClick={() => navigate(routes.RECOVER_ACCOUNT.path)}
          className='mt-2 cursor-pointer hover:text-secondary transition-colors text-right text-textSecondary'
        >
          Esqueceu sua senha?
        </p>
      </div>
      <button
        type='submit'
        disabled={isSubmitting}
        className='bg-primary h-12 w-full max-w-md rounded text-neutralWhite font-semibold hover:bg-hoverButton transition-colors text-lg mt-5 disabled:opacity-60'
      >
        {isSubmitting ? 'Entrando...' : 'Entrar'}
      </button>
    </form>
  );
}
