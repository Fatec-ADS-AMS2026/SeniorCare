import {
  createBrowserRouter,
  createRoutesFromElements,
  Route,
  RouterProvider,
  useRouteError,
} from 'react-router-dom';
import { routes } from './routes';
import AuthenticatedLayout from '@/features/layout/AuthenticatedLayout';

const router = createBrowserRouter(
  createRoutesFromElements(
    <Route>
      <Route element={<AuthenticatedLayout />} errorElement={<GlobalErrorBoundary />}>
        <Route {...routes.HOME} />
        <Route {...routes.PROFILE} />
        <Route {...routes.SECURITY} />
      </Route>
      <Route errorElement={<GlobalErrorBoundary />}>
        <Route {...routes.LOGIN} />
        <Route {...routes.MFA_CHALLENGE} />
        <Route {...routes.MFA_ENROLL} />
      </Route>
    </Route>
  )
);

function GlobalErrorBoundary() {
  const error = useRouteError();
  console.error(error);

  return (
    <div className='flex flex-col items-center justify-center h-screen w-full gap-2'>
      <h1 className='text-2xl text-danger font-bold'>Algo deu errado</h1>
      <p className='text-textSecondary'>Recarregue a página e tente novamente.</p>
    </div>
  );
}

export default function AppRoutes() {
  return <RouterProvider router={router} />;
}
