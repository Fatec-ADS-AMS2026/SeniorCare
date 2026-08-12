import { Envelope, Eye, EyeSlash, Lock } from '@phosphor-icons/react';
import { FormEvent, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import authService from '../../services/authService';
import useAuth from '@/hooks/useAuth';
import { resolveReturnPath } from '@/utils/returnPath';

export default function LoginForm() {
  const [showPassword, setShowPassword] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

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
      // §4.5 — returnTo (contrato: parâmetro de query da própria URL de login,
      // não estado de navegação) só é confiado depois de validado.
      const destination = resolveReturnPath(searchParams.get('returnTo'));
      navigate(destination, { replace: true });
      return;
    }

    if (status === 'mfa_required' && challengeToken) {
      navigate('/login/mfa', { state: { challengeToken } });
      return;
    }

    if (status === 'mfa_enrollment_required' && challengeToken) {
      navigate('/mfa/enroll', { state: { challengeToken } });
      return;
    }

    setError('Resposta inesperada do servidor.');
  };

  return (
    <form
      className='flex flex-col justify-center items-center'
      onSubmit={handleSubmit}
    >
      <h3 className='font-bold text-4xl md:text-5xl mb-4 text-secondary'>
        Login
      </h3>
      {error && (
        <p className='mb-4 w-full max-w-md text-danger text-sm text-center' role='alert'>
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
            aria-label={showPassword ? 'Ocultar senha' : 'Mostrar senha'}
          >
            {showPassword ? <EyeSlash size={24} /> : <Eye size={24} />}
          </button>
        </div>
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
