import { CheckCircle, Question, Wrench, WarningCircle } from '@phosphor-icons/react';
import ModuleCatalogItem from '@/types/models/ModuleCatalogItem';
import OperationalState from '@/types/models/OperationalState';
import { getModuleIcon } from './moduleIcons';

interface ModuleCardProps {
  module: ModuleCatalogItem;
}

// §5.2/§5.6 — cada estado tem ícone + texto próprios (nunca só cor), pra não
// depender de percepção de cor (spec.md "Estado de manutenção"). AVAILABLE é
// o único estado que abre navegação normal; MAINTENANCE/UNAVAILABLE mostram a
// mensagem operacional (já sanitizada no backend, §2.5) sem permitir abrir o
// destino. DISABLED nunca chega aqui — o backend já omite (§3.2) e
// CatalogPage.tsx filtra de novo como defesa em profundidade — mas o
// fallback abaixo evita que ModuleCard quebre (em vez de falhar de forma
// segura) se algum dia for reusado fora desse fluxo filtrado.
const FALLBACK_PRESENTATION = { label: 'Estado desconhecido', Icon: Question };

const STATE_PRESENTATION: Partial<Record<OperationalState, { label: string; Icon: typeof CheckCircle }>> = {
  [OperationalState.AVAILABLE]: { label: 'Disponível', Icon: CheckCircle },
  [OperationalState.MAINTENANCE]: { label: 'Em manutenção', Icon: Wrench },
  [OperationalState.UNAVAILABLE]: { label: 'Indisponível', Icon: WarningCircle },
};

export default function ModuleCard({ module }: ModuleCardProps) {
  const ModuleIcon = getModuleIcon(module.icon);
  const isAvailable = module.operationalState === OperationalState.AVAILABLE;
  const presentation = STATE_PRESENTATION[module.operationalState] ?? FALLBACK_PRESENTATION;

  const cardBody = (
    <>
      <ModuleIcon size={40} className='text-primary shrink-0' aria-hidden='true' />
      <div className='flex flex-col gap-1 min-w-0'>
        <h2 className='font-semibold text-textPrimary text-lg truncate'>{module.name}</h2>
        <p className='text-textSecondary text-sm line-clamp-2'>{module.description}</p>
        <span className='flex items-center gap-1 text-sm text-textSecondary mt-1'>
          <presentation.Icon size={16} aria-hidden='true' />
          {presentation.label}
        </span>
        {!isAvailable && module.operationalMessage && (
          <p className='text-textSecondary text-sm mt-1'>{module.operationalMessage}</p>
        )}
      </div>
    </>
  );

  if (isAvailable) {
    return (
      <a
        href={module.path}
        className='flex gap-3 p-4 bg-neutralWhite rounded-lg shadow-sm border border-neutralLight hover:border-primary focus:outline focus:outline-2 focus:outline-primary transition-colors'
      >
        {cardBody}
      </a>
    );
  }

  return (
    <div
      className='flex gap-3 p-4 bg-neutralLighter rounded-lg border border-neutralLight opacity-80'
      aria-label={`${module.name} — ${presentation.label}`}
    >
      {cardBody}
    </div>
  );
}
