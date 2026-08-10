import { createRoutes } from '@/utils/routesUtils';
import CarrierOverview from './pages/CarrierOverview';
import CarrierForm from './pages/CarrierForm';

export const carrierRoutes = createRoutes({
  CARRIER: {
    path: '/carrier',
    displayName: 'Transportadoras',
    element: <CarrierOverview />,
    requiredPermission: { resource: 'Carrier', action: 'read' },
  },
  CARRIER_REGISTRATION: {
    path: '/carrier/registration',
    displayName: 'Cadastrar Transportadora',
    element: <CarrierForm />,
    requiredPermission: { resource: 'Carrier', action: 'write' },
  },
  CARRIER_EDIT: {
    path: '/carrier/edit/:id',
    displayName: 'Editar Transportadora',
    element: <CarrierForm />,
    requiredPermission: { resource: 'Carrier', action: 'write' },
  },
});
