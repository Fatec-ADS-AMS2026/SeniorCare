#!/usr/bin/env bash
# smoke-test.sh — §8.7. Roda contra a stack docker-test já de pé (docker compose up
# -d --build) e valida, ponta a ponta, os cenários que uma checagem de tipo/unit não
# cobre: raiz, assets, refresh de deep link, navegação entre módulos, MFA, manutenção,
# logout e falha segura do IAM. Não é um teste automatizado de CI (não builda nem sobe a
# stack sozinho) — é a checagem manual/pré-deploy da imagem de produção já construída.
#
# Uso:
#   cd infra/docker-test && docker compose up -d --build
#   ./smoke-test.sh
#
# Variáveis de ambiente (todas opcionais, com default):
#   API_BASE, CARE_BASE, STOCK_BASE, PORTAL_BASE   default localhost:8080/3000/3001/3002
#   ADMIN_EMAIL, ADMIN_PASSWORD                     default admin@example.com / DevSenhaForte!2026

set -uo pipefail

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
COOKIE_JAR=$(mktemp)
trap 'rm -f "$COOKIE_JAR"' EXIT

API_BASE="${API_BASE:-http://localhost:8080}"
CARE_BASE="${CARE_BASE:-http://localhost:3000}"
STOCK_BASE="${STOCK_BASE:-http://localhost:3001}"
PORTAL_BASE="${PORTAL_BASE:-http://localhost:3002}"
ADMIN_EMAIL="${ADMIN_EMAIL:-admin@example.com}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-DevSenhaForte!2026}"

PASS=0
FAIL=0

log() { printf '\033[1;34m==>\033[0m %s\n' "$*"; }
ok()  { PASS=$((PASS + 1)); printf '\033[1;32m  OK\033[0m %s\n' "$*"; }
bad() { FAIL=$((FAIL + 1)); printf '\033[1;31m  FALHA\033[0m %s\n' "$*"; }

