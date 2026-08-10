import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import userSessionService from '../services/userSessionService';
import UserSession from '@/types/models/UserSession';
import Table from '@/components/Table';
import { TableColumn } from '@/components/Table/types';
import { Prohibit } from '@phosphor-icons/react';
import BreadcrumbPageTitle from '@/components/BreadcrumbPageTitle';
import Button from '@/components/Button';
import { AlertModal, ReauthModal } from '@/components/Modal';

const formatDate = (value: unknown) =>
  value ? new Date(value as string).toLocaleString('pt-BR') : '—';

export default function UserSessionOverview() {
  const { userId } = useParams<{ userId: string }>();

  const columns: TableColumn<UserSession>[] = [
    { label: 'IP', attribute: 'ipAddress', render: (v) => (v as string) || '—' },
    { label: 'Dispositivo', attribute: 'userAgent', render: (v) => (v as string) || '—' },
    { label: 'Criada em', attribute: 'createdAtUtc', render: formatDate },
    { label: 'Última atividade', attribute: 'lastSeenAtUtc', render: formatDate },
    { label: 'Revogada em', attribute: 'revokedAtUtc', render: formatDate },
  ];

  const [data, setData] = useState<UserSession[]>([]);
  const [isReauthOpen, setIsReauthOpen] = useState(false);
  const [reauthMode, setReauthMode] = useState<'single' | 'all'>('single');
  const [currentId, setCurrentId] = useState<string | null>(null);
  const [reauthError, setReauthError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
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
    if (!userId) return;
    const res = await userSessionService.getAllForUser(userId);
    if (res.success && res.data) {
      setData(res.data);
    } else {
      showAlert(res.message, 'error');
    }
  }, [userId]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const openRevoke = (id: string) => {
    setReauthMode('single');
    setCurrentId(id);
    setReauthError('');
    setIsReauthOpen(true);
  };

  const openRevokeAll = () => {
    setReauthMode('all');
    setCurrentId(null);
    setReauthError('');
    setIsReauthOpen(true);
  };

  const handleConfirm = async (currentPassword: string) => {
    if (!userId) return;
    setIsSubmitting(true);
    const res =
      reauthMode === 'single' && currentId
        ? await userSessionService.revoke(currentId, { currentPassword })
        : await userSessionService.revokeAll(userId, { currentPassword });
    setIsSubmitting(false);
    if (res.success) {
      setIsReauthOpen(false);
      await fetchData();
      showAlert('Sessão(ões) revogada(s) com sucesso.', 'success');
    } else {
      setReauthError(res.message || 'Não foi possível revogar.');
    }
  };

  const Actions = ({ id }: { id: string }) => (
    <button
      onClick={() => openRevoke(id)}
      className='text-danger hover:text-hoverDanger'
      title='Revogar sessão'
    >
      <Prohibit className='size-6' weight='fill' />
    </button>
  );

  return (
    <div>
      <BreadcrumbPageTitle title='Sessões' />
      <div className='bg-neutralWhite px-6 py-6 max-w-[95%] mx-auto rounded-lg shadow-md mt-10'>
        <ReauthModal
          isOpen={isReauthOpen}
          onClose={() => setIsReauthOpen(false)}
          onConfirm={handleConfirm}
          message={
            reauthMode === 'single'
              ? 'Confirme sua senha para revogar esta sessão.'
              : 'Confirme sua senha para revogar TODAS as sessões deste usuário.'
          }
          isSubmitting={isSubmitting}
          error={reauthError}
        />
        <AlertModal
          isOpen={isAlertOpen}
          onClose={() => setIsAlertOpen(false)}
          message={alertMessage}
          type={alertType}
        />
        <div className='flex items-center justify-end mb-4'>
          <Button
            label='Revogar todas'
            color='danger'
            size='medium'
            onClick={openRevokeAll}
          />
        </div>
        <Table<UserSession>
          columns={columns}
          data={data}
          actions={(id) => <Actions id={id} />}
        />
      </div>
    </div>
  );
}
