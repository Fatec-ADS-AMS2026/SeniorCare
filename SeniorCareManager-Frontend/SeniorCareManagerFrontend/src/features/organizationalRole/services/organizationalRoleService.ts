import { api } from '@/features/api';
import { handleServiceError } from '@/utils/serviceUtils';
import ServiceResult from '@/types/app/ServiceResult';
import OrganizationalRole from '@/types/models/OrganizationalRole';

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface OrganizationalRoleUpsertRequest {
  name: string;
  rowVersion?: number;
}

// Id é Guid — não passa por generateGenericMethods. Link/unlink de grupos de
// permissão não tem endpoint de leitura — mesma limitação de Role/PermissionGroup
// (§10.7), sem UI de gestão de vínculos aqui.
const organizationalRoleService = {
  getAll: async (): Promise<ServiceResult<OrganizationalRole[]>> => {
    try {
      const res = await api.get<PagedResult<OrganizationalRole>>('AdminOrganizationalRole/');
      return { success: true, message: '', data: res.data.items };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  create: async (
    request: OrganizationalRoleUpsertRequest
  ): Promise<ServiceResult<OrganizationalRole>> => {
    try {
      const res = await api.post<OrganizationalRole>('AdminOrganizationalRole/', {
        name: request.name,
      });
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  update: async (
    id: string,
    request: OrganizationalRoleUpsertRequest
  ): Promise<ServiceResult<OrganizationalRole>> => {
    try {
      const res = await api.put<OrganizationalRole>(`AdminOrganizationalRole/${id}`, request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  deleteById: async (id: string): Promise<ServiceResult<undefined>> => {
    try {
      await api.delete(`AdminOrganizationalRole/${id}`);
      return { success: true, message: 'Excluído com sucesso' };
    } catch (error) {
      return handleServiceError(error);
    }
  },
};

export default organizationalRoleService;
