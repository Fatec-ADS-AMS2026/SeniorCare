import { api } from '@/features/api';
import { handleServiceError } from '@/utils/serviceUtils';
import ServiceResult from '@/types/app/ServiceResult';
import PermissionGroup from '@/types/models/PermissionGroup';

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface PermissionGroupUpsertRequest {
  name: string;
  rowVersion?: number;
}

// Id é Guid — não passa por generateGenericMethods. Link/unlink de permissões
// (POST/DELETE .../permissions) não tem endpoint de leitura — mesma limitação de
// Role (§10.7, achado documentado em tasks.md), sem UI de gestão de vínculos aqui.
const permissionGroupService = {
  getAll: async (): Promise<ServiceResult<PermissionGroup[]>> => {
    try {
      const res = await api.get<PagedResult<PermissionGroup>>('AdminPermissionGroup/');
      return { success: true, message: '', data: res.data.items };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  create: async (request: PermissionGroupUpsertRequest): Promise<ServiceResult<PermissionGroup>> => {
    try {
      const res = await api.post<PermissionGroup>('AdminPermissionGroup/', { name: request.name });
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  update: async (
    id: string,
    request: PermissionGroupUpsertRequest
  ): Promise<ServiceResult<PermissionGroup>> => {
    try {
      const res = await api.put<PermissionGroup>(`AdminPermissionGroup/${id}`, request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  deleteById: async (id: string): Promise<ServiceResult<undefined>> => {
    try {
      await api.delete(`AdminPermissionGroup/${id}`);
      return { success: true, message: 'Excluído com sucesso' };
    } catch (error) {
      return handleServiceError(error);
    }
  },
};

export default permissionGroupService;
