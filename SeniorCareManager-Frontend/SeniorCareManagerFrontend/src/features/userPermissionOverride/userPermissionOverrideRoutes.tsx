import { createRoutes } from '@/utils/routesUtils';
import UserPermissionOverrideOverview from './pages/UserPermissionOverrideOverview';

export const userPermissionOverrideRoutes = createRoutes({
  USER_PERMISSION_OVERRIDE: {
    path: '/user-permission-override',
    displayName: 'Exceções de Permissão',
    element: <UserPermissionOverrideOverview />,
    requiredPermission: { resource: 'UserPermissionOverride', action: 'read' },
  },
});
