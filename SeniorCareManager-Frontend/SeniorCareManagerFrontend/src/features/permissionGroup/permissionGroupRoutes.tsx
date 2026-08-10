import { createRoutes } from '@/utils/routesUtils';
import PermissionGroupOverview from './pages/PermissionGroupOverview';

export const permissionGroupRoutes = createRoutes({
  PERMISSION_GROUP: {
    path: '/permission-group',
    displayName: 'Grupos de Permissão',
    element: <PermissionGroupOverview />,
    requiredPermission: { resource: 'PermissionGroup', action: 'read' },
  },
});
