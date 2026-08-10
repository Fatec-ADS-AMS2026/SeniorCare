import { useEffect, useState } from 'react';
import { SelectInput, TextInput } from '@/components/FormControls';
import { ModalProps, FormModal } from '@/components/Modal';
import useFormData from '@/hooks/useFormData';
import { AccessEffect } from '@/types/models/UserPermissionOverride';
import { AccessScopeType } from '@/types/models/OrganizationalRoleAssignment';
import adminUserService from '@/features/adminUser/services/adminUserService';
import permissionService from '@/features/permission/services/permissionService';
import AdminUser from '@/types/models/AdminUser';
import Permission from '@/types/models/Permission';
import { UserPermissionOverrideCreateRequest } from '../services/userPermissionOverrideService';

interface OverrideFormData {
  userId: string;
  permissionId: string;
  scopeType: string;
  scopeKey: string;
  effect: string;
  justification: string;
  validFrom: string;
  validTo: string;
}

interface UserPermissionOverrideFormModalProps
  extends Omit<ModalProps, 'children'> {
  onSubmit: (data: UserPermissionOverrideCreateRequest) => Promise<void>;
}

const SCOPE_OPTIONS = [
  { label: 'Nenhum (sem escopo)', value: '' },
  { label: 'Instituição', value: AccessScopeType.INSTITUTION },
  { label: 'Unidade', value: AccessScopeType.UNIT },
  { label: 'Setor', value: AccessScopeType.SECTOR },
];

const EFFECT_OPTIONS = [
  { label: 'Permitir (allow)', value: AccessEffect.ALLOW },
  { label: 'Negar (deny)', value: AccessEffect.DENY },
];

const emptyData: OverrideFormData = {
  userId: '',
  permissionId: '',
  scopeType: '',
  scopeKey: '',
  effect: String(AccessEffect.ALLOW),
  justification: '',
  validFrom: new Date().toISOString().slice(0, 10),
  validTo: '',
};

export default function UserPermissionOverrideFormModal({
  onClose,
  onSubmit,
  isOpen,
}: UserPermissionOverrideFormModalProps) {
  const { data, updateField, reset } = useFormData<OverrideFormData>(emptyData);
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [permissions, setPermissions] = useState<Permission[]>([]);

  useEffect(() => {
    if (!isOpen) {
      reset();
      return;
    }
    (async () => {
      const [usersRes, permissionsRes] = await Promise.all([
        adminUserService.getAll(),
        permissionService.getAll(),
      ]);
      if (usersRes.success && usersRes.data) setUsers(usersRes.data);
      if (permissionsRes.success && permissionsRes.data) {
        setPermissions(permissionsRes.data);
      }
    })();
  }, [isOpen, reset]);

  // §6.6: exceção permanente (sem ValidTo) exige justificativa não trivial — o
  // backend valida de verdade (ValidateOverrideJustification), aqui é só um
  // guard-rail cedo pra não deixar o usuário submeter algo que sabe que vai falhar.
  const justificationRequired = !data.validTo;

  const handleSubmit = async () => {
    const permission = permissions.find((p) => p.id === data.permissionId);
    if (!permission) return;

    await onSubmit({
      userId: data.userId,
      resource: permission.resource,
      action: permission.action,
      feature: permission.feature,
      scopeType: data.scopeType ? (Number(data.scopeType) as AccessScopeType) : undefined,
      scopeKey: data.scopeKey || undefined,
      effect: Number(data.effect) as AccessEffect,
      justification: data.justification || undefined,
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
      title='Criar Exceção de Permissão'
    >
      <div className='flex flex-col gap-4'>
        <SelectInput<OverrideFormData>
          name='userId'
          label='Usuário'
          onChange={updateField}
          value={data.userId}
          options={users.map((u) => ({ label: u.displayName, value: u.id }))}
          required
        />
        <SelectInput<OverrideFormData>
          name='permissionId'
          label='Permissão'
          onChange={updateField}
          value={data.permissionId}
          options={permissions.map((p) => ({
            label: p.feature
              ? `${p.resource}.${p.action}.${p.feature}`
              : `${p.resource}.${p.action}`,
            value: p.id,
          }))}
          required
        />
        <SelectInput<OverrideFormData>
          name='effect'
          label='Efeito'
          onChange={updateField}
          value={data.effect}
          options={EFFECT_OPTIONS}
          required
        />
        <SelectInput<OverrideFormData>
          name='scopeType'
          label='Tipo de escopo'
          onChange={updateField}
          value={data.scopeType}
          options={SCOPE_OPTIONS}
        />
        <TextInput<OverrideFormData>
          name='scopeKey'
          label='Identificador do escopo (opcional)'
          onChange={updateField}
          value={data.scopeKey}
        />
        <TextInput<OverrideFormData>
          name='validFrom'
          label='Válido a partir de'
          type='date'
          onChange={updateField}
          value={data.validFrom}
          required
        />
        <TextInput<OverrideFormData>
          name='validTo'
          label='Válido até (opcional — deixe em branco para permanente)'
          type='date'
          onChange={updateField}
          value={data.validTo}
        />
        <TextInput<OverrideFormData>
          name='justification'
          label={
            justificationRequired
              ? 'Justificativa (obrigatória para exceção permanente)'
              : 'Justificativa (opcional)'
          }
          onChange={updateField}
          value={data.justification}
          required={justificationRequired}
        />
      </div>
    </FormModal>
  );
}
