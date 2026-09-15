#!/usr/bin/env bash
set -euo pipefail
ROOT=$(mktemp -d)
trap 'rm -rf "$ROOT"' EXIT
ENV_FILE="$ROOT/academico.env"
VOLUME="$ROOT/postgres"
LOG="$ROOT/calls"
printf 'POSTGRES_DB=db_seniorcare_academico\nACADEMIC_POSTGRES_DATA_PATH=%s\n' "$VOLUME" > "$ENV_FILE"
mkdir -p "$ROOT/bin" "$VOLUME"
cat > "$ROOT/bin/docker" <<EOF
#!/usr/bin/env bash
echo docker >> "$LOG"
EOF
cat > "$ROOT/bin/rm" <<EOF
#!/usr/bin/env bash
echo rm >> "$LOG"
command /bin/rm "\$@"
EOF
cat > "$ROOT/bin/mkdir" <<EOF
#!/usr/bin/env bash
echo mkdir >> "$LOG"
command /bin/mkdir "\$@"
EOF
chmod +x "$ROOT/bin"/*
run() { PATH="$ROOT/bin:$PATH" RESET_TEST_MODE=1 RESET_TEST_DB=db_seniorcare_academico RESET_TEST_VOLUME="$VOLUME" RESET_TEST_ENV_FILE="$ENV_FILE" CLIENT=academico infra/deploy/reset-academico.sh "$@"; }
if run --confirm outro; then exit 1; fi
[ ! -e "$LOG" ] || exit 1
printf 'POSTGRES_DB=db_seniorcare_academico\nACADEMIC_POSTGRES_DATA_PATH=%s\n' "$ROOT/outro" > "$ENV_FILE"
if run --confirm seniorcare-academico; then exit 1; fi
[ ! -e "$LOG" ] || exit 1
printf 'POSTGRES_DB=db_seniorcare_academico\nACADEMIC_POSTGRES_DATA_PATH=%s\n' "$VOLUME" > "$ENV_FILE"
run --confirm seniorcare-academico
[ -d "$VOLUME" ] && grep -qx docker "$LOG" && grep -qx rm "$LOG" && grep -qx mkdir "$LOG"
echo 'OK: reset recusa divergências sem comandos destrutivos e aceita alvo acadêmico efêmero.'
