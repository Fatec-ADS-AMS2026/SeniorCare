import { api } from '@/features/api';
import { handleServiceError } from '@/utils/serviceUtils';
import ServiceResult from '@/types/app/ServiceResult';
import UserSession from '@/types/models/UserSession';

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface RevokeSessionRequest {
  currentPassword: string;
}

// UserSession é sempre lido/revogado no contexto de um usuário (§10.8) — não passa
// por generateGenericMethods (sem create/update genérico, ações exigem
// reautenticação).
const userSessionService = {
  getAllForUser: async (userId: string): Promise<ServiceResult<UserSession[]>> => {
    try {
      const res = await api.get<PagedResult<UserSession>>(`AdminUserSession?userId=${userId}`);
      return { success: true, message: '', data: res.data.items };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  revoke: async (id: string, request: RevokeSessionRequest): Promise<ServiceResult<UserSession>> => {
    try {
      const res = await api.put<UserSession>(`AdminUserSession/${id}/revoke`, request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  revokeAll: async (userId: string, request: RevokeSessionRequest): Promise<ServiceResult<undefined>> => {
    try {
      await api.put(`AdminUserSession/revoke-all?userId=${userId}`, request);
      return { success: true, message: 'Todas as sessões foram revogadas.' };
    } catch (error) {
      return handleServiceError(error);
    }
  },
};

export default userSessionService;
