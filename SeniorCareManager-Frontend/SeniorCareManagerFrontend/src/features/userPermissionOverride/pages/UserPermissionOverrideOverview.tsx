import { useCallback, useEffect, useState } from 'react';
import userPermissionOverrideService, {
  UserPermissionOverrideCreateRequest,
} from '../services/userPermissionOverrideService';
import adminUserService from '@/features/adminUser/services/adminUserService';
import UserPermissionOverride, {
  AccessEffect,
} from '@/types/models/UserPermissionOverride';
import Table from '@/components/Table';
import { TableColumn } from '@/components/Table/types';
import { Plus, Prohibit } from '@phosphor-icons/react';
import BreadcrumbPageTitle from '@/components/BreadcrumbPageTitle';
import Button from '@/components/Button';
import { AlertModal, ConfirmModal } from '@/components/Modal';
import UserPermissionOverrideFormModal from '../components/UserPermissionOverrideFormModal';

const EFFECT_LABELS: Record<AccessEffect, string> = {
  [AccessEffect.ALLOW]: 'Permitir',
  [AccessEffect.DENY]: 'Negar',
};

const formatDate = (value: unknown) =>
  value ? new Date(value as string).toLocaleDateString('pt-BR') : '—';

interface OverrideRow extends UserPermissionOverride {
  userName: string;
  permissionLabel: string;
}

export default function UserPermissionOverrideOverview() {
  const columns: TableColumn<OverrideRow>[] = [
    { label: 'Usuário', attribute: 'userName' },
    { label: 'Permissão', attribute: 'permissionLabel' },
    {
      label: 'Efeito',
      attribute: 'effect',
      render: (value) => EFFECT_LABELS[value as AccessEffect],
    },
    { label: 'Justificativa', attribute: 'justification', render: (v) => (v as string) || '—' },
    { label: 'Válido de', attribute: 'validFrom', render: formatDate },
    { label: 'Válido até', attribute: 'validTo', render: formatDate },
  ];

  const [data, setData] = useState<OverrideRow[]>([]);
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [isRevokeModalOpen, setIsRevokeModalOpen] = useState(false);
  const [currentId, setCurrentId] = useState<string | null>(null);
  const [isAlertModalOpen, setIsAlertModalOpen] = useState(false);
  const [alertMessage, setAlertMessage] = useState('');
  const [alertType, setAlertType] = useState<'info' | 'success' | 'error'>(
    'info'
  );

  const showAlert = (message: string, type: 'info' | 'success' | 'error') => {
    setAlertMessage(message);
    setAlertType(type);
    setIsAlertModalOpen(true);
  };

  const fetchData = useCallback(async () => {
    const [overridesRes, usersRes] = await Promise.all([
      userPermissionOverrideService.getAll(),
      adminUserService.getAll(),
    ]);

    if (!overridesRes.success || !overridesRes.data) {
      showAlert(overridesRes.message, 'error');
      return;
    }

    const userNames = new Map(
      (usersRes.data ?? []).map((u) => [u.id, u.displayName])
    );

    setData(
      overridesRes.data.map((override) => ({
        ...override,
        userName: userNames.get(override.userId) ?? override.userId,
        permissionLabel: override.feature
          ? `${override.resource}.${override.action}.${override.feature}`
          : `${override.resource}.${override.action}`,
      }))
    );
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleCreate = async (formData: UserPermissionOverrideCreateRequest) => {
    const res = await userPermissionOverrideService.create(formData);
    if (res.success) {
      await fetchData();
      showAlert('Exceção de permissão criada com sucesso!', 'success');
    } else {
      showAlert(res.message || 'Erro inesperado ao criar a exceção.', 'error');
      throw new Error(res.message);
    }
  };

  const openRevokeModal = (id: string) => {
    setCurrentId(id);
    setIsRevokeModalOpen(true);
  };

  const revokeOverride = async () => {
    if (!currentId) return;
    const res = await userPermissionOverrideService.revoke(currentId);
    if (res.success) {
      setIsRevokeModalOpen(false);
      setCurrentId(null);
      await fetchData();
      showAlert('Exceção revogada com sucesso!', 'success');
    } else {
      showAlert(res.message || 'Erro inesperado ao revogar a exceção.', 'error');
    }
  };

  const Actions = ({ id }: { id: string }) => (
    <button
      onClick={() => openRevokeModal(id)}
      className='text-danger hover:text-hoverDanger'
      title='Revogar exceção'
    >
      <Prohibit className='size-6' weight='fill' />
    </button>
  );

  return (
    <div>
      <BreadcrumbPageTitle title='Exceções de Permissão' />
      <div className='bg-neutralWhite px-6 py-6 max-w-[95%] mx-auto rounded-lg shadow-md mt-10'>
        <UserPermissionOverrideFormModal
          isOpen={isFormModalOpen}
          onClose={() => setIsFormModalOpen(false)}
          onSubmit={handleCreate}
        />
        <ConfirmModal
          isOpen={isRevokeModalOpen}
          onClose={() => setIsRevokeModalOpen(false)}
          onConfirm={revokeOverride}
          title='Revogar exceção de permissão?'
          message='A validade desta exceção será encerrada agora.'
        />
        <AlertModal
          isOpen={isAlertModalOpen}
          onClose={() => setIsAlertModalOpen(false)}
          message={alertMessage}
          type={alertType}
        />
        <div className='flex items-center justify-end mb-4'>
          <Button
            label='Criar exceção'
            icon={<Plus />}
            iconPosition='left'
            color='success'
            size='medium'
            onClick={() => setIsFormModalOpen(true)}
          />
        </div>
        <Table<OverrideRow>
          columns={columns}
          data={data}
          actions={(id) => <Actions id={id} />}
        />
      </div>
    </div>
  );
}
