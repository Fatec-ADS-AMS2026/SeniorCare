import {
  Ruler,
  Truck,
  Handshake,
  Archive,
  Package,
  Factory,
} from '@phosphor-icons/react';
import Card from '@/components/Card';
import SearchBar from '@/components/SearchBar';
import BreadcrumbPageTitle from '@/components/BreadcrumbPageTitle';
import useAppRoutes from '@/hooks/useAppRoutes';
import useAuth from '@/hooks/useAuth';
import { RequiredPermission } from '@/types/app/RouteDefinition';

interface RegistrationCard {
  text: string;
  subText: string;
  icon: JSX.Element;
  page: string;
  requiredPermission?: RequiredPermission;
}

export default function RegisterPage() {
  const routes = useAppRoutes();
  const { hasPermission } = useAuth();

  const cards: RegistrationCard[] = [
    {
      text: routes.UNIT_OF_MEASURE.displayName,
      subText: 'Unidades de medidas cadastradas',
      icon: <Ruler size={28} className='shrink-0' />,
      page: routes.UNIT_OF_MEASURE.path,
      requiredPermission: routes.UNIT_OF_MEASURE.requiredPermission,
    },
    {
      text: routes.CARRIER.displayName,
      subText: 'Transportadoras cadastradas',
      icon: <Truck size={28} className='shrink-0' />,
      page: routes.CARRIER.path,
      requiredPermission: routes.CARRIER.requiredPermission,
    },
    {
      text: routes.PRODUCT.displayName,
      subText: 'Produtos cadastrados',
      icon: <Ruler size={28} className='shrink-0' />,
      page: routes.PRODUCT.path,
      requiredPermission: routes.PRODUCT.requiredPermission,
    },
    {
      text: routes.MANUFACTURER.displayName,
      subText: 'Fabricantes cadastrados',
      icon: <Factory size={28} className='shrink-0' />,
      page: routes.MANUFACTURER.path,
      requiredPermission: routes.MANUFACTURER.requiredPermission,
    },
    {
      text: routes.PRODUCT_GROUP.displayName,
      subText: 'Grupos de produtos cadastrados',
      icon: <Package size={28} className='shrink-0' />,
      page: routes.PRODUCT_GROUP.path,
      requiredPermission: routes.PRODUCT_GROUP.requiredPermission,
    },
    {
      text: routes.SUPPLIER.displayName,
      subText: 'Fornecedores cadastrados',
      icon: <Handshake size={28} className='shrink-0' />,
      page: routes.SUPPLIER.path,
      requiredPermission: routes.SUPPLIER.requiredPermission,
    },
    {
      text: routes.PRODUCT_TYPE.displayName,
      subText: 'Tipos de produto cadastrados',
      icon: <Archive size={28} className='shrink-0' />,
      page: routes.PRODUCT_TYPE.path,
      requiredPermission: routes.PRODUCT_TYPE.requiredPermission,
    },
  ];

  // §10.5: menu reflete a mesma permissão exigida pela rota (RequireAuth já barra o
  // acesso direto por URL — aqui é só não oferecer o link pra quem não pode usá-lo).
  const visibleCards = cards.filter(
    ({ requiredPermission }) =>
      !requiredPermission ||
      hasPermission(requiredPermission.resource, requiredPermission.action, requiredPermission.feature)
  );

  return (
    <div className='bg-neutralLighter'>
      <BreadcrumbPageTitle title='Cadastros' />

      <div className='mt-8 px-4 flex flex-wrap items-center gap-8'>
        <SearchBar placeholder='Buscar Cadastro' action={console.log} />
        {/* Card Grid */}
        {visibleCards.map(({ text, icon, page, subText }) => (
          <Card
            key={page}
            subText={subText}
            text={text}
            icon={icon}
            page={page}
          />
        ))}
      </div>
    </div>
  );
}
