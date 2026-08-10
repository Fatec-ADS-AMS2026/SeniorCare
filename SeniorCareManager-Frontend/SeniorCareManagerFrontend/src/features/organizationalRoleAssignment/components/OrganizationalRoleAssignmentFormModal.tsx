import { useEffect, useState } from 'react';
import { SelectInput, TextInput } from '@/components/FormControls';
import { ModalProps, FormModal } from '@/components/Modal';
import useFormData from '@/hooks/useFormData';
import { AccessScopeType } from '@/types/models/OrganizationalRoleAssignment';
import adminUserService from '@/features/adminUser/services/adminUserService';
import organizationalRoleService from '@/features/organizationalRole/services/organizationalRoleService';
import AdminUser from '@/types/models/AdminUser';
import OrganizationalRole from '@/types/models/OrganizationalRole';

interface AssignmentFormData {
  userId: string;
  organizationalRoleId: string;
  scopeType: string;
  scopeKey: string;
  validFrom: string;
  validTo: string;
}

interface OrganizationalRoleAssignmentFormModalProps
  extends Omit<ModalProps, 'children'> {
  onSubmit: (data: {
    userId: string;
    organizationalRoleId: string;
    scopeType: AccessScopeType;
    scopeKey?: string;
    validFrom: string;
    validTo?: string;
  }) => Promise<void>;
}

const SCOPE_OPTIONS = [
  { label: 'Instituição', value: AccessScopeType.INSTITUTION },
  { label: 'Unidade', value: AccessScopeType.UNIT },
  { label: 'Setor', value: AccessScopeType.SECTOR },
];

const emptyData: AssignmentFormData = {
  userId: '',
  organizationalRoleId: '',
  scopeType: String(AccessScopeType.INSTITUTION),
  scopeKey: '',
  validFrom: new Date().toISOString().slice(0, 10),
  validTo: '',
};

export default function OrganizationalRoleAssignmentFormModal({
  onClose,
  onSubmit,
  isOpen,
}: OrganizationalRoleAssignmentFormModalProps) {
  const { data, updateField, reset } = useFormData<AssignmentFormData>(emptyData);
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [organizationalRoles, setOrganizationalRoles] = useState<
    OrganizationalRole[]
  >([]);

  useEffect(() => {
    if (!isOpen) {
      reset();
      return;
    }
    (async () => {
      const [usersRes, rolesRes] = await Promise.all([
        adminUserService.getAll(),
        organizationalRoleService.getAll(),
      ]);
      if (usersRes.success && usersRes.data) setUsers(usersRes.data);
      if (rolesRes.success && rolesRes.data) setOrganizationalRoles(rolesRes.data);
    })();
  }, [isOpen, reset]);

  const handleSubmit = async () => {
    await onSubmit({
      userId: data.userId,
      organizationalRoleId: data.organizationalRoleId,
      scopeType: Number(data.scopeType) as AccessScopeType,
      scopeKey: data.scopeKey || undefined,
      validFrom: data.validFrom,
      validTo: data.validTo || undefined,
    });
    handleClose();
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <FormModal
      isOpen={isOpen}
      onClose={handleClose}
      onSubmit={handleSubmit}
      title='Atribuir Papel Organizacional'
    >
      <div className='flex flex-col gap-4'>
        <SelectInput<AssignmentFormData>
          name='userId'
          label='Usuário'
          onChange={updateField}
          value={data.userId}
          options={users.map((u) => ({ label: u.displayName, value: u.id }))}
          required
        />
        <SelectInput<AssignmentFormData>
          name='organizationalRoleId'
          label='Papel organizacional'
          onChange={updateField}
          value={data.organizationalRoleId}
          options={organizationalRoles.map((r) => ({ label: r.name, value: r.id }))}
          required
        />
        <SelectInput<AssignmentFormData>
          name='scopeType'
          label='Tipo de escopo'
          onChange={updateField}
          value={data.scopeType}
          options={SCOPE_OPTIONS}
          required
        />
        <TextInput<AssignmentFormData>
          name='scopeKey'
          label='Identificador do escopo (opcional)'
          onChange={updateField}
          value={data.scopeKey}
        />
        <TextInput<AssignmentFormData>
          name='validFrom'
          label='Válido a partir de'
          type='date'
          onChange={updateField}
          value={data.validFrom}
          required
        />
        <TextInput<AssignmentFormData>
          name='validTo'
          label='Válido até (opcional)'
          type='date'
          onChange={updateField}
          value={data.validTo}
        />
      </div>
    </FormModal>
  );
}
