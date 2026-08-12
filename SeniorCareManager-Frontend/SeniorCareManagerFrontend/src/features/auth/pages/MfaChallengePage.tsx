import { FormEvent, useState } from 'react';
import { Navigate, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { TextInput } from '@/components/FormControls';
import Button from '@/components/Button';
import { AlertModal } from '@/components/Modal';
import authService from '../services/authService';
import useAuth from '@/hooks/useAuth';
import useAppRoutes from '@/hooks/useAppRoutes';
import { resolveReturnPath } from '@/utils/returnPath';

interface MfaChallengeFormData {
  code: string;
}

export default function MfaChallengePage() {
  const location = useLocation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const routes = useAppRoutes();
  const { login } = useAuth();

  // §6.4 — mesma prioridade de LoginForm/index.tsx: returnTo cruzado exige
  // navegação de página inteira (apps separados, sem module federation).
  const navigateToDestination = () => {
    const crossAppReturnTo = resolveReturnPath(searchParams.get('returnTo'));
    if (crossAppReturnTo) {
      window.location.assign(crossAppReturnTo);
      return;
    }
    navigate(routes.ADMIN_OVERVIEW.path, { replace: true });
  };

  const challengeToken = (
    location.state as { challengeToken?: string } | null
  )?.challengeToken;

  const [code, setCode] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isAlertOpen, setIsAlertOpen] = useState(false);
  const [alertMessage, setAlertMessage] = useState('');
  const [isLowRecoveryCodes, setIsLowRecoveryCodes] = useState(false);

  // §7.7: o desafio só existe durante o login em andamento — sem ele (ex.: reload
  // direto nesta URL), não há como confirmar, volta pro login.
  if (!challengeToken) {
    return <Navigate to={routes.LOGIN.path} replace />;
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    const res = await authService.loginMfa({ challengeToken, code });
    setIsSubmitting(false);

    if (res.success && res.data?.status === 'ok' && res.data.identity) {
      login(res.data.identity);
      const remaining = res.data.remainingRecoveryCodes;
      if (remaining !== undefined && remaining !== null && remaining <= 2) {
        setIsLowRecoveryCodes(true);
        setAlertMessage(
          `Restam apenas ${remaining} código(s) de recuperação. Considere gerar novos códigos.`
        );
        setIsAlertOpen(true);
        return;
      }
      navigateToDestination();
      return;
    }

    setIsLowRecoveryCodes(false);
    setAlertMessage(res.message || 'Código inválido.');
    setIsAlertOpen(true);
  };

  const closeAlert = () => {
    setIsAlertOpen(false);
    if (isLowRecoveryCodes) {
      navigateToDestination();
    }
  };

  return (
    <div className='flex flex-col items-center justify-center h-full py-16'>
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
        <TextInput<MfaChallengeFormData>
          label='Código'
          name='code'
          value={code}
          onChange={(_, value) => setCode(value)}
          required
        />
        <Button
          type='submit'
          label={isSubmitting ? 'Verificando...' : 'Confirmar'}
          disabled={isSubmitting}
        />
      </form>
      <AlertModal
        isOpen={isAlertOpen}
        onClose={closeAlert}
        message={alertMessage}
        type={isLowRecoveryCodes ? 'info' : 'error'}
      />
    </div>
  );
}
