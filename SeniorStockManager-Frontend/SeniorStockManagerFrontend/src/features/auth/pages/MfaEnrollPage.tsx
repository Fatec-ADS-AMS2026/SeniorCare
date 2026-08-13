import { FormEvent, useEffect, useState } from 'react';
import { Navigate, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { TextInput } from '@/components/FormControls';
import Button from '@/components/Button';
import authService from '../services/authService';
import { MfaEnrollResponse } from '../types';
import useAuth from '@/hooks/useAuth';
import useAppRoutes from '@/hooks/useAppRoutes';
import { resolveReturnPath } from '@/utils/returnPath';

interface MfaConfirmFormData {
  code: string;
}

export default function MfaEnrollPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const routes = useAppRoutes();
  const { login, status: authStatus } = useAuth();

  // Sem ChallengeToken = cadastro voluntário por quem já tem sessão (§7.7); com
  // ChallengeToken = cadastro obrigatório no meio do login (ainda sem sessão).
  const challengeToken = (
    location.state as { challengeToken?: string } | null
  )?.challengeToken;

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
    return <Navigate to={routes.LOGIN.path} replace />;
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

  // §7.4 — mesma prioridade de LoginForm.tsx: returnTo cruzado exige
  // navegação de página inteira (apps separados, sem module federation).
  const handleDone = () => {
    const crossAppReturnTo = resolveReturnPath(searchParams.get('returnTo'));
    if (crossAppReturnTo) {
      window.location.assign(crossAppReturnTo);
      return;
    }
    navigate(routes.ADMIN_OVERVIEW.path, { replace: true });
  };

  if (recoveryCodes) {
    return (
      <div className='flex flex-col items-center justify-center h-full py-16'>
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
          <Button label='Concluir' onClick={handleDone} />
        </div>
      </div>
    );
  }

  return (
    <div className='flex flex-col items-center justify-center h-full py-16'>
      <form
        onSubmit={handleConfirm}
        className='w-full max-w-md bg-neutralWhite p-8 rounded-lg shadow-md flex flex-col gap-4'
      >
        <h1 className='text-2xl font-bold text-secondary'>
          Ativar verificação em duas etapas
        </h1>
        {isLoading && (
          <p className='text-textSecondary text-sm'>Carregando...</p>
        )}
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
        {error && <p className='text-danger text-sm'>{error}</p>}
        <TextInput<MfaConfirmFormData>
          label='Código de confirmação'
          name='code'
          value={code}
          onChange={(_, value) => setCode(value)}
          required
        />
        <Button
          type='submit'
          label={isSubmitting ? 'Confirmando...' : 'Confirmar'}
          disabled={isSubmitting || !enrollment}
        />
      </form>
    </div>
  );
}
