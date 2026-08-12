// Espelha EffectivePermissionDTO — mesma tripla Resource/Action/Feature usada por
// [RequirePermission] no backend, devolvida por GET /auth/me.
export default interface EffectivePermission {
  resource: string;
  action: string;
  feature?: string;
}
