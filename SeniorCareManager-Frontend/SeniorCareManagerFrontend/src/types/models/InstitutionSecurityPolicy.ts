// Espelha InstitutionSecurityPolicyDTO (§6.8) — registro único por instituição,
// não uma lista. Campos nulos usam o default do backend (5 tentativas / 15 min).
export default interface InstitutionSecurityPolicy {
  lockoutDurationMinutes?: number;
  maxFailedAttempts?: number;
  accessTokenDurationMinutes?: number;
  refreshTokenDurationDays?: number;
  mfaRequiredForAllUsers: boolean;
}
