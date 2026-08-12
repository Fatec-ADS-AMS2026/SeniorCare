import OperationalState from './OperationalState';

// Espelha ModuleCatalogItemDTO (GET /api/v1/me/modules, §3.2) — resposta
// mínima: chave, apresentação aprovada, caminho relativo, ordem e estado
// operacional. Nunca inclui regra de autorização ou dado de domínio.
export default interface ModuleCatalogItem {
  key: string;
  name: string;
  description: string;
  icon: string;
  path: string;
  order: number;
  operationalState: OperationalState;
  operationalMessage?: string | null;
}
