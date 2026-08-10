import { createRoutes } from '@/utils/routesUtils';
import RoleOverview from './pages/RoleOverview';

export const roleRoutes = createRoutes({
  ROLE: {
    path: '/role',
    displayName: 'Papéis',
    element: <RoleOverview />,
    requiredPermission: { resource: 'Role', action: 'read' },
  },
});
