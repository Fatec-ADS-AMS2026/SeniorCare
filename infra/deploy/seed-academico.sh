#!/usr/bin/env bash
# Carga didática explícita: cria somente a instituição e o administrador sintéticos
# configurados no .env. Nunca é acionada pelo startup normal da stack acadêmica.
set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
REPO_ROOT=$(cd "$SCRIPT_DIR/../.." && pwd)
CLIENT=${CLIENT:-}
PROJECT=seniorcare-academico
EXPECTED_DB=db_seniorcare_academico
EXPECTED_VOLUME_PATH=/opt/seniorcare-academico/postgres
ENV_FILE="$SCRIPT_DIR/clients/academico/.env"
STATE_DIR="$SCRIPT_DIR/.state"
RELEASES_DIR="$REPO_ROOT/releases"

# Apenas os testes podem substituir alvos e diretórios; o fluxo de produção usa
# identificadores fixos do ambiente acadêmico.
if [ "${SEED_TEST_MODE:-}" = 1 ]; then
  EXPECTED_DB=${SEED_TEST_DB:?}
  EXPECTED_VOLUME_PATH=${SEED_TEST_VOLUME:?}
  ENV_FILE=${SEED_TEST_ENV_FILE:?}
  STATE_DIR=${SEED_TEST_STATE_DIR:?}
  RELEASES_DIR=${SEED_TEST_RELEASES_DIR:?}
fi

fail() { echo "ERRO: $*" >&2; exit 1; }
[ "$CLIENT" = academico ] || fail "seed permitido somente com CLIENT=academico"
[ -f "$ENV_FILE" ] || fail "configuração acadêmica ausente"
value() { grep -E "^$1=" "$ENV_FILE" | tail -1 | cut -d= -f2-; }
[ "$(value POSTGRES_DB)" = "$EXPECTED_DB" ] || fail "banco acadêmico divergente"
[ "$(value ACADEMIC_POSTGRES_DATA_PATH)" = "$EXPECTED_VOLUME_PATH" ] || fail "volume acadêmico divergente"
[ "$(value Bootstrap__RunOnStartup)" = false ] || fail "Bootstrap__RunOnStartup deve ser false no ambiente acadêmico"

release=$(cat "$STATE_DIR/current-release" 2>/dev/null || true)
[ -n "$release" ] || fail "nenhum release acadêmico foi implantado"
RELEASE_ENV="$RELEASES_DIR/${release}.env"
[ -f "$RELEASE_ENV" ] || fail "manifesto do release atual não encontrado: $RELEASE_ENV"

compose=(docker compose -p "$PROJECT" -f "$SCRIPT_DIR/docker-compose.yml" -f "$SCRIPT_DIR/docker-compose.homolog.yml" --env-file "$RELEASE_ENV" --env-file "$ENV_FILE")
"${compose[@]}" up -d postgres mailpit
"${compose[@]}" run --rm --no-deps seniorcare-api --academic-seed

echo "Seed acadêmico concluído para $PROJECT com o release $release."
