// §7.4 — validador de `returnTo` pro caso cruzado (portal → stock): quando o
// Senior Portal ou um link direto manda alguém pra
// `/login?returnTo=<destino>`, esse parâmetro é entrada externa de verdade
// (diferente de `location.state.from`, sintetizado internamente por
// RequireAuth e nunca alcançável por URL) — precisa da mesma validação do
// portal antes de navegar. Mesmo contrato de
// docs/architecture/senior-portal-contracts.md §3 e de
// SeniorPortal-Frontend/.../utils/returnPath.ts (§4.5) e
// SeniorCareManager-Frontend/.../utils/returnPath.ts (§6.4): caminho
// relativo (sem `//`, sem esquema, sem `\`) e dentro de `/`, `/care` ou
// `/stock`. Sem pacote compartilhado ainda (design.md decisão 8) —
// duplicado deliberadamente, não divergido.
const KNOWN_APP_PREFIXES = ['/care', '/stock'];
export const RETURN_PATH_FALLBACK = '/';

export function isSafeReturnPath(value: string): boolean {
  if (typeof value !== 'string' || value.length === 0) return false;
  // startsWith('/') já basta contra esquema (http:, javascript:, etc.) —
  // nenhum deles começa com `/`.
  if (!value.startsWith('/')) return false;
  if (value.startsWith('//')) return false;
  if (value.includes('\\')) return false;

  if (value === RETURN_PATH_FALLBACK) return true;
  return KNOWN_APP_PREFIXES.some((prefix) => value === prefix || value.startsWith(`${prefix}/`));
}

export function resolveReturnPath(returnTo: string | null | undefined): string | null {
  if (!returnTo) return null;

  if (!isSafeReturnPath(returnTo)) {
    console.warn(
      `[returnPath] destino rejeitado: "${returnTo}" — ignorado, sem navegação forçada.`
    );
    return null;
  }

  return returnTo;
}
