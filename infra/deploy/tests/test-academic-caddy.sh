#!/usr/bin/env bash
set -euo pipefail

ROOT=$(cd "$(dirname "$0")/../../.." && pwd)
PREFIX="seniorcare-caddy-test-$$"
NETWORK="$PREFIX"
CADDY="$PREFIX-caddy"
API="$PREFIX-api"
CARE="$PREFIX-care"
STOCK="$PREFIX-stock"
PORTAL="$PREFIX-portal"

cleanup() {
  docker rm -f "$CADDY" "$API" "$CARE" "$STOCK" "$PORTAL" >/dev/null 2>&1 || true
  docker network rm "$NETWORK" >/dev/null 2>&1 || true
}
trap cleanup EXIT

command -v docker >/dev/null || { echo "docker não encontrado" >&2; exit 2; }
command -v curl >/dev/null || { echo "curl não encontrado" >&2; exit 2; }

docker network create "$NETWORK" >/dev/null
docker run -d --rm --name "$API" --network "$NETWORK" --network-alias seniorcare-academico-api \
  hashicorp/http-echo:1.0.0 -listen :8080 -text api >/dev/null
docker run -d --rm --name "$CARE" --network "$NETWORK" --network-alias seniorcare-academico-care-web \
  hashicorp/http-echo:1.0.0 -listen :80 -text care >/dev/null
docker run -d --rm --name "$STOCK" --network "$NETWORK" --network-alias seniorcare-academico-stock-web \
  hashicorp/http-echo:1.0.0 -listen :80 -text stock >/dev/null
docker run -d --rm --name "$PORTAL" --network "$NETWORK" --network-alias seniorcare-academico-portal \
  hashicorp/http-echo:1.0.0 -listen :80 -text portal >/dev/null
docker run -d --rm --name "$CADDY" --network "$NETWORK" -p "127.0.0.1::443" \
  -v "$ROOT/infra/deploy/academico/Caddyfile:/etc/caddy/Caddyfile:ro" \
  -e ACADEMIC_HOSTNAME=seniorcare.test caddy:2.8.4-alpine >/dev/null
PORT=$(docker port "$CADDY" 443/tcp | sed 's/.*://')

url() { printf 'https://seniorcare.test:%s%s' "$PORT" "$1"; }
request() {
  curl --noproxy '*' --silent --show-error --fail --insecure --resolve "seniorcare.test:${PORT}:127.0.0.1" "$(url "$1")"
}

ready=false
for _ in $(seq 1 20); do
  if request / >/dev/null 2>&1; then ready=true; break; fi
  sleep 1
done
$ready || { docker logs "$CADDY" >&2; exit 1; }
[ "$(request /)" = portal ]
[ "$(request /api/v1/health)" = api ]
[ "$(request /care/assets/app.js)" = care ]
[ "$(request /stock/assets/app.js)" = stock ]

headers=$(curl --noproxy '*' --silent --show-error --insecure --head --resolve "seniorcare.test:${PORT}:127.0.0.1" "$(url /)")
printf '%s\n' "$headers" | grep -qi '^X-Frame-Options: DENY'
printf '%s\n' "$headers" | grep -qi '^X-Content-Type-Options: nosniff'

redirect=$(curl --noproxy '*' --silent --show-error --insecure --head --resolve "seniorcare.test:${PORT}:127.0.0.1" "$(url /care)")
printf '%s\n' "$redirect" | grep -qi "^location: /care/"

printf 'OK: Caddy acadêmico roteia HTTPS, caminhos-base, assets e cabeçalhos.\n'
