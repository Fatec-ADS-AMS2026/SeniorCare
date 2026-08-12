#!/bin/bash
# check-clinical-scope.sh
#
# Guard de regressão pra tarefa de aceite 13.5 (`stabilize-existing-platform`):
# "confirmar que profissão não concede acesso e que nenhum dado clínico,
# prontuário, dashboard ou assinatura foi introduzido por esta mudança". Essa
# mudança comprovadamente NÃO introduz nenhum desses conceitos — este script
# não corrige um problema, trava contra um futuro PR introduzir escopo clínico/
# assistencial sem passar por uma decisão OpenSpec explícita.
#
# Escopo deliberadamente restrito ao CÓDIGO-FONTE de implementação (backend
# WebAPI + src/ dos dois front-ends) — não ao repo inteiro. Documentos de
# escopo/roadmap (docs/escopo-do-projeto.md, o relatório de avaliação, o change
# `introduce-senior-portal`) DISCUTEM esse escopo futuro de propósito; não são
# o alvo desta checagem, que é sobre o que foi IMPLEMENTADO nesta mudança.
#
# Uso: ./check-clinical-scope.sh   (roda a partir de qualquer diretório do repo)

set -euo pipefail

REPO_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$REPO_DIR"

SCAN_PATHS=(
  "SeniorCareManager-Backend/SeniorCareManager.WebAPI"
  "SeniorCareManager-Frontend/SeniorCareManagerFrontend/src"
  "SeniorStockManager-Frontend/SeniorStockManagerFrontend/src"
)

# Termos que indicariam escopo clínico/assistencial ou de profissão-como-acesso
# entrando no código de implementação. Case-insensitive.
FORBIDDEN_RE='prontu[aá]rio|medicalrecord|clinicaldata|dashboard|assinatura|e?signature|profiss[aã]o|conselho de classe|conselho profissional'

echo "==> Verificando que nenhum escopo clínico/assistencial foi introduzido no código..."

FAILURES=()
for path in "${SCAN_PATHS[@]}"; do
  [ -d "$path" ] || continue
  while IFS=: read -r file line match; do
    [ -z "$match" ] && continue
    # clinical-scope:allow — mesma convenção do gitleaks:allow já usada no repo. Só
    # suprime a linha marcada explicitamente (nunca o arquivo inteiro), então continua
    # exigindo revisão humana pra cada novo uso — não abre uma exceção geral.
    grep -qF 'clinical-scope:allow' <<<"$(sed -n "${line}p" "$file")" && continue
    FAILURES+=("$file:$line: $match")
  done < <(grep -rnoiE "$FORBIDDEN_RE" "$path" \
             --include='*.cs' --include='*.ts' --include='*.tsx' 2>/dev/null || true)
done

if [ "${#FAILURES[@]}" -gt 0 ]; then
  echo "FALHA: termo de escopo clínico/assistencial encontrado no código de implementação:"
  printf '  - %s\n' "${FAILURES[@]}"
  echo ""
  echo "Esta mudança (stabilize-existing-platform) não introduz prontuário, dado clínico,"
  echo "dashboard, assinatura eletrônica nem profissão-como-acesso. Se isso é intencional,"
  echo "deveria estar numa mudança OpenSpec própria, não deslizar aqui."
  exit 1
fi

echo "OK: nenhum termo de escopo clínico/assistencial encontrado no código de implementação."
exit 0
