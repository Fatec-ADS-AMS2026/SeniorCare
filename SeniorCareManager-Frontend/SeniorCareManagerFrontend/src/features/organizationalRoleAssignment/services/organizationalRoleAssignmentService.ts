import { api } from '@/features/api';
import { handleServiceError } from '@/utils/serviceUtils';
import ServiceResult from '@/types/app/ServiceResult';
import OrganizationalRoleAssignment, {
  AccessScopeType,
} from '@/types/models/OrganizationalRoleAssignment';

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface OrganizationalRoleAssignmentCreateRequest {
  userId: string;
  organizationalRoleId: string;
  scopeType: AccessScopeType;
  scopeKey?: string;
  validFrom: string;
  validTo?: string;
}

// Id é Guid — não passa por generateGenericMethods. "Encerrar" é PUT {id}/end (sem
// corpo) — não existe delete físico, coerente com o restante do modelo de acesso.
const organizationalRoleAssignmentService = {
  getAll: async (): Promise<ServiceResult<OrganizationalRoleAssignment[]>> => {
    try {
      const res = await api.get<PagedResult<OrganizationalRoleAssignment>>(
        'AdminOrganizationalRoleAssignment/'
      );
      return { success: true, message: '', data: res.data.items };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  create: async (
    request: OrganizationalRoleAssignmentCreateRequest
  ): Promise<ServiceResult<OrganizationalRoleAssignment>> => {
    try {
      const res = await api.post<OrganizationalRoleAssignment>(
        'AdminOrganizationalRoleAssignment/',
        request
      );
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  endEarly: async (id: string): Promise<ServiceResult<OrganizationalRoleAssignment>> => {
    try {
      const res = await api.put<OrganizationalRoleAssignment>(
        `AdminOrganizationalRoleAssignment/${id}/end`
      );
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },
};

export default organizationalRoleAssignmentService;
