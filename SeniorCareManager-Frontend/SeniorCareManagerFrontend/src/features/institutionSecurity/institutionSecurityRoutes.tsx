import { createRoutes } from '@/utils/routesUtils';
import InstitutionSecurityPage from './pages/InstitutionSecurityPage';

export const institutionSecurityRoutes = createRoutes({
  INSTITUTION_SECURITY: {
    path: '/institution-security',
    displayName: 'Parâmetros de Segurança',
    element: <InstitutionSecurityPage />,
    requiredPermission: { resource: 'InstitutionSecurityPolicy', action: 'read' },
  },
});
