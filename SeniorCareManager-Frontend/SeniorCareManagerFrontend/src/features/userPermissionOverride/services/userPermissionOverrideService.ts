import { api } from '@/features/api';
import { handleServiceError } from '@/utils/serviceUtils';
import ServiceResult from '@/types/app/ServiceResult';
import UserPermissionOverride, {
  AccessEffect,
} from '@/types/models/UserPermissionOverride';
import { AccessScopeType } from '@/types/models/OrganizationalRoleAssignment';

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface UserPermissionOverrideCreateRequest {
  userId: string;
  resource: string;
  action: string;
  feature?: string;
  scopeType?: AccessScopeType;
  scopeKey?: string;
  effect: AccessEffect;
  justification?: string;
  validFrom: string;
  validTo?: string;
}

// Id é Guid — não passa por generateGenericMethods. "Revogar" é PUT {id}/revoke
// (sem corpo, encerra a validade agora) — sem delete físico.
const userPermissionOverrideService = {
  getAll: async (): Promise<ServiceResult<UserPermissionOverride[]>> => {
    try {
      const res = await api.get<PagedResult<UserPermissionOverride>>(
        'AdminUserPermissionOverride/'
      );
      return { success: true, message: '', data: res.data.items };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  create: async (
    request: UserPermissionOverrideCreateRequest
  ): Promise<ServiceResult<UserPermissionOverride>> => {
    try {
      const res = await api.post<UserPermissionOverride>(
        'AdminUserPermissionOverride/',
        request
      );
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  revoke: async (id: string): Promise<ServiceResult<UserPermissionOverride>> => {
    try {
      const res = await api.put<UserPermissionOverride>(
        `AdminUserPermissionOverride/${id}/revoke`
      );
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },
};

export default userPermissionOverrideService;
