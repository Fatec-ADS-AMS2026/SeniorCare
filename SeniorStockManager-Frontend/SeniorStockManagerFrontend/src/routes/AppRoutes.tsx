import {
  createBrowserRouter,
  createRoutesFromElements,
  Route,
  RouterProvider,
  useRouteError,
} from 'react-router-dom';
import { routes } from './routes';
import { AppLayout, HeaderFooterLayout } from '@/features/layout';
import RequireAuth from '@/features/auth/components/RequireAuth';

// §7.1 — mesma variável usada por vite.config.ts (base do bundle); o
// roteador também precisa saber o caminho-base pra casar `/product` etc.
// contra a URL real `/stock/product` depois que a borda migrar pra
// roteamento por caminho (§8.2). Sem barra final (React Router não aceita) e
// `undefined` quando o caminho-base é a raiz — preserva o comportamento
// atual (deploy por subdomínio) sem qualquer mudança de rota. Mesmo padrão
// do care-web (§6.1).
const rawBasePath = import.meta.env.VITE_BASE_PATH;
const basename =
  rawBasePath && rawBasePath !== '/' ? rawBasePath.replace(/\/$/, '') : undefined;

const router = createBrowserRouter(
  createRoutesFromElements(
    <Route>
      <Route
        path=''
        element={
          <RequireAuth>
            <AppLayout />
          </RequireAuth>
        }
        errorElement={<GlobalErrorBoundary />}
      >
        <Route {...routes.ADMIN_OVERVIEW} />
        <Route {...routes.REGISTRATIONS} />
        <Route {...routes.CARRIER} />
        <Route {...routes.CARRIER_REGISTRATION} />
        <Route {...routes.CARRIER_EDIT} />
        <Route {...routes.MANUFACTURER} />
        <Route {...routes.PRODUCT} />
        <Route {...routes.PRODUCT_REGISTRATION} />
        <Route {...routes.PRODUCT_EDIT} />
        <Route {...routes.PRODUCT_GROUP} />
        <Route {...routes.PRODUCT_TYPE} />
        <Route {...routes.SUPPLIER} />
        <Route {...routes.SUPPLIER_REGISTRATION} />
        <Route {...routes.SUPPLIER_EDIT} />
        <Route {...routes.UNIT_OF_MEASURE} />
      </Route>
      <Route
        path=''
        element={<HeaderFooterLayout />}
        errorElement={<GlobalErrorBoundary />}
      >
        <Route {...routes.LOGIN} />
        <Route {...routes.ACCESSIBILITY} />
        <Route {...routes.LANDING} />
      </Route>
    </Route>
  ),
  { basename }
);

/**
 * Componente exibido ao dar erro nas rotas
 */
function GlobalErrorBoundary() {
  const error = useRouteError();
  console.error(error);

  return (
    <main className='min-h-screen w-screen p-4'>
      <h1 className='text-2xl text-red-500'>
        Erro ao tentar acessar a página!
      </h1>
    </main>
  );
}

export default function AppRoutes() {
  return <RouterProvider router={router} />;
}
