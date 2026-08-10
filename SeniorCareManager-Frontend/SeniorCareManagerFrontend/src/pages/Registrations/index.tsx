import { Cross, FirstAid, Briefcase } from '@phosphor-icons/react';
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

export default function Registrations() {
  const routes = useAppRoutes();
  const { hasPermission } = useAuth();

  const cards: RegistrationCard[] = [
    {
      text: 'Religião',
      subText: 'Religiões Cadastradas',
      icon: <Cross weight='bold' className='shrink-0 size-full' />,
      page: routes.RELIGION.path,
      requiredPermission: routes.RELIGION.requiredPermission,
    },
    {
      text: 'Plano de Saúde',
      subText: 'Planos de Saúde Cadastrados',
      icon: <FirstAid weight='bold' className='shrink-0 size-full' />,
      page: routes.HEALTH_INSURANCE_PLAN.path,
      requiredPermission: routes.HEALTH_INSURANCE_PLAN.requiredPermission,
    },
    {
      text: 'Cargo',
      subText: 'Cargos Cadastrados',
      icon: <Briefcase weight='bold' className='shrink-0 size-full' />,
      page: routes.POSITION.path,
      requiredPermission: routes.POSITION.requiredPermission,
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
    <div className='min-h bg-neutralLighter'>
      <BreadcrumbPageTitle title='Cadastros' />

      <div className='py-8 px-4 flex flex-col flex-wrap items-start gap-8'>
        {/* Search Bar Section */}
        <div className='w-96'>
          <SearchBar placeholder='Digite aqui...' action={console.log} />
        </div>

        <div className='flex flex-wrap gap-8 justify-center'>
          {/* Card Grid */}
          {visibleCards.map(({ text, icon, page, subText }) => (
            <Card
              key={text}
              subText={subText}
              text={text}
              icon={icon}
              page={page}
            />
          ))}
        </div>
      </div>
    </div>
  );
}
