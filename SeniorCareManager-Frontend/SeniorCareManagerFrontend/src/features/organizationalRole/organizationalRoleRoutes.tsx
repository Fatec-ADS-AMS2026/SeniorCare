import { createRoutes } from '@/utils/routesUtils';
import OrganizationalRoleOverview from './pages/OrganizationalRoleOverview';

export const organizationalRoleRoutes = createRoutes({
  ORGANIZATIONAL_ROLE: {
    path: '/organizational-role',
    displayName: 'Papéis Organizacionais',
    element: <OrganizationalRoleOverview />,
    requiredPermission: { resource: 'OrganizationalRole', action: 'read' },
  },
});
