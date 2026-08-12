import { FormEvent, useEffect, useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import authService from '../services/authService';
import { MfaEnrollResponse } from '../types';
import useAuth from '@/hooks/useAuth';

export default function MfaEnrollPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const { login, status: authStatus } = useAuth();

  // Sem challengeToken = cadastro voluntário por quem já tem sessão; com
  // challengeToken = cadastro obrigatório no meio do login (ainda sem sessão).
  const challengeToken = (location.state as { challengeToken?: string } | null)
    ?.challengeToken;

  const [enrollment, setEnrollment] = useState<MfaEnrollResponse | null>(null);
  const [code, setCode] = useState('');
  const [recoveryCodes, setRecoveryCodes] = useState<string[] | null>(null);
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      const res = await authService.mfaEnroll({ challengeToken });
      if (cancelled) return;
      if (res.success && res.data) {
        setEnrollment(res.data);
      } else {
        setError(res.message || 'Não foi possível iniciar o cadastro de MFA.');
      }
      setIsLoading(false);
    })();
    return () => {
      cancelled = true;
    };
  }, [challengeToken]);

  if (!challengeToken && authStatus !== 'authenticated') {
    return <Navigate to='/login' replace />;
  }

  const handleConfirm = async (e: FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError('');
    const res = await authService.mfaConfirm({ challengeToken, code });
    setIsSubmitting(false);

    if (!res.success || !res.data) {
      setError(res.message || 'Código inválido.');
      return;
    }

    setRecoveryCodes(res.data.recoveryCodes);
    if (res.data.identity) {
      login(res.data.identity);
    }
  };

  const handleDone = () => navigate('/', { replace: true });

  if (recoveryCodes) {
    return (
      <div className='flex flex-col items-center justify-center h-screen w-full py-16'>
        <div className='w-full max-w-md bg-neutralWhite p-8 rounded-lg shadow-md flex flex-col gap-4'>
          <h1 className='text-2xl font-bold text-secondary'>
            Guarde seus códigos de recuperação
          </h1>
          <p className='text-textSecondary text-sm'>
            Cada código só pode ser usado uma vez e não será mostrado
            novamente.
          </p>
          <ul className='font-mono text-sm bg-neutralLighter p-4 rounded grid grid-cols-2 gap-2'>
            {recoveryCodes.map((recoveryCode) => (
              <li key={recoveryCode}>{recoveryCode}</li>
            ))}
          </ul>
          <button
            type='button'
            onClick={handleDone}
            className='bg-primary h-12 w-full rounded text-neutralWhite font-semibold hover:bg-hoverButton transition-colors'
          >
            Concluir
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className='flex flex-col items-center justify-center h-screen w-full py-16'>
      <form
        onSubmit={handleConfirm}
        className='w-full max-w-md bg-neutralWhite p-8 rounded-lg shadow-md flex flex-col gap-4'
      >
        <h1 className='text-2xl font-bold text-secondary'>
          Ativar verificação em duas etapas
        </h1>
        {isLoading && <p className='text-textSecondary text-sm'>Carregando...</p>}
        {enrollment && (
          <>
            <p className='text-textSecondary text-sm'>
              Escaneie o código abaixo no seu aplicativo autenticador ou
              informe a chave manualmente.
            </p>
            <p className='font-mono text-xs break-all bg-neutralLighter p-2 rounded'>
              {enrollment.otpAuthUri}
            </p>
            <p className='font-mono text-sm'>{enrollment.authenticatorKey}</p>
          </>
        )}
        {error && (
          <p className='text-danger text-sm' role='alert'>
            {error}
          </p>
        )}
        <div className='flex flex-col'>
          <label htmlFor='code' className='text-sm font-semibold mb-1 text-textSecondary'>
            Código de confirmação
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
          disabled={isSubmitting || !enrollment}
          className='bg-primary h-12 w-full rounded text-neutralWhite font-semibold hover:bg-hoverButton transition-colors disabled:opacity-60'
        >
          {isSubmitting ? 'Confirmando...' : 'Confirmar'}
        </button>
      </form>
    </div>
  );
}
