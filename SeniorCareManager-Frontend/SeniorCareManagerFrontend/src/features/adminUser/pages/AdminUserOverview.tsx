import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import adminUserService from '../services/adminUserService';
import AdminUser, { AccountState } from '@/types/models/AdminUser';
import Table from '@/components/Table';
import { TableColumn } from '@/components/Table/types';
import { Pencil, Plus, UsersThree } from '@phosphor-icons/react';
import BreadcrumbPageTitle from '@/components/BreadcrumbPageTitle';
import SearchBar from '@/components/SearchBar';
import Button from '@/components/Button';
import { AlertModal } from '@/components/Modal';
import AdminUserFormModal from '../components/AdminUserFormModal';
import AdminUserStateModal from '../components/AdminUserStateModal';
import useAppRoutes from '@/hooks/useAppRoutes';

const STATE_LABELS: Record<AccountState, string> = {
  [AccountState.PROVISIONED]: 'Provisionada',
  [AccountState.ACTIVE]: 'Ativa',
  [AccountState.INACTIVE]: 'Inativa',
  [AccountState.BLOCKED]: 'Bloqueada',
  [AccountState.EXPIRED]: 'Expirada',
};

export default function AdminUserOverview() {
  const columns: TableColumn<AdminUser>[] = [
    { label: 'Nome', attribute: 'displayName' },
    { label: 'E-mail', attribute: 'email' },
    {
      label: 'Estado',
      attribute: 'accountState',
      render: (value) => STATE_LABELS[value as AccountState],
    },
  ];
  const navigate = useNavigate();
  const routes = useAppRoutes();

  const [data, setData] = useState<AdminUser[]>([]);
  const [originalData, setOriginalData] = useState<AdminUser[]>([]);
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [isStateModalOpen, setIsStateModalOpen] = useState(false);
  const [isAlertModalOpen, setIsAlertModalOpen] = useState(false);
  const [alertMessage, setAlertMessage] = useState('');
  const [alertType, setAlertType] = useState<'info' | 'success' | 'error'>(
    'info'
  );
  const [currentId, setCurrentId] = useState<string | null>(null);
  const [stateError, setStateError] = useState('');
  const [isStateSubmitting, setIsStateSubmitting] = useState(false);

  const fetchData = useCallback(async () => {
    const res = await adminUserService.getAll();
    if (res.success && res.data) {
      setData([...res.data]);
      setOriginalData([...res.data]);
    } else {
      showAlert(res.message, 'error');
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleSearch = (searchTerm: string) => {
    if (!searchTerm) {
      setData(originalData);
      return;
    }
    const filtered = originalData.filter(
      (u) =>
        u.displayName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        u.email.toLowerCase().includes(searchTerm.toLowerCase())
    );
    setData(filtered);
  };

  const showAlert = (message: string, type: 'info' | 'success' | 'error') => {
    setAlertMessage(message);
    setAlertType(type);
    setIsAlertModalOpen(true);
  };

  const handleCreate = async (formData: {
    email: string;
    displayName: string;
  }) => {
    const res = await adminUserService.create(formData);
    if (res.success && res.data) {
      await fetchData();
      // O token de ativação nunca chega até aqui — a API não o devolve (achado da
      // revisão do PR: platform-authentication proíbe token em resposta
      // administrativa). A entrega do link de ativação é um procedimento
      // operacional separado, fora do escopo desta tela.
      showAlert(
        `Usuário "${res.data.displayName}" criado com estado PROVISIONED. Siga o procedimento institucional para entregar a ativação com segurança.`,
        'success'
      );
    } else {
      showAlert(res.message || 'Erro inesperado ao criar o usuário.', 'error');
      throw new Error(res.message);
    }
  };

  const openStateModal = (id: string) => {
    setCurrentId(id);
    setStateError('');
    setIsStateModalOpen(true);
  };

  const handleStateChange = async (
    accountState: AccountState,
    currentPassword: string
  ) => {
    if (!currentId) return;
    setIsStateSubmitting(true);
    const res = await adminUserService.changeState(currentId, {
      accountState,
      currentPassword,
    });
    setIsStateSubmitting(false);
    if (res.success) {
      setIsStateModalOpen(false);
      await fetchData();
      showAlert('Estado da conta atualizado com sucesso!', 'success');
    } else {
      setStateError(res.message || 'Não foi possível alterar o estado da conta.');
    }
  };

  const currentUser = data.find((u) => u.id === currentId);

  const Actions = ({ id }: { id: string }) => (
    <>
      <button
        onClick={() => openStateModal(id)}
        className='text-edit hover:text-hoverEdit'
        title='Alterar estado'
      >
        <Pencil className='size-6' weight='fill' />
      </button>
      <button
        onClick={() =>
          navigate(routes.USER_SESSION.path.replace(':userId', id))
        }
        className='text-primary hover:text-secondary'
        title='Ver sessões'
      >
        <UsersThree className='size-6' weight='fill' />
      </button>
    </>
  );

  return (
    <div>
      <BreadcrumbPageTitle title='Usuários' />
      <div className='bg-neutralWhite px-6 py-6 max-w-[95%] mx-auto rounded-lg shadow-md mt-10'>
        <AdminUserFormModal
          isOpen={isFormModalOpen}
          onClose={() => setIsFormModalOpen(false)}
          onSubmit={handleCreate}
        />
        <AdminUserStateModal
          isOpen={isStateModalOpen}
          onClose={() => setIsStateModalOpen(false)}
          onConfirm={handleStateChange}
          currentState={currentUser?.accountState}
          isSubmitting={isStateSubmitting}
          error={stateError}
        />
        <AlertModal
          isOpen={isAlertModalOpen}
          onClose={() => setIsAlertModalOpen(false)}
          message={alertMessage}
          type={alertType}
        />
        <div className='flex items-center justify-between mb-4'>
          <SearchBar action={handleSearch} placeholder='Buscar usuário...' />
          <Button
            label='Adicionar'
            icon={<Plus />}
            iconPosition='left'
            color='success'
            size='medium'
            onClick={() => setIsFormModalOpen(true)}
          />
        </div>
        <Table<AdminUser>
          columns={columns}
          data={data}
          actions={(id) => <Actions id={id} />}
        />
      </div>
    </div>
  );
}
