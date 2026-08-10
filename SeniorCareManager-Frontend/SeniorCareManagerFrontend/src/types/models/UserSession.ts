// Espelha UserSessionDTO (§6.8/§10.8). Sem flag "é a sessão atual" — /auth/me não
// devolve session_id e o cookie é HttpOnly (ilegível por JS); limitação aceita.
export default interface UserSession {
  id: string;
  userId: string;
  createdAtUtc: string;
  lastSeenAtUtc: string;
  revokedAtUtc?: string;
  userAgent?: string;
  ipAddress?: string;
}
