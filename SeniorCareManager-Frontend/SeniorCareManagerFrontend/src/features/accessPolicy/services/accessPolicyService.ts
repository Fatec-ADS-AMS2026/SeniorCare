import { api } from '@/features/api';
import { handleServiceError } from '@/utils/serviceUtils';
import ServiceResult from '@/types/app/ServiceResult';
import AccessPolicy from '@/types/models/AccessPolicy';
import { AccessScopeType } from '@/types/models/OrganizationalRoleAssignment';
import { AccessEffect } from '@/types/models/UserPermissionOverride';

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface AccessPolicyUpsertRequest {
  resource: string;
  action: string;
  feature?: string;
  scopeType?: AccessScopeType;
  scopeKey?: string;
  effect: AccessEffect;
}

// Id é Guid — não passa por generateGenericMethods. Política é versionada e
// imutável (§6.7): "revisar" cria uma NOVA linha (mesma PolicyKey, Version+1),
// nunca edita a existente; ativar/retirar trocam State, não os dados da versão.
const accessPolicyService = {
  getAll: async (): Promise<ServiceResult<AccessPolicy[]>> => {
    try {
      const res = await api.get<PagedResult<AccessPolicy>>('AdminAccessPolicy/');
      return { success: true, message: '', data: res.data.items };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  create: async (request: AccessPolicyUpsertRequest): Promise<ServiceResult<AccessPolicy>> => {
    try {
      const res = await api.post<AccessPolicy>('AdminAccessPolicy/', request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  revise: async (
    id: string,
    request: AccessPolicyUpsertRequest
  ): Promise<ServiceResult<AccessPolicy>> => {
    try {
      const res = await api.post<AccessPolicy>(`AdminAccessPolicy/${id}/revise`, request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  activate: async (id: string): Promise<ServiceResult<AccessPolicy>> => {
    try {
      const res = await api.put<AccessPolicy>(`AdminAccessPolicy/${id}/activate`);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  retire: async (id: string): Promise<ServiceResult<AccessPolicy>> => {
    try {
      const res = await api.put<AccessPolicy>(`AdminAccessPolicy/${id}/retire`);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },
};

export default accessPolicyService;
