import { authRoutes } from '@/features/auth';
import { adminUserRoutes } from '@/features/adminUser';
import { userSessionRoutes } from '@/features/userSession';
import { roleRoutes } from '@/features/role';
import { permissionGroupRoutes } from '@/features/permissionGroup';
import { organizationalRoleRoutes } from '@/features/organizationalRole';
import { organizationalRoleAssignmentRoutes } from '@/features/organizationalRoleAssignment';
import { userPermissionOverrideRoutes } from '@/features/userPermissionOverride';
import { accessPolicyRoutes } from '@/features/accessPolicy';
import { institutionSecurityRoutes } from '@/features/institutionSecurity';
import { healthInsurancePlanRoutes } from '@/features/healthInsurancePlan';
import { positionRoutes } from '@/features/position';
import { religionRoutes } from '@/features/religion';
import AccessibilityPage from '@/pages/AccessibilityPage';
import AdminOverview from '@/pages/Admin/AdminOverview';
import LandingPage from '@/pages/LandingPage';
import Registrations from '@/pages/Registrations';
import { createRoutes } from '@/utils/routesUtils';

const appRoutes = createRoutes({
  ACCESSIBILITY: {
    path: '/accessibility',
    displayName: 'Acessibilidade',
    element: <AccessibilityPage />,
  },
  LANDING: {
    displayName: 'Página Inicial',
    element: <LandingPage />,
    index: true,
    path: '',
  },
  ADMIN_OVERVIEW: {
    path: '/admin',
    displayName: 'Visão Geral',
    element: <AdminOverview />,
  },
  REGISTRATIONS: {
    path: '/registrations',
    displayName: 'Cadastros',
    element: <Registrations />,
  },
});

// É a união de todas as definições de rotas da aplicação
export const routes = {
  ...appRoutes,
  ...authRoutes,
  ...adminUserRoutes,
  ...userSessionRoutes,
  ...roleRoutes,
  ...permissionGroupRoutes,
  ...organizationalRoleRoutes,
  ...organizationalRoleAssignmentRoutes,
  ...userPermissionOverrideRoutes,
  ...accessPolicyRoutes,
  ...institutionSecurityRoutes,
  ...healthInsurancePlanRoutes,
  ...positionRoutes,
  ...religionRoutes,
} as const;
