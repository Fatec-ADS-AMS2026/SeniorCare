import { createRoutes } from '@/utils/routesUtils';
import ProductOverview from './pages/ProductOverview';
import ProductForm from './pages/ProductForm';

export const productRoutes = createRoutes({
  PRODUCT: {
    path: '/product',
    displayName: 'Produtos',
    element: <ProductOverview />,
    requiredPermission: { resource: 'Product', action: 'read' },
  },
  PRODUCT_REGISTRATION: {
    path: '/product/registration',
    displayName: 'Cadastrar Produto',
    element: <ProductForm />,
    requiredPermission: { resource: 'Product', action: 'write' },
  },
  PRODUCT_EDIT: {
    path: '/product/edit/:id',
    displayName: 'Editar Produto',
    element: <ProductForm />,
    requiredPermission: { resource: 'Product', action: 'write' },
  },
});
