export enum AccessScopeType {
  INSTITUTION = 1,
  UNIT = 2,
  SECTOR = 3,
}

// Espelha OrganizationalRoleAssignmentDTO (§6.5) — escopo e validade são campos
// reais do DTO (diferente de Role/PermissionGroup, aqui dá pra listar o que já
// existe de verdade, GET /?userId={guid?}).
export default interface OrganizationalRoleAssignment {
  id: string;
  userId: string;
  organizationalRoleId: string;
  scopeType: AccessScopeType;
  scopeKey?: string;
  validFrom: string;
  validTo?: string;
}
