import { createRoutes } from '@/utils/routesUtils';
import OrganizationalRoleAssignmentOverview from './pages/OrganizationalRoleAssignmentOverview';

export const organizationalRoleAssignmentRoutes = createRoutes({
  ORGANIZATIONAL_ROLE_ASSIGNMENT: {
    path: '/organizational-role-assignment',
    displayName: 'Atribuições de Papel Organizacional',
    element: <OrganizationalRoleAssignmentOverview />,
    requiredPermission: {
      resource: 'OrganizationalRoleAssignment',
      action: 'read',
    },
  },
});
