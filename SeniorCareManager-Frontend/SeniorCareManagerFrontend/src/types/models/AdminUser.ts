// Espelha os enums do backend (Objects/Enums) — sem JsonStringEnumConverter
// registrado, System.Text.Json serializa o valor numérico subjacente.
export enum AccountState {
  PROVISIONED = 1,
  ACTIVE = 2,
  INACTIVE = 3,
  BLOCKED = 4,
  EXPIRED = 5,
}

export enum IdentityOrigin {
  LOCAL = 1,
  LDAP = 2,
  OIDC = 3,
}

// Espelha AdminUserDTO (§6.1) — nunca carrega senha/credencial nem token de
// ativação (platform-authentication: tokens SHALL NOT aparecer em resposta
// administrativa nenhuma, nem a de criação — achado da revisão do PR).
export default interface AdminUser {
  id: string;
  email: string;
  displayName: string;
  accountState: AccountState;
  identityOrigin: IdentityOrigin;
}
