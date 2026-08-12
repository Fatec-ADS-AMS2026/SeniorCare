import EffectivePermission from './EffectivePermission';

export interface OrganizationalResponsibility {
  name: string;
  scopeType: 'INSTITUTION' | 'UNIT' | 'SECTOR';
  scopeKey?: string;
}

// Espelha CurrentIdentityDTO (GET /auth/me,
// docs/architecture/senior-portal-contracts.md §2) — a única fonte de sessão
// restaurada; nunca persistida fora de memória.
export default interface CurrentIdentity {
  userId: string;
  institutionId: string;
  institutionName: string;
  displayName: string;
  email: string;
  roles: string[];
  organizationalResponsibilities: OrganizationalResponsibility[];
  effectivePermissions: EffectivePermission[];
}
