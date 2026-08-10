import { createRoutes } from '@/utils/routesUtils';
import SupplierOverview from './pages/SupplierOverview';
import SupplierForm from './pages/SupplierForm';

export const supplierRoutes = createRoutes({
  SUPPLIER: {
    path: '/supplier',
    displayName: 'Fornecedores',
    element: <SupplierOverview />,
    requiredPermission: { resource: 'Supplier', action: 'read' },
  },
  SUPPLIER_REGISTRATION: {
    path: '/supplier/registration',
    displayName: 'Cadastrar Fornecedor',
    element: <SupplierForm />,
    requiredPermission: { resource: 'Supplier', action: 'write' },
  },
  SUPPLIER_EDIT: {
    path: '/supplier/edit/:id',
    displayName: 'Editar Fornecedor',
    element: <SupplierForm />,
    requiredPermission: { resource: 'Supplier', action: 'write' },
  },
});
