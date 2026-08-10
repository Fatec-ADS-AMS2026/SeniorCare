import { useEffect, useState } from 'react';
import { SelectInput, TextInput } from '@/components/FormControls';
import { ModalProps, FormModal } from '@/components/Modal';
import useFormData from '@/hooks/useFormData';
import { AccessScopeType } from '@/types/models/OrganizationalRoleAssignment';
import { AccessEffect } from '@/types/models/UserPermissionOverride';
import AccessPolicy from '@/types/models/AccessPolicy';
import permissionService from '@/features/permission/services/permissionService';
import Permission from '@/types/models/Permission';
import { AccessPolicyUpsertRequest } from '../services/accessPolicyService';

interface PolicyFormData {
  permissionId: string;
  scopeType: string;
  scopeKey: string;
  effect: string;
}

interface AccessPolicyFormModalProps extends Omit<ModalProps, 'children'> {
  onSubmit: (data: AccessPolicyUpsertRequest) => Promise<void>;
  // Presente = "revisar" esta versão (pré-preenche os campos com o valor atual).
  revisingFrom?: AccessPolicy;
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

const emptyData: PolicyFormData = {
  permissionId: '',
  scopeType: '',
  scopeKey: '',
  effect: String(AccessEffect.ALLOW),
};

export default function AccessPolicyFormModal({
  onClose,
  onSubmit,
  isOpen,
  revisingFrom,
}: AccessPolicyFormModalProps) {
  const { data, setData, updateField, reset } = useFormData<PolicyFormData>(emptyData);
  const [permissions, setPermissions] = useState<Permission[]>([]);

  useEffect(() => {
    if (!isOpen) {
      reset();
      return;
    }
    (async () => {
      const res = await permissionService.getAll();
      if (res.success && res.data) {
        setPermissions(res.data);
        if (revisingFrom) {
          const matching = res.data.find(
            (p) =>
              p.resource === revisingFrom.resource &&
              p.action === revisingFrom.action &&
              p.feature === revisingFrom.feature
          );
          setData({
            permissionId: matching?.id ?? '',
            scopeType: revisingFrom.scopeType ? String(revisingFrom.scopeType) : '',
            scopeKey: revisingFrom.scopeKey ?? '',
            effect: String(revisingFrom.effect),
          });
        }
      }
    })();
  }, [isOpen, revisingFrom, reset, setData]);

  const handleSubmit = async () => {
    const permission = permissions.find((p) => p.id === data.permissionId);
    if (!permission) return;

    await onSubmit({
      resource: permission.resource,
      action: permission.action,
      feature: permission.feature,
      scopeType: data.scopeType ? (Number(data.scopeType) as AccessScopeType) : undefined,
      scopeKey: data.scopeKey || undefined,
      effect: Number(data.effect) as AccessEffect,
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
      title={revisingFrom ? 'Revisar Política de Acesso' : 'Criar Política de Acesso'}
    >
      <div className='flex flex-col gap-4'>
        <SelectInput<PolicyFormData>
          name='permissionId'
          label='Permissão'
          onChange={updateField}
          value={data.permissionId}
          options={permissions.map((p) => ({
            label: p.feature ? `${p.resource}.${p.action}.${p.feature}` : `${p.resource}.${p.action}`,
            value: p.id,
          }))}
          required
        />
        <SelectInput<PolicyFormData>
          name='effect'
          label='Efeito'
          onChange={updateField}
          value={data.effect}
          options={EFFECT_OPTIONS}
          required
        />
        <SelectInput<PolicyFormData>
          name='scopeType'
          label='Tipo de escopo'
          onChange={updateField}
          value={data.scopeType}
          options={SCOPE_OPTIONS}
        />
        <TextInput<PolicyFormData>
          name='scopeKey'
          label='Identificador do escopo (opcional)'
          onChange={updateField}
          value={data.scopeKey}
        />
      </div>
    </FormModal>
  );
}
