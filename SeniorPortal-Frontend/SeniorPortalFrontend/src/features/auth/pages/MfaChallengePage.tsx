import { FormEvent, useState } from 'react';
import { Navigate, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import authService from '../services/authService';
import useAuth from '@/hooks/useAuth';
import { resolveReturnPath } from '@/utils/returnPath';

export default function MfaChallengePage() {
  const location = useLocation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { login } = useAuth();

  const challengeToken = (location.state as { challengeToken?: string } | null)
    ?.challengeToken;

  const [code, setCode] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');

  // O desafio só existe durante o login em andamento — sem ele (ex.: reload
  // direto nesta URL), não há como confirmar, volta pro login.
  if (!challengeToken) {
    return <Navigate to='/login' replace />;
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);
    const res = await authService.loginMfa({ challengeToken, code });
    setIsSubmitting(false);

    if (res.success && res.data?.status === 'ok' && res.data.identity) {
      login(res.data.identity);
      const destination = resolveReturnPath(searchParams.get('returnTo'));
      navigate(destination, { replace: true });
      return;
    }

    setError(res.message || 'Código inválido.');
  };

  return (
    <div className='flex flex-col items-center justify-center h-screen w-full py-16'>
      <form
        onSubmit={handleSubmit}
        className='w-full max-w-md bg-neutralWhite p-8 rounded-lg shadow-md flex flex-col gap-4'
      >
        <h1 className='text-2xl font-bold text-secondary'>
          Verificação em duas etapas
        </h1>
        <p className='text-textSecondary text-sm'>
          Informe o código do seu aplicativo autenticador ou um código de
          recuperação.
        </p>
        {error && (
          <p className='text-danger text-sm' role='alert'>
            {error}
          </p>
        )}
        <div className='flex flex-col'>
          <label htmlFor='code' className='text-sm font-semibold mb-1 text-textSecondary'>
            Código
          </label>
          <input
            id='code'
            name='code'
            type='text'
            value={code}
            onChange={(e) => setCode(e.target.value)}
            required
            className='h-12 px-4 border border-textSecondary rounded focus:outline-none'
          />
        </div>
        <button
          type='submit'
          disabled={isSubmitting}
          className='bg-primary h-12 w-full rounded text-neutralWhite font-semibold hover:bg-hoverButton transition-colors disabled:opacity-60'
        >
          {isSubmitting ? 'Verificando...' : 'Confirmar'}
        </button>
      </form>
    </div>
  );
}