totp() {
  python3 - "$1" <<'PYEOF'
import base64, hmac, hashlib, struct, time, sys
secret = sys.argv[1]
key = base64.b32decode(secret.upper() + '=' * (-len(secret) % 8))
msg = struct.pack('>Q', int(time.time() // 30))
h = hmac.new(key, msg, hashlib.sha1).digest()
o = h[-1] & 0x0f
code = (struct.unpack('>I', h[o:o+4])[0] & 0x7fffffff) % 10**6
print(str(code).zfill(6))
PYEOF
}

json_get() {
  python3 -c "import sys,json; print(json.loads(sys.argv[1]).get(sys.argv[2],''))" "$1" "$2" 2>/dev/null || echo ""
}

http_code() { curl -sS -o /dev/null -w '%{http_code}' "$@"; }

check_eq() {
  local desc="$1" expected="$2" actual="$3"
  if [ "$actual" = "$expected" ]; then
    ok "$desc (HTTP $actual)"
  else
    bad "$desc — esperado HTTP $expected, veio $actual"
  fi
}

# ── 0. API pronta ─────────────────────────────────────────────────────────
log "aguardando $API_BASE/health/ready..."
for _ in $(seq 1 60); do
  curl -fsS "$API_BASE/health/ready" >/dev/null 2>&1 && break
  sleep 2
done
curl -fsS "$API_BASE/health/ready" >/dev/null 2>&1 || { bad "API não ficou pronta"; exit 1; }
ok "API pronta"

# ── 1. Raiz — os três front-ends ─────────────────────────────────────────
log "1/8 raiz"
check_eq "GET $CARE_BASE/" 200 "$(http_code "$CARE_BASE/")"
check_eq "GET $STOCK_BASE/" 200 "$(http_code "$STOCK_BASE/")"
check_eq "GET $PORTAL_BASE/" 200 "$(http_code "$PORTAL_BASE/")"

# ── 2. Assets — extrai o primeiro asset hasheado do index.html e busca ──
log "2/8 assets"
for name in "care|$CARE_BASE" "stock|$STOCK_BASE" "portal|$PORTAL_BASE"; do
  IFS='|' read -r app base <<<"$name"
  index=$(curl -sS "$base/")
  asset=$(printf '%s' "$index" | grep -oE '(src|href)="[^"]*assets/[^"]*"' | head -1 | sed -E 's/^(src|href)="//; s/"$//')
  if [ -z "$asset" ]; then
    bad "$app: nenhum asset encontrado em index.html"
    continue
  fi
  code=$(http_code "$base$asset")
  check_eq "$app asset $asset" 200 "$code"
done

# ── 3. Refresh de deep link — SPA fallback (try_files), não 404 cru ─────
log "3/8 refresh de deep link"
for name in "care|$CARE_BASE|/residents/999" "stock|$STOCK_BASE|/products/999" "portal|$PORTAL_BASE|/profile"; do
  IFS='|' read -r app base path <<<"$name"
  code=$(http_code "$base$path")
  check_eq "$app deep link $path cai no shell da SPA" 200 "$code"
done

# ── 4. Ativação (primeiro acesso) + Login + MFA (enroll/confirm) ────────
log "4/8 MFA"
if docker ps --format '{{.Names}}' 2>/dev/null | grep -q '^seniorcare-api$'; then
  ACTIVATION_TOKEN=$(docker logs seniorcare-api 2>&1 | grep "Token de ativação" | tail -1 | sed 's/^.*: //')
  if [ -n "$ACTIVATION_TOKEN" ]; then
    ACTIVATE_CODE=$(curl -sS -o /dev/null -w '%{http_code}' -X POST "$API_BASE/api/v1/Auth/activate" \
      -H "Content-Type: application/json" \
      -d "{\"email\":\"$ADMIN_EMAIL\",\"token\":\"$ACTIVATION_TOKEN\",\"newPassword\":\"$ADMIN_PASSWORD\"}")
    if [ "$ACTIVATE_CODE" = "200" ]; then
      ok "conta ativada (primeiro acesso)"
    else
      log "ativação retornou HTTP $ACTIVATE_CODE — provavelmente já ativa; seguindo pro login"
    fi
  fi
fi

LOGIN_RESP=$(curl -sS -c "$COOKIE_JAR" -X POST "$API_BASE/api/v1/Auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$ADMIN_EMAIL\",\"password\":\"$ADMIN_PASSWORD\"}")
STATUS=$(json_get "$LOGIN_RESP" status)

case "$STATUS" in
  ok)
    ok "login sem exigir novo MFA (já cadastrado nesta execução)"
    ;;
  mfa_enrollment_required)
    CHALLENGE_TOKEN=$(json_get "$LOGIN_RESP" challengeToken)
    ENROLL_RESP=$(curl -sS -c "$COOKIE_JAR" -b "$COOKIE_JAR" -X POST "$API_BASE/api/v1/Auth/mfa/enroll" \
      -H "Content-Type: application/json" -d "{\"challengeToken\":\"$CHALLENGE_TOKEN\"}")
    KEY=$(json_get "$ENROLL_RESP" authenticatorKey)
    if [ -z "$KEY" ]; then
      bad "enroll de MFA não devolveu authenticatorKey — resposta: $ENROLL_RESP"
    else
      CODE=$(totp "$KEY")
      CONFIRM_RESP=$(curl -sS -c "$COOKIE_JAR" -b "$COOKIE_JAR" -X POST "$API_BASE/api/v1/Auth/mfa/confirm" \
        -H "Content-Type: application/json" -d "{\"challengeToken\":\"$CHALLENGE_TOKEN\",\"code\":\"$CODE\"}")
      if python3 -c "import sys,json; json.loads(sys.argv[1])['identity']" "$CONFIRM_RESP" >/dev/null 2>&1; then
        ok "MFA cadastrado e confirmado (enroll -> TOTP -> confirm)"
      else
        bad "confirmação de MFA falhou — resposta: $CONFIRM_RESP"
      fi
    fi
    ;;
  mfa_required)
    bad "conta já tinha MFA cadastrado de uma execução anterior sem chave salva — rode com um Bootstrap__AdminEmail novo ou reset o banco"
    ;;
  *)
    bad "login não retornou status esperado (veio '$STATUS') — resposta: $LOGIN_RESP"
    ;;
esac

ME_CODE=$(curl -sS -o /dev/null -w '%{http_code}' -b "$COOKIE_JAR" "$API_BASE/api/v1/Auth/me")
check_eq "sessão confirmada em /Auth/me" 200 "$ME_CODE"

# ── 5. Navegação entre módulos — catálogo autenticado ────────────────────
# Módulos nascem provisionados porém DESABILITADOS (§2.3) — precisa habilitar
# "care" antes de qualquer verificação de catálogo ter algo pra encontrar.
log "5/8 navegação entre módulos"
ADMIN_MODULES=$(curl -sS -b "$COOKIE_JAR" "$API_BASE/api/v1/AdminInstitutionModule")
CARE_ID=$(python3 -c "import sys,json; m=[x for x in json.loads(sys.argv[1]) if x['moduleKey']=='care']; print(m[0]['id'] if m else '')" "$ADMIN_MODULES" 2>/dev/null || echo "")
CARE_ROWVERSION=$(python3 -c "import sys,json; m=[x for x in json.loads(sys.argv[1]) if x['moduleKey']=='care']; print(m[0]['rowVersion'] if m else '')" "$ADMIN_MODULES" 2>/dev/null || echo "")

if [ -z "$CARE_ID" ]; then
  bad "não encontrou o InstitutionModule 'care' via AdminInstitutionModule — resposta: $ADMIN_MODULES"
else
  ENABLE_RESP=$(curl -sS -b "$COOKIE_JAR" -X PUT "$API_BASE/api/v1/AdminInstitutionModule/$CARE_ID" \
    -H "Content-Type: application/json" \
    -d "{\"isEnabled\":true,\"order\":1,\"operationalState\":0,\"rowVersion\":$CARE_ROWVERSION}")
  CARE_ROWVERSION=$(python3 -c "import sys,json; print(json.loads(sys.argv[1]).get('rowVersion',''))" "$ENABLE_RESP" 2>/dev/null || echo "$CARE_ROWVERSION")
