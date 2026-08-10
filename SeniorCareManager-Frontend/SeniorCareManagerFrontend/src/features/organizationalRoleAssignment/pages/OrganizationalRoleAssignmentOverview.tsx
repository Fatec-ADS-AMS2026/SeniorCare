import { useCallback, useEffect, useState } from 'react';
import organizationalRoleAssignmentService from '../services/organizationalRoleAssignmentService';
import adminUserService from '@/features/adminUser/services/adminUserService';
import organizationalRoleService from '@/features/organizationalRole/services/organizationalRoleService';
import OrganizationalRoleAssignment, {
  AccessScopeType,
} from '@/types/models/OrganizationalRoleAssignment';
import Table from '@/components/Table';
import { TableColumn } from '@/components/Table/types';
import { Plus, Prohibit } from '@phosphor-icons/react';
import BreadcrumbPageTitle from '@/components/BreadcrumbPageTitle';
import Button from '@/components/Button';
import { AlertModal, ConfirmModal } from '@/components/Modal';
import OrganizationalRoleAssignmentFormModal from '../components/OrganizationalRoleAssignmentFormModal';

const SCOPE_LABELS: Record<AccessScopeType, string> = {
  [AccessScopeType.INSTITUTION]: 'Instituição',
  [AccessScopeType.UNIT]: 'Unidade',
  [AccessScopeType.SECTOR]: 'Setor',
};

const formatDate = (value: unknown) =>
  value ? new Date(value as string).toLocaleDateString('pt-BR') : '—';

interface AssignmentRow extends OrganizationalRoleAssignment {
  userName: string;
  roleName: string;
}

export default function OrganizationalRoleAssignmentOverview() {
  const columns: TableColumn<AssignmentRow>[] = [
    { label: 'Usuário', attribute: 'userName' },
    { label: 'Papel', attribute: 'roleName' },
    {
      label: 'Escopo',
      attribute: 'scopeType',
      render: (value) => SCOPE_LABELS[value as AccessScopeType],
    },
    { label: 'Identificador', attribute: 'scopeKey', render: (v) => (v as string) || '—' },
    { label: 'Válido de', attribute: 'validFrom', render: formatDate },
    { label: 'Válido até', attribute: 'validTo', render: formatDate },
  ];

  const [data, setData] = useState<AssignmentRow[]>([]);
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [isEndModalOpen, setIsEndModalOpen] = useState(false);
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
    const [assignmentsRes, usersRes, rolesRes] = await Promise.all([
      organizationalRoleAssignmentService.getAll(),
      adminUserService.getAll(),
      organizationalRoleService.getAll(),
    ]);

    if (!assignmentsRes.success || !assignmentsRes.data) {
      showAlert(assignmentsRes.message, 'error');
      return;
    }

    const userNames = new Map(
      (usersRes.data ?? []).map((u) => [u.id, u.displayName])
    );
    const roleNames = new Map((rolesRes.data ?? []).map((r) => [r.id, r.name]));

    setData(
      assignmentsRes.data.map((assignment) => ({
        ...assignment,
        userName: userNames.get(assignment.userId) ?? assignment.userId,
        roleName:
          roleNames.get(assignment.organizationalRoleId) ??
          assignment.organizationalRoleId,
      }))
    );
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleCreate = async (formData: {
    userId: string;
    organizationalRoleId: string;
    scopeType: AccessScopeType;
    scopeKey?: string;
    validFrom: string;
    validTo?: string;
  }) => {
    const res = await organizationalRoleAssignmentService.create(formData);
    if (res.success) {
      await fetchData();
      showAlert('Atribuição criada com sucesso!', 'success');
    } else {
      showAlert(res.message || 'Erro inesperado ao criar a atribuição.', 'error');
      throw new Error(res.message);
    }
  };

  const openEndModal = (id: string) => {
    setCurrentId(id);
    setIsEndModalOpen(true);
  };

  const endAssignment = async () => {
    if (!currentId) return;
    const res = await organizationalRoleAssignmentService.endEarly(currentId);
    if (res.success) {
      setIsEndModalOpen(false);
      setCurrentId(null);
      await fetchData();
      showAlert('Atribuição encerrada com sucesso!', 'success');
    } else {
      showAlert(res.message || 'Erro inesperado ao encerrar a atribuição.', 'error');
    }
  };

  const Actions = ({ id }: { id: string }) => (
    <button
      onClick={() => openEndModal(id)}
      className='text-danger hover:text-hoverDanger'
      title='Encerrar atribuição'
    >
      <Prohibit className='size-6' weight='fill' />
    </button>
  );

  return (
    <div>
      <BreadcrumbPageTitle title='Atribuições de Papel Organizacional' />
      <div className='bg-neutralWhite px-6 py-6 max-w-[95%] mx-auto rounded-lg shadow-md mt-10'>
        <OrganizationalRoleAssignmentFormModal
          isOpen={isFormModalOpen}
          onClose={() => setIsFormModalOpen(false)}
          onSubmit={handleCreate}
        />
        <ConfirmModal
          isOpen={isEndModalOpen}
          onClose={() => setIsEndModalOpen(false)}
          onConfirm={endAssignment}
          title='Encerrar atribuição?'
          message='A validade desta atribuição será encerrada agora — a pessoa deixa de ter esse papel organizacional a partir deste momento.'
        />
        <AlertModal
          isOpen={isAlertModalOpen}
          onClose={() => setIsAlertModalOpen(false)}
          message={alertMessage}
          type={alertType}
        />
        <div className='flex items-center justify-end mb-4'>
          <Button
            label='Atribuir'
            icon={<Plus />}
            iconPosition='left'
            color='success'
            size='medium'
            onClick={() => setIsFormModalOpen(true)}
          />
        </div>
        <Table<AssignmentRow>
          columns={columns}
          data={data}
          actions={(id) => <Actions id={id} />}
        />
      </div>
    </div>
  );
}
