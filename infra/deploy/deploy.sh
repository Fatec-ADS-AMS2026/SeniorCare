#!/usr/bin/env bash
# deploy.sh — publicação pull-based do SeniorCare no servidor.
#
# Uso:
#   ./deploy.sh <versao>     # ex.: ./deploy.sh 2026.08.0  → pull por digest + up + healthcheck
#   ./deploy.sh rollback     # volta para a versão anterior registrada
#   ./deploy.sh status       # versão corrente + estado dos containers
#
# Configuração (variáveis de ambiente ou infra/deploy/.state/client):
#   CLIENT           nome da pasta em clients/<CLIENT>/.env  — obrigatório
#   GHCR_USER/TOKEN  (opcional) faz docker login no GHCR; senão assume login prévio
#
# Modelo: build once/deploy many. O servidor NUNCA compila — só puxa imagem pronta.
# Ver docs/infra/ci-cd-arquitetura.md.

set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
REPO_ROOT=$(cd "$SCRIPT_DIR/../.." && pwd)
COMPOSE_FILE="$SCRIPT_DIR/docker-compose.yml"
RELEASES_DIR="$REPO_ROOT/releases"
CLIENTS_DIR="$SCRIPT_DIR/clients"
BACKUPS_DIR="$SCRIPT_DIR/backups"
STATE_DIR="$SCRIPT_DIR/.state"
BACKUP_KEEP=3          # mantém os N backups mais recentes (rollback offline)
HEALTH_TIMEOUT=300     # segundos aguardando healthchecks

log() { printf '\033[1;34m==>\033[0m %s\n' "$*"; }
err() { printf '\033[1;31mERRO:\033[0m %s\n' "$*" >&2; }
die() { err "$*"; exit 1; }

# ── Resolver cliente ─────────────────────────────────────────────────────
resolve_client() {
  CLIENT="${CLIENT:-}"
  if [ -z "$CLIENT" ] && [ -f "$STATE_DIR/client" ]; then
    CLIENT=$(cat "$STATE_DIR/client")
  fi
  [ -n "$CLIENT" ] || die "defina CLIENT (env) ou $STATE_DIR/client — nome da pasta em clients/"
  CLIENT_ENV="$CLIENTS_DIR/$CLIENT/.env"
  [ -f "$CLIENT_ENV" ] || die "config não encontrada: $CLIENT_ENV"
  mkdir -p "$STATE_DIR"; echo "$CLIENT" > "$STATE_DIR/client"
}

# Lê uma chave do .env do cliente (sem sourcing).
get_env() { grep -E "^$1=" "$CLIENT_ENV" 2>/dev/null | tail -1 | cut -d= -f2- || true; }

# docker compose com manifest de release + configuração do ambiente.
compose() {
  docker compose -f "$COMPOSE_FILE" --env-file "$RELEASE_ENV" --env-file "$CLIENT_ENV" "$@"
}

# ── Login opcional no GHCR ───────────────────────────────────────────────
ghcr_login() {
  if [ -n "${GHCR_USER:-}" ] && [ -n "${GHCR_TOKEN:-}" ]; then
    log "docker login ghcr.io (usuário $GHCR_USER)"
    echo "$GHCR_TOKEN" | docker login ghcr.io -u "$GHCR_USER" --password-stdin
  else
    log "GHCR_USER/GHCR_TOKEN não definidos — assumindo login prévio no ghcr.io"
  fi
}

