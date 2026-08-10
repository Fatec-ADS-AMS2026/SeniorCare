import { useCallback, useEffect, useState } from 'react';
import accessPolicyService, {
  AccessPolicyUpsertRequest,
} from '../services/accessPolicyService';
import AccessPolicy, { AccessPolicyState } from '@/types/models/AccessPolicy';
import { AccessEffect } from '@/types/models/UserPermissionOverride';
import Table from '@/components/Table';
import { TableColumn } from '@/components/Table/types';
import { CheckCircle, PencilSimple, Plus, XCircle } from '@phosphor-icons/react';
import BreadcrumbPageTitle from '@/components/BreadcrumbPageTitle';
import Button from '@/components/Button';
import { AlertModal } from '@/components/Modal';
import AccessPolicyFormModal from '../components/AccessPolicyFormModal';

const STATE_LABELS: Record<AccessPolicyState, string> = {
  [AccessPolicyState.DRAFT]: 'Rascunho',
  [AccessPolicyState.ACTIVE]: 'Ativa',
  [AccessPolicyState.RETIRED]: 'Retirada',
};

const EFFECT_LABELS: Record<AccessEffect, string> = {
  [AccessEffect.ALLOW]: 'Permitir',
  [AccessEffect.DENY]: 'Negar',
};

interface PolicyRow extends AccessPolicy {
  permissionLabel: string;
}

export default function AccessPolicyOverview() {
  const columns: TableColumn<PolicyRow>[] = [
    { label: 'Permissão', attribute: 'permissionLabel' },
    { label: 'Versão', attribute: 'version' },
    {
      label: 'Efeito',
      attribute: 'effect',
      render: (value) => EFFECT_LABELS[value as AccessEffect],
    },
    {
      label: 'Estado',
      attribute: 'state',
      render: (value) => STATE_LABELS[value as AccessPolicyState],
    },
  ];

  const [data, setData] = useState<PolicyRow[]>([]);
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [revisingFrom, setRevisingFrom] = useState<AccessPolicy | undefined>();
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
    const res = await accessPolicyService.getAll();
    if (res.success && res.data) {
      setData(
        [...res.data]
          .sort((a, b) => a.resource.localeCompare(b.resource) || b.version - a.version)
          .map((policy) => ({
            ...policy,
            permissionLabel: policy.feature
              ? `${policy.resource}.${policy.action}.${policy.feature}`
              : `${policy.resource}.${policy.action}`,
          }))
      );
    } else {
      showAlert(res.message, 'error');
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const openCreateModal = () => {
    setRevisingFrom(undefined);
    setIsFormModalOpen(true);
  };

  const openReviseModal = (id: string) => {
    const item = data.find((row) => row.id === id);
    if (item) {
      setRevisingFrom(item);
      setIsFormModalOpen(true);
    }
  };

  const handleSubmit = async (formData: AccessPolicyUpsertRequest) => {
    const res = revisingFrom
      ? await accessPolicyService.revise(revisingFrom.id, formData)
      : await accessPolicyService.create(formData);

    if (res.success) {
      await fetchData();
      showAlert(
        revisingFrom ? 'Nova versão criada com sucesso!' : 'Política criada com sucesso!',
        'success'
      );
    } else {
      showAlert(res.message || 'Erro inesperado ao salvar a política.', 'error');
      throw new Error(res.message);
    }
  };

  const handleActivate = async (id: string) => {
    const res = await accessPolicyService.activate(id);
    if (res.success) {
      await fetchData();
      showAlert('Política ativada com sucesso!', 'success');
    } else {
      showAlert(res.message || 'Erro inesperado ao ativar a política.', 'error');
    }
  };

  const handleRetire = async (id: string) => {
    const res = await accessPolicyService.retire(id);
    if (res.success) {
      await fetchData();
      showAlert('Política retirada com sucesso!', 'success');
    } else {
      showAlert(res.message || 'Erro inesperado ao retirar a política.', 'error');
    }
  };

  const Actions = ({ id }: { id: string }) => {
    const item = data.find((row) => row.id === id);
    return (
      <>
        <button
          onClick={() => openReviseModal(id)}
          className='text-edit hover:text-hoverEdit'
          title='Revisar (nova versão)'
        >
          <PencilSimple className='size-6' weight='fill' />
        </button>
        {item?.state === AccessPolicyState.DRAFT && (
          <button
            onClick={() => handleActivate(id)}
            className='text-success hover:text-hoverSuccess'
            title='Ativar'
          >
            <CheckCircle className='size-6' weight='fill' />
          </button>
        )}
        {item?.state === AccessPolicyState.ACTIVE && (
          <button
            onClick={() => handleRetire(id)}
            className='text-danger hover:text-hoverDanger'
            title='Retirar'
          >
            <XCircle className='size-6' weight='fill' />
          </button>
        )}
      </>
    );
  };

  return (
    <div>
      <BreadcrumbPageTitle title='Políticas de Acesso' />
      <div className='bg-neutralWhite px-6 py-6 max-w-[95%] mx-auto rounded-lg shadow-md mt-10'>
        <AccessPolicyFormModal
          isOpen={isFormModalOpen}
          onClose={() => setIsFormModalOpen(false)}
          onSubmit={handleSubmit}
          revisingFrom={revisingFrom}
        />
        <AlertModal
          isOpen={isAlertModalOpen}
          onClose={() => setIsAlertModalOpen(false)}
          message={alertMessage}
          type={alertType}
        />
        <div className='flex items-center justify-end mb-4'>
          <Button
            label='Criar política'
            icon={<Plus />}
            iconPosition='left'
            color='success'
            size='medium'
            onClick={openCreateModal}
          />
        </div>
        <Table<PolicyRow>
          columns={columns}
          data={data}
          actions={(id) => <Actions id={id} />}
        />
      </div>
    </div>
  );
}
