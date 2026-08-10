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

// Espelha AdminUserDTO (§6.1) — nunca carrega senha/credencial.
export default interface AdminUser {
  id: string;
  email: string;
  displayName: string;
  accountState: AccountState;
  identityOrigin: IdentityOrigin;
}

// Só a resposta de POST /AdminUser (§10.6) — não existe serviço de e-mail no
// projeto, então o token de ativação precisa ser mostrado pro admin repassar.
export interface AdminUserCreated extends AdminUser {
  activationToken: string;
}