# ── Backup do banco antes de mudar (rede de segurança do rollback) ───────
pg_backup() {
  local ver="$1" pg_cid
  pg_cid=$(compose ps -q postgres 2>/dev/null || true)
  if [ -z "$pg_cid" ]; then
    log "postgres não está rodando — pulando backup (provável 1º deploy)"
    return 0
  fi
  mkdir -p "$BACKUPS_DIR"
  local user db out
  user=$(get_env POSTGRES_USER); user=${user:-postgres}
  db=$(get_env POSTGRES_DB);     db=${db:-db_seniorcare}
  out="$BACKUPS_DIR/${ver}-pre-$(date -u +%Y%m%dT%H%M%SZ).sql"
  log "backup do banco -> $out"
  docker exec "$pg_cid" pg_dump -U "$user" -d "$db" > "$out" \
    || die "pg_dump falhou — abortando deploy (não vou subir sem backup)"
  # retém só os BACKUP_KEEP mais recentes
  ls -1t "$BACKUPS_DIR"/*.sql 2>/dev/null | tail -n +$((BACKUP_KEEP + 1)) | xargs -r rm -f
}

# ── Espera todos os healthchecks ficarem saudáveis ───────────────────────
wait_healthy() {
  local elapsed=0 ids id st pending unhealthy
  log "aguardando healthchecks (timeout ${HEALTH_TIMEOUT}s)..."
  while :; do
    ids=$(compose ps -q 2>/dev/null || true)
    pending=0; unhealthy=0
    for id in $ids; do
      st=$(docker inspect -f '{{if .State.Health}}{{.State.Health.Status}}{{else}}none{{end}}' "$id" 2>/dev/null || echo none)
      case "$st" in
        healthy|none) ;;
        starting)     pending=$((pending + 1)) ;;
        *)            unhealthy=$((unhealthy + 1)) ;;
      esac
    done
    if [ "$pending" -eq 0 ] && [ "$unhealthy" -eq 0 ]; then
      log "todos os serviços saudáveis"; return 0
    fi
    if [ "$elapsed" -ge "$HEALTH_TIMEOUT" ]; then
      err "timeout aguardando healthcheck (pendentes=$pending, não-saudáveis=$unhealthy)"
      compose ps; return 1
    fi
    sleep 5; elapsed=$((elapsed + 5))
  done
}

# ── Registro de estado e histórico ───────────────────────────────────────
record_release() {
  local ver="$1"
  if [ -f "$STATE_DIR/current-release" ]; then
    cp "$STATE_DIR/current-release" "$STATE_DIR/previous-release"
  fi
  echo "$ver" > "$STATE_DIR/current-release"
  local hist="$CLIENTS_DIR/$CLIENT/deploy-history.md"
  [ -f "$hist" ] || printf '# Histórico de deploys — %s\n\n| Data (UTC) | Versão | Resultado |\n|---|---|---|\n' "$CLIENT" > "$hist"
  printf '| %s | %s | %s |\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$ver" "${2:-OK}" >> "$hist"
}

# ── Fluxo principal de deploy ────────────────────────────────────────────
do_deploy() {
  local ver="$1"
  RELEASE_ENV="$RELEASES_DIR/${ver}.env"
  [ -f "$RELEASE_ENV" ] || die "manifest do release não encontrado: $RELEASE_ENV"

  log "deploy do SeniorCare $ver — cliente $CLIENT"
  ghcr_login
  pg_backup "$ver"
  log "puxando imagens (por digest)..."
  compose pull
  log "subindo containers..."
  compose up -d --remove-orphans
  if wait_healthy; then
    record_release "$ver" "OK"
    log "deploy $ver concluído."
    compose ps
  else
    record_release "$ver" "FALHOU-HEALTHCHECK"
    die "deploy $ver não ficou saudável — considere ./deploy.sh rollback"
  fi
}

do_rollback() {
  local prev
  prev=$(cat "$STATE_DIR/previous-release" 2>/dev/null || true)
  [ -n "$prev" ] || die "sem versão anterior registrada em $STATE_DIR/previous-release"
  log "rollback para $prev"
  do_deploy "$prev"
}

do_status() {
  local cur
  cur=$(cat "$STATE_DIR/current-release" 2>/dev/null || echo "(desconhecida)")
  log "versão corrente: $cur   |   cliente: $CLIENT"
  RELEASE_ENV="$RELEASES_DIR/${cur}.env"
  if [ -f "$RELEASE_ENV" ]; then compose ps; else docker ps --filter "name=seniorcare"; fi
}

# ── Dispatch ─────────────────────────────────────────────────────────────
main() {
  command -v docker >/dev/null || die "docker não encontrado no servidor"
  resolve_client
  local cmd="${1:-}"
  case "$cmd" in
    rollback) do_rollback ;;
    status)   do_status ;;
    "")       die "uso: ./deploy.sh <versao> | rollback | status" ;;
    *)        do_deploy "$cmd" ;;
  esac
}

main "$@"
