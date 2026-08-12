import { api } from '@/features/api';
import { handleServiceError } from '@/utils/serviceUtils';
import ServiceResult from '@/types/app/ServiceResult';
import CurrentIdentity from '@/types/models/CurrentIdentity';
import {
  LoginMfaRequest,
  LoginRequest,
  LoginResponse,
  MessageResponse,
  MfaConfirmRequest,
  MfaConfirmResponse,
  MfaEnrollRequest,
  MfaEnrollResponse,
} from '../types';

// Endpoints de api/v1/Auth não são CRUD de catálogo — cada um ganha um método
// tipado aqui. Escopo do portal (§4): só o que o login/restauração precisam
// (me, login, MFA, logout) — recuperação/ativação/troca de senha continuam
// vivendo em care-web/stock-web até uma tarefa própria migrar esses fluxos.
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

  logout: async (): Promise<ServiceResult<MessageResponse>> => {
    try {
      const res = await api.post<MessageResponse>('Auth/logout');
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },
};

export default authService;
