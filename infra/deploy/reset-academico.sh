#!/usr/bin/env bash
# Reset destrutivo somente para a stack acadêmica identificada explicitamente.
set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
CLIENT=${CLIENT:-}
PROJECT=seniorcare-academico
EXPECTED_DB=db_seniorcare_academico
EXPECTED_VOLUME_PATH=/opt/seniorcare-academico/postgres
ENV_FILE="$SCRIPT_DIR/clients/academico/.env"

# Apenas os testes podem substituir os alvos fixos; o caminho de produção nunca
# aceita alvo destrutivo parametrizável.
if [ "${RESET_TEST_MODE:-}" = 1 ]; then
  EXPECTED_DB=${RESET_TEST_DB:?}
  EXPECTED_VOLUME_PATH=${RESET_TEST_VOLUME:?}
  ENV_FILE=${RESET_TEST_ENV_FILE:?}
fi

fail() { echo "ERRO: $*" >&2; exit 1; }
[ "$CLIENT" = academico ] || fail "reset permitido somente com CLIENT=academico"
[ "${1:-}" = --confirm ] && [ "${2:-}" = "$PROJECT" ] || fail "uso: CLIENT=academico $0 --confirm $PROJECT"

[ -f "$ENV_FILE" ] || fail "configuração acadêmica ausente"
value() { grep -E "^$1=" "$ENV_FILE" | tail -1 | cut -d= -f2-; }
[ "$(value POSTGRES_DB)" = "$EXPECTED_DB" ] || fail "banco acadêmico divergente"
[ "$(value ACADEMIC_POSTGRES_DATA_PATH)" = "$EXPECTED_VOLUME_PATH" ] || fail "volume acadêmico divergente"

docker compose -p "$PROJECT" -f "$SCRIPT_DIR/docker-compose.yml" -f "$SCRIPT_DIR/docker-compose.homolog.yml" --env-file "$ENV_FILE" down
rm -rf -- "$EXPECTED_VOLUME_PATH"
mkdir -p "$EXPECTED_VOLUME_PATH"
echo "Reset acadêmico concluído; execute o seed acadêmico aprovado separadamente."
