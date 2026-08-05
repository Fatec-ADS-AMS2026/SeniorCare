#!/bin/bash
# check-env-hygiene.sh
#
# Verifica que:
#   1. nenhum arquivo .env "real" (com segredo de ambiente) está rastreado no git —
#      só *.example (template sem segredo) pode estar versionado;
#   2. os arquivos *.example rastreados não contêm valores que pareçam segredo real
#      (chave privada, JWT, credencial AWS, string de alta entropia), só placeholders.
#
# Uso: ./check-env-hygiene.sh   (roda a partir de qualquer diretório do repo)
# Sem dependências além de git/grep — usado tanto localmente quanto no CI (ci.yml).

set -euo pipefail

REPO_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$REPO_DIR"

FAILURES=0

echo "==> [1/2] Verificando que nenhum .env real está rastreado no git..."

UNTRACKED_VIOLATIONS=()
while IFS= read -r f; do
  [ -z "$f" ] && continue
  case "$f" in
    *.example|*.example.*)
      # template sem segredo — permitido
      ;;
    *)
      UNTRACKED_VIOLATIONS+=("$f")
      ;;
  esac
done < <(git ls-files | grep -E '(^|/)\.env(\.[A-Za-z0-9_.]+)?$' || true)

if [ "${#UNTRACKED_VIOLATIONS[@]}" -gt 0 ]; then
  echo "FALHA: arquivo(s) .env real(is) rastreado(s) no git (deveriam estar só no .gitignore/local):"
  printf '  - %s\n' "${UNTRACKED_VIOLATIONS[@]}"
  FAILURES=$((FAILURES + 1))
else
  echo "OK: nenhum .env real rastreado — só *.example"
fi

echo ""
echo "==> [2/2] Verificando que os *.example não contêm segredo real..."

EXAMPLE_FILES=()
while IFS= read -r f; do
  [ -z "$f" ] && continue
  EXAMPLE_FILES+=("$f")
done < <(git ls-files | grep -E '\.env(\.[A-Za-z0-9_]+)*\.example(\.[a-z0-9]+)?$' || true)

# Placeholders literais conhecidos — não é segredo real. Deliberadamente restrito (sem
# termos genéricos como "example"/"sample", que também aparecem dentro de segredos de
# teste/documentação de terceiros, ex.: a access key de exemplo da AWS contém "EXAMPLE").
SAFE_PLACEHOLDER_RE='^troque|^changeme$|^change_me$|^senha_forte$|^xxxxxxxx+$|^<.*>$|^\$\{.*\}$'

SECRET_VIOLATIONS=()
if [ "${#EXAMPLE_FILES[@]}" -gt 0 ]; then
  for f in "${EXAMPLE_FILES[@]}"; do
    while IFS= read -r line; do
      if printf '%s' "$line" | grep -qE -- '-----BEGIN [A-Z ]*PRIVATE KEY|eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}|AKIA[0-9A-Z]{16}'; then
        SECRET_VIOLATIONS+=("$f: $line")
        continue
      fi

      if printf '%s' "$line" | grep -qEi '^[A-Z0-9_]*(PASSWORD|SECRET|TOKEN|CREDENTIAL|_KEY)[A-Z0-9_]*='; then
        value="${line#*=}"
        if [ -n "$value" ] && [ "${#value}" -ge 12 ] && ! printf '%s' "$value" | grep -qEi "$SAFE_PLACEHOLDER_RE"; then
          SECRET_VIOLATIONS+=("$f: $line")
        fi
      fi
    done < "$f"
  done
fi

if [ "${#SECRET_VIOLATIONS[@]}" -gt 0 ]; then
  echo "FALHA: possível segredo real encontrado em arquivo .example:"
  printf '  - %s\n' "${SECRET_VIOLATIONS[@]}"
  FAILURES=$((FAILURES + 1))
else
  echo "OK: ${#EXAMPLE_FILES[@]} arquivo(s) .example verificado(s), nenhum segredo real encontrado"
fi

echo ""
if [ "$FAILURES" -eq 0 ]; then
  echo "==> verificação de higiene de .env passou."
  exit 0
else
  echo "==> $FAILURES verificação(ões) falharam."
  exit 1
fi
