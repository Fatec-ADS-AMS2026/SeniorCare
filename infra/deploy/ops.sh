#!/usr/bin/env bash
# ops.sh — sobe/gere a camada de operação (docker-compose.ops.yml).
#
# Roda como PROJETO compose separado (seniorcare-ops) para não interferir com o
# deploy.sh da stack base (o --remove-orphans do deploy nunca vê estes containers).
#
# Uso:
#   ./ops.sh up        # sobe portainer, dozzle, caddy, backup
#   ./ops.sh down      # derruba a camada de operação (não toca na stack base)
#   ./ops.sh status    # estado dos containers de operação
#   ./ops.sh logs [svc]
#   ./ops.sh pull      # atualiza as imagens das ferramentas

set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
PROJECT="seniorcare-ops"
COMPOSE_FILE="$SCRIPT_DIR/docker-compose.ops.yml"
ENV_FILE="$SCRIPT_DIR/.env.ops"
NET="seniorcare-net"

log() { printf '\033[1;34m==>\033[0m %s\n' "$*"; }
die() { printf '\033[1;31mERRO:\033[0m %s\n' "$*" >&2; exit 1; }

compose() {
  local args=(-p "$PROJECT" -f "$COMPOSE_FILE" --profile ops)
  [ -f "$ENV_FILE" ] && args+=(--env-file "$ENV_FILE")
  docker compose "${args[@]}" "$@"
}

preflight() {
  command -v docker >/dev/null || die "docker não encontrado"
  [ -f "$ENV_FILE" ] || die "faltou $ENV_FILE — rode: cp .env.ops.example .env.ops (e ajuste)"
  if ! docker network inspect "$NET" >/dev/null 2>&1; then
    die "rede '$NET' não existe — suba a stack base primeiro (./deploy.sh <versao>)"
  fi
}

case "${1:-}" in
  up)
    preflight
    log "subindo camada de operação (projeto $PROJECT)..."
    compose up -d
    compose ps
    ;;
  down)
    log "derrubando camada de operação (a stack base NÃO é afetada)..."
    compose down
    ;;
  status) compose ps ;;
  logs)   shift; compose logs -f "$@" ;;
  pull)   preflight; compose pull ;;
  *) die "uso: ./ops.sh up | down | status | logs [svc] | pull" ;;
esac
