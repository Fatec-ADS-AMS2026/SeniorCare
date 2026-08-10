import { api } from '@/features/api';
import { handleServiceError } from '@/utils/serviceUtils';
import ServiceResult from '@/types/app/ServiceResult';
import InstitutionSecurityPolicy from '@/types/models/InstitutionSecurityPolicy';

export interface InstitutionSecurityPolicyUpdateRequest {
  lockoutDurationMinutes?: number;
  maxFailedAttempts?: number;
  accessTokenDurationMinutes?: number;
  refreshTokenDurationDays?: number;
  mfaRequiredForAllUsers: boolean;
  currentPassword: string;
}

// Registro único por instituição (GET sem paginação) — mudar exige reautenticação
// (§6.8/§10.8), mesmo padrão do ReauthModal usado em AdminUser/UserSession.
const institutionSecurityService = {
  get: async (): Promise<ServiceResult<InstitutionSecurityPolicy>> => {
    try {
      const res = await api.get<InstitutionSecurityPolicy>('AdminInstitutionSecurity/');
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  update: async (
    request: InstitutionSecurityPolicyUpdateRequest
  ): Promise<ServiceResult<InstitutionSecurityPolicy>> => {
    try {
      const res = await api.put<InstitutionSecurityPolicy>(
        'AdminInstitutionSecurity/',
        request
      );
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },
};

export default institutionSecurityService;
