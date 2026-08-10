export default interface Product {
  id: number;
  description: string;
  genericName: string;
  minimumStock: number;
  currentStock: number;
  stockValue?: number;
  unitPrice: number;
  averageCost?: number;
  lastPurchasePrice?: number;
  // bool no backend (§9.4) — não YesNo (1/2): esse campo nunca casou com o que a API
  // realmente manda, então o estado das duas checkboxes do formulário nunca refletia o
  // valor salvo de verdade.
  highCost: boolean;
  expirationControlled: boolean;
  productTypeId: number;
  unitOfMeasureId: number;
  // §9.1/9.7: token de concorrência e estado ativo — não existiam antes da API real.
  rowVersion: number;
  isActive: boolean;
}
