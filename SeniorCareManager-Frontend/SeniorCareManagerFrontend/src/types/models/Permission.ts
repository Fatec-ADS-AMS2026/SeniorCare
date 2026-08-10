// Espelha PermissionDTO — vocabulário fixo do sistema (§5), só leitura.
export default interface Permission {
  id: string;
  resource: string;
  action: string;
  feature?: string;
  description?: string;
  isSystemOperation: boolean;
}
