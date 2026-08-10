import { createRoutes } from '@/utils/routesUtils';
import AccessPolicyOverview from './pages/AccessPolicyOverview';

export const accessPolicyRoutes = createRoutes({
  ACCESS_POLICY: {
    path: '/access-policy',
    displayName: 'Políticas de Acesso',
    element: <AccessPolicyOverview />,
    requiredPermission: { resource: 'AccessPolicy', action: 'read' },
  },
});
