import { createRoutes } from '@/utils/routesUtils';
import AdminUserOverview from './pages/AdminUserOverview';

export const adminUserRoutes = createRoutes({
  ADMIN_USER: {
    path: '/admin-user',
    displayName: 'Usuários',
    element: <AdminUserOverview />,
    requiredPermission: { resource: 'AdminUser', action: 'read' },
  },
});
