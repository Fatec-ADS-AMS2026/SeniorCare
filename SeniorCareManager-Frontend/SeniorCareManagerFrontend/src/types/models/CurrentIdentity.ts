import EffectivePermission from './EffectivePermission';

export interface OrganizationalResponsibility {
  name: string;
  scopeType: 'INSTITUTION' | 'UNIT' | 'SECTOR';
  scopeKey?: string;
}

// Espelha CurrentIdentityDTO (GET /auth/me) — a única fonte de sessão restaurada;
// nunca persistida fora de memória (§10.3).
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
