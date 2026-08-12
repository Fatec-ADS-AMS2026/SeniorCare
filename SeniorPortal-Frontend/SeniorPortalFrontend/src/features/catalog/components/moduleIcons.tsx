import { HeartStraight, Package, Question, type Icon } from '@phosphor-icons/react';

// Allowlist, não lookup dinâmico — mesmo racional de
// ModuleDefinitionValidator.cs (ApprovedIcons) no backend: um ícone novo exige
// código novo aqui, nunca um nome de string arbitrário resolvido em tempo de
// execução. `Question` cobre o caso defensivo de um `icon` desconhecido
// chegar do backend (nunca deveria acontecer — a validação já existe lá — mas
// a UI não deve quebrar se acontecer).
const MODULE_ICONS: Record<string, Icon> = {
  HeartStraight,
  Package,
};

export function getModuleIcon(icon: string): Icon {
  return MODULE_ICONS[icon] ?? Question;
}
