import { AccessScopeType } from './OrganizationalRoleAssignment';

export enum AccessEffect {
  ALLOW = 1,
  DENY = 2,
}

// Espelha UserPermissionOverrideDTO (§6.6) — exceção pontual de permissão, grant
// ou deny, por cima do RBAC normal.
export default interface UserPermissionOverride {
  id: string;
  userId: string;
  resource: string;
  action: string;
  feature?: string;
  scopeType?: AccessScopeType;
  scopeKey?: string;
  effect: AccessEffect;
  justification: string;
  grantedByUserId: string;
  validFrom: string;
  validTo?: string;
}
