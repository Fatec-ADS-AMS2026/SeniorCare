import { authRoutes } from '@/features/auth';
import CatalogPage from '@/features/catalog/pages/CatalogPage';
import ProfilePage from '@/features/auth/pages/ProfilePage';
import SecurityPage from '@/features/auth/pages/SecurityPage';
import { createRoutes } from '@/utils/routesUtils';

export const routes = createRoutes({
  HOME: {
    path: '/',
    displayName: 'Catálogo',
    element: <CatalogPage />,
    index: true,
  },
  PROFILE: {
    path: '/profile',
    displayName: 'Perfil',
    element: <ProfilePage />,
  },
  SECURITY: {
    path: '/security',
    displayName: 'Segurança',
    element: <SecurityPage />,
  },
  ...authRoutes,
});
