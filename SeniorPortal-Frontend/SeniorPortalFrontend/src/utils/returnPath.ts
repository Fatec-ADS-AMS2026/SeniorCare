// §4.5 — validador central de `returnTo`, contrato travado em
// docs/architecture/senior-portal-contracts.md §3: caminho relativo (não pode
// começar com `//`, não pode conter esquema http(s):// nem `\`) E deve ser
// exatamente `/` (raiz do próprio portal) ou estar sob `/care`/`/stock`
// (rotas-base dos módulos). Qualquer outro valor é rejeitado — fallback `/`.
// Função única, pensada para ser reusada por portal/care/stock (ainda só
// consumida pelo portal em §4; a integração dos módulos é §6.4/§7.4).
const KNOWN_MODULE_PREFIXES = ['/care', '/stock'];
export const RETURN_PATH_FALLBACK = '/';

export function isSafeReturnPath(value: string): boolean {
  if (typeof value !== 'string' || value.length === 0) return false;
  // Só o startsWith('/') abaixo já basta contra esquema (`http:`,
  // `javascript:`, etc.) — nenhum deles começa com `/`. Uma regex de esquema
  // aqui seria código morto (nunca dispararia depois do startsWith).
  if (!value.startsWith('/')) return false;
  if (value.startsWith('//')) return false;
  if (value.includes('\\')) return false;

  if (value === RETURN_PATH_FALLBACK) return true;
  return KNOWN_MODULE_PREFIXES.some(
    (prefix) => value === prefix || value.startsWith(`${prefix}/`)
  );
}

// Resolve com fallback e registra a rejeição (spec.md "Redirecionamento
// malicioso" — a plataforma deve auditar/logar destinos rejeitados). O log é
// só de console por ora: não existe endpoint de auditoria de front-end até uma
// tarefa própria de observabilidade (§8.5) definir um.
export function resolveReturnPath(returnTo: string | null | undefined): string {
  if (!returnTo) return RETURN_PATH_FALLBACK;

  if (!isSafeReturnPath(returnTo)) {
    console.warn(
      `[returnPath] destino rejeitado: "${returnTo}" — usando fallback "${RETURN_PATH_FALLBACK}".`
    );
    return RETURN_PATH_FALLBACK;
  }

  return returnTo;
}
