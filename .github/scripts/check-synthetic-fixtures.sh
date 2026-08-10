#!/bin/bash
# check-synthetic-fixtures.sh
#
# Verifica que fixtures/seeds/evidências de teste (backend e os dois
# front-ends) só usam endereços de e-mail sintéticos — nenhum residente,
# familiar, trabalhador ou doador real (spec automated-quality-gates:
# "Fixtures, seeds e evidências de teste SHALL usar dados sintéticos").
#
# Checagem automatizável e de alto sinal: e-mail é o campo de dado pessoal
# mais previsível de detectar mecanicamente (nome/CPF sintéticos não têm um
# formato verificável por regex — a convenção do projeto já os fabrica
# manualmente). Exige que todo endereço de e-mail encontrado em arquivo de
# teste/fixture use um domínio reservado para documentação/teste (RFC 2606:
# example.com/.org/.net, *.example, *.test, *.invalid; RFC 6762: *.local) ou
# a convenção já usada nos .env.example do projeto (exemplo.com.br).
#
# Escopo deliberadamente restrito aos diretórios de teste/fixture — não
# escaneia o repo inteiro, pra não gerar falso positivo em endereço de
# exemplo usado em documentação (README, CONFIGURATION.md etc.), que não é
# fixture nem seed.
#
# Uso: ./check-synthetic-fixtures.sh   (roda a partir de qualquer diretório do repo)

set -euo pipefail

REPO_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$REPO_DIR"

FIXTURE_PATHS=(
  "SeniorCareManager-Backend/SeniorCareManager.IntegrationTests"
  "SeniorCareManager-Backend/SeniorCareManager.UnitTests"
  "SeniorCareManager-Frontend/SeniorCareManagerFrontend/src"
  "SeniorStockManager-Frontend/SeniorStockManagerFrontend/src"
)

# Domínio reservado pra documentação/teste (RFC 2606 + RFC 6762) ou convenção
# já usada nos .env.example do projeto — qualquer outro domínio reprova.
ALLOWED_DOMAIN_RE='^example\.(com|org|net)$|\.(example|test|invalid|local)$|^exemplo\.com\.br$|\.exemplo\.com\.br$'

EMAIL_RE='[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}'

echo "==> Verificando que fixtures/seeds de teste só usam e-mail sintético..."

VIOLATIONS=()
for path in "${FIXTURE_PATHS[@]}"; do
  [ -d "$path" ] || continue
  while IFS=: read -r file line email; do
    [ -z "$email" ] && continue
    domain="${email#*@}"
    if ! printf '%s' "$domain" | grep -qEi "$ALLOWED_DOMAIN_RE"; then
      VIOLATIONS+=("$file:$line: $email")
    fi
  done < <(grep -rnoE "$EMAIL_RE" "$path" \
             --include='*.cs' --include='*.ts' --include='*.tsx' 2>/dev/null || true)
done

if [ "${#VIOLATIONS[@]}" -gt 0 ]; then
  echo "FALHA: e-mail com domínio fora da allowlist sintética encontrado em fixture/seed de teste:"
  printf '  - %s\n' "${VIOLATIONS[@]}"
  echo ""
  echo "Use um domínio reservado (example.com/.org/.net, *.example, *.test, *.invalid, *.local) ou exemplo.com.br."
  exit 1
fi

echo "OK: nenhum e-mail fora da allowlist sintética encontrado em fixture/seed de teste."
exit 0
