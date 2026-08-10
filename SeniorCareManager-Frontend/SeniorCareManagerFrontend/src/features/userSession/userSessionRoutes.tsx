import { createRoutes } from '@/utils/routesUtils';
import UserSessionOverview from './pages/UserSessionOverview';

export const userSessionRoutes = createRoutes({
  USER_SESSION: {
    path: '/admin-user/:userId/sessions',
    displayName: 'Sessões',
    element: <UserSessionOverview />,
    requiredPermission: { resource: 'UserSession', action: 'read' },
  },
});
