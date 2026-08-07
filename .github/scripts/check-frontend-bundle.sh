#!/bin/bash
# check-frontend-bundle.sh
#
# Verifica, no bundle de produção de cada front-end (dist/), que:
#   1. não há URL absoluta apontando para localhost:<porta> como destino
#      operacional (regressão do bug original: baseURL fixo em
#      https://localhost:7053 — ver spec runtime-configuration, cenário
#      "Execução em produção");
#   2. não há segredo óbvio embutido (mesmas heurísticas do
#      check-env-hygiene.sh, aplicadas ao JS/CSS gerado).
#
# Não usa "grep localhost" solto: bibliotecas como o axios têm fallback
# interno "http://localhost" (sem porta) usado só pra detecção de ambiente,
# não como destino de rede — isso é ruído esperado, não regressão.
#
# Uso: ./check-frontend-bundle.sh <caminho-do-dist> [<caminho-do-dist> ...]

set -euo pipefail

if [ $# -eq 0 ]; then
  echo "uso: $(basename "$0") <caminho-do-dist> [<caminho-do-dist> ...]" >&2
  exit 2
fi

FAILURES=0
SAFE_PLACEHOLDER_RE='^troque|^changeme$|^change_me$|^senha_forte$|^xxxxxxxx+$|^<.*>$|^\$\{.*\}$'

for dist in "$@"; do
  echo "==> Verificando $dist"

  if [ ! -d "$dist" ]; then
    echo "FALHA: diretório não encontrado: $dist"
    FAILURES=$((FAILURES + 1))
    continue
  fi

  echo "  [1/2] URL absoluta localhost:<porta>..."
  LOCALHOST_HITS=$(grep -rEo 'https?://localhost:[0-9]+' "$dist" 2>/dev/null || true)
  if [ -n "$LOCALHOST_HITS" ]; then
    echo "FALHA: destino operacional localhost:<porta> encontrado no bundle:"
    echo "$LOCALHOST_HITS" | sed 's/^/  /'
    FAILURES=$((FAILURES + 1))
  else
    echo "  OK"
  fi

  echo "  [2/2] segredo óbvio embutido..."
  SECRET_HITS=""
  while IFS= read -r -d '' f; do
    while IFS= read -r line; do
      if printf '%s' "$line" | grep -qE -- '-----BEGIN [A-Z ]*PRIVATE KEY|AKIA[0-9A-Z]{16}'; then
        SECRET_HITS="${SECRET_HITS}${f}: chave privada/AWS suspeita"$'\n'
        continue
      fi
      if printf '%s' "$line" | grep -qEi '[A-Z0-9_]*(PASSWORD|SECRET|API_?KEY|TOKEN)[A-Z0-9_]*["'"'"']?\s*[:=]\s*["'"'"'][^"'"'"']{12,}'; then
        SECRET_HITS="${SECRET_HITS}${f}: possível segredo embutido"$'\n'
      fi
    done < "$f"
  done < <(find "$dist" -type f \( -name '*.js' -o -name '*.css' \) -print0)

  if [ -n "$SECRET_HITS" ]; then
    echo "FALHA: possível segredo embutido no bundle:"
    printf '%s' "$SECRET_HITS" | sed 's/^/  /'
    FAILURES=$((FAILURES + 1))
  else
    echo "  OK"
  fi
done

echo
if [ "$FAILURES" -eq 0 ]; then
  echo "==> verificação de bundle passou."
  exit 0
else
  echo "==> $FAILURES verificação(ões) falharam."
  exit 1
fi
