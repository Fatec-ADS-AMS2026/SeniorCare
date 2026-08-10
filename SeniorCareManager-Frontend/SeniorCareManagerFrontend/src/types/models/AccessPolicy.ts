import { AccessScopeType } from './OrganizationalRoleAssignment';
import { AccessEffect } from './UserPermissionOverride';

export enum AccessPolicyState {
  DRAFT = 1,
  ACTIVE = 2,
  RETIRED = 3,
}

// Espelha AccessPolicyDTO (§6.7) — versionado por PolicyKey, imutável (editar
// nunca muda a linha existente, sempre cria uma nova via "revisar").
export default interface AccessPolicy {
  id: string;
  policyKey: string;
  version: number;
  resource: string;
  action: string;
  feature?: string;
  scopeType?: AccessScopeType;
  scopeKey?: string;
  effect: AccessEffect;
  state: AccessPolicyState;
}
