import { useCallback, useEffect, useState } from 'react';
import institutionSecurityService from '../services/institutionSecurityService';
import InstitutionSecurityPolicy from '@/types/models/InstitutionSecurityPolicy';
import { Checkbox, TextInput } from '@/components/FormControls';
import BreadcrumbPageTitle from '@/components/BreadcrumbPageTitle';
import Button from '@/components/Button';
import { AlertModal, ReauthModal } from '@/components/Modal';

interface FormState {
  lockoutDurationMinutes: string;
  maxFailedAttempts: string;
  accessTokenDurationMinutes: string;
  refreshTokenDurationDays: string;
  mfaRequiredForAllUsers: boolean;
}

const toFormState = (policy: InstitutionSecurityPolicy): FormState => ({
  lockoutDurationMinutes: policy.lockoutDurationMinutes?.toString() ?? '',
  maxFailedAttempts: policy.maxFailedAttempts?.toString() ?? '',
  accessTokenDurationMinutes: policy.accessTokenDurationMinutes?.toString() ?? '',
  refreshTokenDurationDays: policy.refreshTokenDurationDays?.toString() ?? '',
  mfaRequiredForAllUsers: policy.mfaRequiredForAllUsers,
});

const toNumberOrUndefined = (value: string) =>
  value.trim() === '' ? undefined : Number(value);

export default function InstitutionSecurityPage() {
  const [form, setForm] = useState<FormState>({
    lockoutDurationMinutes: '',
    maxFailedAttempts: '',
    accessTokenDurationMinutes: '',
    refreshTokenDurationDays: '',
    mfaRequiredForAllUsers: false,
  });
  const [isLoading, setIsLoading] = useState(true);
  const [isReauthOpen, setIsReauthOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [reauthError, setReauthError] = useState('');
  const [isAlertOpen, setIsAlertOpen] = useState(false);
  const [alertMessage, setAlertMessage] = useState('');
  const [alertType, setAlertType] = useState<'info' | 'success' | 'error'>(
    'info'
  );

  const showAlert = (message: string, type: 'info' | 'success' | 'error') => {
    setAlertMessage(message);
    setAlertType(type);
    setIsAlertOpen(true);
  };

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    const res = await institutionSecurityService.get();
    setIsLoading(false);
    if (res.success && res.data) {
      setForm(toFormState(res.data));
    } else {
      showAlert(res.message, 'error');
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleConfirm = async (currentPassword: string) => {
    setIsSubmitting(true);
    const res = await institutionSecurityService.update({
      lockoutDurationMinutes: toNumberOrUndefined(form.lockoutDurationMinutes),
      maxFailedAttempts: toNumberOrUndefined(form.maxFailedAttempts),
      accessTokenDurationMinutes: toNumberOrUndefined(form.accessTokenDurationMinutes),
      refreshTokenDurationDays: toNumberOrUndefined(form.refreshTokenDurationDays),
      mfaRequiredForAllUsers: form.mfaRequiredForAllUsers,
      currentPassword,
    });
    setIsSubmitting(false);

    if (res.success && res.data) {
      setForm(toFormState(res.data));
      setIsReauthOpen(false);
      showAlert('Parâmetros de segurança atualizados com sucesso!', 'success');
    } else {
      setReauthError(res.message || 'Não foi possível salvar os parâmetros.');
    }
  };

  if (isLoading) {
    return (
      <div>
        <BreadcrumbPageTitle title='Parâmetros de Segurança' />
        <p className='px-6 py-6 text-textSecondary'>Carregando...</p>
      </div>
    );
  }

  return (
    <div>
      <BreadcrumbPageTitle title='Parâmetros de Segurança' />
      <div className='bg-neutralWhite px-6 py-6 max-w-[95%] mx-auto rounded-lg shadow-md mt-10 flex flex-col gap-4'>
        <TextInput<FormState>
          name='maxFailedAttempts'
          label='Máximo de tentativas de login com falha (padrão: 5)'
          type='number'
          value={form.maxFailedAttempts}
          onChange={(_, value) =>
            setForm((prev) => ({ ...prev, maxFailedAttempts: value }))
          }
        />
        <TextInput<FormState>
          name='lockoutDurationMinutes'
          label='Duração do bloqueio em minutos (padrão: 15)'
          type='number'
          value={form.lockoutDurationMinutes}
          onChange={(_, value) =>
            setForm((prev) => ({ ...prev, lockoutDurationMinutes: value }))
          }
        />
        <TextInput<FormState>
          name='accessTokenDurationMinutes'
          label='Duração do token de acesso em minutos'
          type='number'
          value={form.accessTokenDurationMinutes}
          onChange={(_, value) =>
            setForm((prev) => ({ ...prev, accessTokenDurationMinutes: value }))
          }
        />
        <TextInput<FormState>
          name='refreshTokenDurationDays'
          label='Duração do token de renovação em dias'
          type='number'
          value={form.refreshTokenDurationDays}
          onChange={(_, value) =>
            setForm((prev) => ({ ...prev, refreshTokenDurationDays: value }))
          }
        />
        <Checkbox<FormState>
          name='mfaRequiredForAllUsers'
          label='Exigir verificação em duas etapas para todos os usuários'
          checked={form.mfaRequiredForAllUsers}
          onChange={(_, value) =>
            setForm((prev) => ({ ...prev, mfaRequiredForAllUsers: value }))
          }
        />
        <div className='flex justify-end'>
          <Button
            label='Salvar'
            color='primary'
            size='medium'
            onClick={() => {
              setReauthError('');
              setIsReauthOpen(true);
            }}
          />
        </div>
      </div>
      <ReauthModal
        isOpen={isReauthOpen}
        onClose={() => setIsReauthOpen(false)}
        onConfirm={handleConfirm}
        message='Confirme sua senha para salvar os parâmetros de segurança da instituição.'
        isSubmitting={isSubmitting}
        error={reauthError}
      />
      <AlertModal
        isOpen={isAlertOpen}
        onClose={() => setIsAlertOpen(false)}
        message={alertMessage}
        type={alertType}
      />
    </div>
  );
}
