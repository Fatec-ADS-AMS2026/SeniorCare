#!/usr/bin/env bash
set -euo pipefail
ROOT=$(mktemp -d)
trap 'rm -rf "$ROOT"' EXIT
ENV_FILE="$ROOT/academico.env"
STATE_DIR="$ROOT/state"
RELEASES_DIR="$ROOT/releases"
VOLUME="$ROOT/postgres"
LOG="$ROOT/docker.calls"
mkdir -p "$ROOT/bin" "$STATE_DIR" "$RELEASES_DIR" "$VOLUME"
printf '2026.09.0\n' > "$STATE_DIR/current-release"
printf 'API_IMAGE=ghcr.io/example/api@sha256:%064d\n' 0 > "$RELEASES_DIR/2026.09.0.env"
printf 'POSTGRES_DB=db_seniorcare_academico\nACADEMIC_POSTGRES_DATA_PATH=%s\nBootstrap__RunOnStartup=false\n' "$VOLUME" > "$ENV_FILE"
cat > "$ROOT/bin/docker" <<EOF
#!/usr/bin/env bash
printf '%s\n' "\$*" >> "$LOG"
EOF
chmod +x "$ROOT/bin/docker"
run() {
  PATH="$ROOT/bin:$PATH" \
    SEED_TEST_MODE=1 \
    SEED_TEST_DB=db_seniorcare_academico \
    SEED_TEST_VOLUME="$VOLUME" \
    SEED_TEST_ENV_FILE="$ENV_FILE" \
    SEED_TEST_STATE_DIR="$STATE_DIR" \
    SEED_TEST_RELEASES_DIR="$RELEASES_DIR" \
    CLIENT=academico infra/deploy/seed-academico.sh
}

printf 'POSTGRES_DB=outro\nACADEMIC_POSTGRES_DATA_PATH=%s\nBootstrap__RunOnStartup=false\n' "$VOLUME" > "$ENV_FILE"
if run; then exit 1; fi
[ ! -e "$LOG" ] || exit 1
printf 'POSTGRES_DB=db_seniorcare_academico\nACADEMIC_POSTGRES_DATA_PATH=%s\nBootstrap__RunOnStartup=true\n' "$VOLUME" > "$ENV_FILE"
if run; then exit 1; fi
[ ! -e "$LOG" ] || exit 1
printf 'POSTGRES_DB=db_seniorcare_academico\nACADEMIC_POSTGRES_DATA_PATH=%s\nBootstrap__RunOnStartup=false\n' "$VOLUME" > "$ENV_FILE"
run
grep -Fqx 'compose -p seniorcare-academico -f '"$(pwd)"'/infra/deploy/docker-compose.yml -f '"$(pwd)"'/infra/deploy/docker-compose.homolog.yml --env-file '"$RELEASES_DIR"'/2026.09.0.env --env-file '"$ENV_FILE"' up -d postgres mailpit' "$LOG"
grep -Fqx 'compose -p seniorcare-academico -f '"$(pwd)"'/infra/deploy/docker-compose.yml -f '"$(pwd)"'/infra/deploy/docker-compose.homolog.yml --env-file '"$RELEASES_DIR"'/2026.09.0.env --env-file '"$ENV_FILE"' run --rm --no-deps seniorcare-api --academic-seed' "$LOG"
echo 'OK: seed acadêmico falha fora do alvo esperado e executa carga explícita idempotente.'
