import {
  IdentificationBadge,
  Devices,
  UsersFour,
  ShieldCheck,
  Buildings,
  UserSwitch,
  Prohibit,
  FileLock,
  Gear,
} from '@phosphor-icons/react';
import BreadcrumbPageTitle from '@/components/BreadcrumbPageTitle';
import Card from '@/components/Card';
import useAppRoutes from '@/hooks/useAppRoutes';
import useAuth from '@/hooks/useAuth';
import { RequiredPermission } from '@/types/app/RouteDefinition';

interface AdminCard {
  text: string;
  subText: string;
  icon: JSX.Element;
  page: string;
  requiredPermission?: RequiredPermission;
}

// Console administrativo (§10.6-10.8) — só existe no front-end care (decisão do
// usuário: um único console, não duplicado no front-end de estoque).
export default function AdminOverview() {
  const routes = useAppRoutes();
  const { hasPermission } = useAuth();

  const cards: AdminCard[] = [
    {
      text: routes.ADMIN_USER.displayName,
      subText: 'Contas de usuário e estado',
      icon: <IdentificationBadge size={28} weight='bold' className='shrink-0 size-full' />,
      page: routes.ADMIN_USER.path,
      requiredPermission: routes.ADMIN_USER.requiredPermission,
    },
    {
      text: routes.ROLE.displayName,
      subText: 'Papéis e permissões associadas',
      icon: <ShieldCheck size={28} weight='bold' className='shrink-0 size-full' />,
      page: routes.ROLE.path,
      requiredPermission: routes.ROLE.requiredPermission,
    },
    {
      text: routes.PERMISSION_GROUP.displayName,
      subText: 'Conjuntos de permissões reutilizáveis',
      icon: <UsersFour size={28} weight='bold' className='shrink-0 size-full' />,
      page: routes.PERMISSION_GROUP.path,
      requiredPermission: routes.PERMISSION_GROUP.requiredPermission,
    },
    {
      text: routes.ORGANIZATIONAL_ROLE.displayName,
      subText: 'Papéis organizacionais da instituição',
      icon: <Buildings size={28} weight='bold' className='shrink-0 size-full' />,
      page: routes.ORGANIZATIONAL_ROLE.path,
      requiredPermission: routes.ORGANIZATIONAL_ROLE.requiredPermission,
    },
    {
      text: routes.ORGANIZATIONAL_ROLE_ASSIGNMENT.displayName,
      subText: 'Atribuições, escopo e validade',
      icon: <UserSwitch size={28} weight='bold' className='shrink-0 size-full' />,
      page: routes.ORGANIZATIONAL_ROLE_ASSIGNMENT.path,
      requiredPermission: routes.ORGANIZATIONAL_ROLE_ASSIGNMENT.requiredPermission,
    },
    {
      text: routes.USER_PERMISSION_OVERRIDE.displayName,
      subText: 'Exceções pontuais de permissão',
      icon: <Prohibit size={28} weight='bold' className='shrink-0 size-full' />,
      page: routes.USER_PERMISSION_OVERRIDE.path,
      requiredPermission: routes.USER_PERMISSION_OVERRIDE.requiredPermission,
    },
    {
      text: routes.ACCESS_POLICY.displayName,
      subText: 'Políticas versionadas de acesso',
      icon: <FileLock size={28} weight='bold' className='shrink-0 size-full' />,
      page: routes.ACCESS_POLICY.path,
      requiredPermission: routes.ACCESS_POLICY.requiredPermission,
    },
    {
      text: routes.INSTITUTION_SECURITY.displayName,
      subText: 'Bloqueio, MFA e duração de sessão',
      icon: <Gear size={28} weight='bold' className='shrink-0 size-full' />,
      page: routes.INSTITUTION_SECURITY.path,
      requiredPermission: routes.INSTITUTION_SECURITY.requiredPermission,
    },
    {
      text: 'Sessões ativas',
      subText: 'Consultadas a partir de cada usuário',
      icon: <Devices size={28} weight='bold' className='shrink-0 size-full' />,
      page: routes.ADMIN_USER.path,
      requiredPermission: { resource: 'UserSession', action: 'read' },
    },
  ];

  const visibleCards = cards.filter(
    ({ requiredPermission }) =>
      !requiredPermission ||
      hasPermission(requiredPermission.resource, requiredPermission.action, requiredPermission.feature)
  );

  return (
    <div className='min-h bg-neutralLighter'>
      <BreadcrumbPageTitle title={routes.ADMIN_OVERVIEW.displayName} />
      <div className='py-8 px-4 flex flex-wrap gap-8 justify-center'>
        {visibleCards.map(({ text, icon, page, subText }) => (
          <Card key={text} subText={subText} text={text} icon={icon} page={page} />
        ))}
      </div>
    </div>
  );
}