fi

CATALOG=$(curl -sS -b "$COOKIE_JAR" "$API_BASE/api/v1/me/modules")
CATALOG_KEYS=$(python3 -c "import sys,json; print(','.join(m['key'] for m in json.loads(sys.argv[1])))" "$CATALOG" 2>/dev/null || echo "")
if [ -n "$CATALOG_KEYS" ]; then
  ok "catálogo retornou módulo(s) após habilitar: $CATALOG_KEYS"
else
  bad "catálogo vazio ou resposta inesperada: $CATALOG"
fi

# ── 6. Manutenção — muda o estado de um módulo e confirma no catálogo ───
log "6/8 estado de manutenção"
if [ -z "$CARE_ID" ]; then
  bad "sem CARE_ID (passo anterior falhou) — pulando teste de manutenção"
else
  PUT_RESP=$(curl -sS -b "$COOKIE_JAR" -X PUT "$API_BASE/api/v1/AdminInstitutionModule/$CARE_ID" \
    -H "Content-Type: application/json" \
    -d "{\"isEnabled\":true,\"order\":1,\"operationalState\":1,\"operationalMessage\":\"Em manutenção (smoke test).\",\"rowVersion\":$CARE_ROWVERSION}")

  CATALOG_AFTER=$(curl -sS -b "$COOKIE_JAR" "$API_BASE/api/v1/me/modules")
  CARE_STATE=$(python3 -c "import sys,json; m=[x for x in json.loads(sys.argv[1]) if x['key']=='care']; print(m[0]['operationalState'] if m else '')" "$CATALOG_AFTER" 2>/dev/null || echo "")
  if [ "$CARE_STATE" = "1" ]; then
    ok "módulo 'care' aparece em MAINTENANCE (1) no catálogo após a alteração"
  else
    bad "módulo 'care' não refletiu MAINTENANCE no catálogo — resposta: $CATALOG_AFTER"
  fi

  # Reverte pra não deixar a stack de smoke test num estado alterado.
  NEW_ROWVERSION=$(python3 -c "import sys,json; print(json.loads(sys.argv[1]).get('rowVersion',''))" "$PUT_RESP" 2>/dev/null || echo "")
  if [ -n "$NEW_ROWVERSION" ]; then
    curl -sS -b "$COOKIE_JAR" -X PUT "$API_BASE/api/v1/AdminInstitutionModule/$CARE_ID" \
      -H "Content-Type: application/json" \
      -d "{\"isEnabled\":true,\"order\":1,\"operationalState\":0,\"rowVersion\":$NEW_ROWVERSION}" >/dev/null
  fi
fi

# ── 7. Logout ──────────────────────────────────────────────────────────
log "7/8 logout"
LOGOUT_CODE=$(curl -sS -o /dev/null -w '%{http_code}' -b "$COOKIE_JAR" -X POST "$API_BASE/api/v1/Auth/logout")
check_eq "POST /Auth/logout" 200 "$LOGOUT_CODE"
ME_AFTER_LOGOUT=$(curl -sS -o /dev/null -w '%{http_code}' -b "$COOKIE_JAR" "$API_BASE/api/v1/Auth/me")
check_eq "/Auth/me após logout" 401 "$ME_AFTER_LOGOUT"

# ── 8. Falha segura do IAM — API fora do ar não derruba os front-ends ───
log "8/8 falha segura do IAM"
if docker ps --format '{{.Names}}' 2>/dev/null | grep -q '^seniorcare-api$'; then
  docker stop seniorcare-api >/dev/null
  sleep 2

  STATIC_CODE=$(http_code "$PORTAL_BASE/")
  check_eq "shell estático do portal continua de pé com a API fora do ar" 200 "$STATIC_CODE"

  API_PROXY_CODE=$(http_code "$PORTAL_BASE/api/v1/me/modules")
  case "$API_PROXY_CODE" in
    502|503|504)
      ok "chamada a /api via portal falha de forma segura (HTTP $API_PROXY_CODE), sem pendurar nem responder com sucesso falso"
      ;;
    *)
      bad "esperado 502/503/504 com a API fora do ar, veio HTTP $API_PROXY_CODE"
      ;;
  esac

  docker start seniorcare-api >/dev/null
  log "aguardando API voltar..."
  for _ in $(seq 1 60); do
    curl -fsS "$API_BASE/health/ready" >/dev/null 2>&1 && break
    sleep 2
  done
  curl -fsS "$API_BASE/health/ready" >/dev/null 2>&1 && ok "API voltou ao ar (stack restaurada)" || bad "API não voltou ao ar depois do restart"
else
  bad "container seniorcare-api não encontrado — pulei o teste de falha segura do IAM"
fi

# ── Resumo ────────────────────────────────────────────────────────────
echo ""
log "resumo: $PASS OK, $FAIL falha(s)"
[ "$FAIL" -eq 0 ]
