import { api } from '@/features/api';
import { handleServiceError } from '@/utils/serviceUtils';
import ServiceResult from '@/types/app/ServiceResult';
import CurrentIdentity from '@/types/models/CurrentIdentity';
import {
  ActivateAccountRequest,
  ChangePasswordRequest,
  LoginMfaRequest,
  LoginRequest,
  LoginResponse,
  MessageResponse,
  MfaConfirmRequest,
  MfaConfirmResponse,
  MfaEnrollRequest,
  MfaEnrollResponse,
  RecoverAccountRequest,
  RegenerateRecoveryCodesRequest,
  ResetPasswordRequest,
} from '../types';

// Endpoints de api/v1/Auth (§7) não são CRUD de catálogo — cada um ganha um método
// tipado aqui em vez de generateGenericMethods (que assume list/get/create/update/
// delete de uma única entidade).
const authService = {
  me: async (): Promise<ServiceResult<CurrentIdentity>> => {
    try {
      const res = await api.get<CurrentIdentity>('Auth/me');
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  login: async (request: LoginRequest): Promise<ServiceResult<LoginResponse>> => {
    try {
      const res = await api.post<LoginResponse>('Auth/login', request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  loginMfa: async (request: LoginMfaRequest): Promise<ServiceResult<LoginResponse>> => {
    try {
      const res = await api.post<LoginResponse>('Auth/login/mfa', request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  mfaEnroll: async (request: MfaEnrollRequest): Promise<ServiceResult<MfaEnrollResponse>> => {
    try {
      const res = await api.post<MfaEnrollResponse>('Auth/mfa/enroll', request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  mfaConfirm: async (request: MfaConfirmRequest): Promise<ServiceResult<MfaConfirmResponse>> => {
    try {
      const res = await api.post<MfaConfirmResponse>('Auth/mfa/confirm', request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  regenerateRecoveryCodes: async (
    request: RegenerateRecoveryCodesRequest
  ): Promise<ServiceResult<MfaConfirmResponse>> => {
    try {
      const res = await api.post<MfaConfirmResponse>('Auth/mfa/recovery-codes/regenerate', request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  logout: async (): Promise<ServiceResult<MessageResponse>> => {
    try {
      const res = await api.post<MessageResponse>('Auth/logout');
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  activate: async (request: ActivateAccountRequest): Promise<ServiceResult<MessageResponse>> => {
    try {
      const res = await api.post<MessageResponse>('Auth/activate', request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  recover: async (request: RecoverAccountRequest): Promise<ServiceResult<MessageResponse>> => {
    try {
      const res = await api.post<MessageResponse>('Auth/recover', request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  resetPassword: async (request: ResetPasswordRequest): Promise<ServiceResult<MessageResponse>> => {
    try {
      const res = await api.post<MessageResponse>('Auth/reset-password', request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  changePassword: async (request: ChangePasswordRequest): Promise<ServiceResult<MessageResponse>> => {
    try {
      const res = await api.post<MessageResponse>('Auth/change-password', request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },
};

export default authService;
