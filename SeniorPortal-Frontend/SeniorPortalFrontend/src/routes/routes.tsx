import { authRoutes } from '@/features/auth';
import HomePage from '@/pages/HomePage';
import { createRoutes } from '@/utils/routesUtils';

export const routes = createRoutes({
  HOME: {
    path: '/',
    displayName: 'Início',
    element: <HomePage />,
    index: true,
  },
  ...authRoutes,
});
