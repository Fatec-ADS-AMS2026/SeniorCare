import { api } from '@/features/api';
import { handleServiceError } from '@/utils/serviceUtils';
import ServiceResult from '@/types/app/ServiceResult';
import AdminUser, { AccountState } from '@/types/models/AdminUser';

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface AdminUserCreateRequest {
  email: string;
  displayName: string;
}

export interface AdminUserStateChangeRequest {
  accountState: AccountState;
  currentPassword: string;
}

// AdminUser não é CRUD reto (id é Guid, sem update genérico — só troca de estado
// com reautenticação) — não passa por generateGenericMethods (§10.7).
const adminUserService = {
  getAll: async (): Promise<ServiceResult<AdminUser[]>> => {
    try {
      const res = await api.get<PagedResult<AdminUser>>('AdminUser/');
      return { success: true, message: '', data: res.data.items };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  getById: async (id: string): Promise<ServiceResult<AdminUser>> => {
    try {
      const res = await api.get<AdminUser>(`AdminUser/${id}`);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  create: async (request: AdminUserCreateRequest): Promise<ServiceResult<AdminUser>> => {
    try {
      const res = await api.post<AdminUser>('AdminUser/', request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  changeState: async (
    id: string,
    request: AdminUserStateChangeRequest
  ): Promise<ServiceResult<AdminUser>> => {
    try {
      const res = await api.put<AdminUser>(`AdminUser/${id}/state`, request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },
};

export default adminUserService;
