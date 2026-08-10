import CurrentIdentity from '@/types/models/CurrentIdentity';

// Formas exatas das requests/responses de api/v1/auth/* (§7) — não são CRUD de
// catálogo, por isso não passam por generateGenericMethods (ver serviceUtils.ts).

export interface LoginRequest {
  email: string;
  password: string;
}

// "ok" | "mfa_required" | "mfa_enrollment_required"
export type LoginStatus = 'ok' | 'mfa_required' | 'mfa_enrollment_required';

export interface LoginResponse {
  status: LoginStatus;
  challengeToken?: string;
  identity?: CurrentIdentity;
  remainingRecoveryCodes?: number;
}

export interface LoginMfaRequest {
  challengeToken: string;
  code: string;
}

export interface MfaEnrollRequest {
  challengeToken?: string;
}

export interface MfaEnrollResponse {
  authenticatorKey: string;
  otpAuthUri: string;
}

export interface MfaConfirmRequest {
  challengeToken?: string;
  code: string;
}

export interface MfaConfirmResponse {
  recoveryCodes: string[];
  identity?: CurrentIdentity;
}

export interface RegenerateRecoveryCodesRequest {
  currentPassword: string;
}

export interface ActivateAccountRequest {
  email: string;
  token: string;
  newPassword: string;
}

export interface RecoverAccountRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}

export interface ChangePasswordRequest {
  email: string;
  currentPassword: string;
  newPassword: string;
}

export interface MessageResponse {
  message: string;
}
