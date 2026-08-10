import { ReactNode } from 'react';

export interface RequiredPermission {
  resource: string;
  action: string;
  feature?: string;
}

export interface RouteDefinition {
  displayName: string;
  path: string;
  element: ReactNode;
  index?: boolean;
  // §10.4/§10.5: checado por RequireAuth (rota) e pelo Sidebar (menu) contra
  // AuthContext.hasPermission — ausente = qualquer pessoa autenticada acessa.
  requiredPermission?: RequiredPermission;
  // §10.5: item existe (ex. tela ainda não construída) mas não deve aparecer no menu.
  hideFromNav?: boolean;
}
