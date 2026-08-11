#!/usr/bin/env bash
# bootstrap-dev-admin.sh — automatiza o primeiro acesso (ativação + MFA) em
# ambiente de desenvolvimento, sem depender de e-mail nem de QR code (nenhum
# dos dois existe na plataforma ainda — ver
# ../deploy/BOOTSTRAP.md#pendências-conhecidas).
#
# Faz de ponta a ponta o que o tutorial descreve manualmente: espera a API
# ficar pronta, captura o token de ativação, ativa a conta, loga, cadastra o
# MFA (calculando o código TOTP sozinho) e confirma a sessão — sem abrir
# navegador, sem digitar nada num app autenticador. Não contorna nem enfraquece
# o MFA (continua obrigatório, sem exceção) — só automatiza os passos que já
# existem, exatamente como um humano faria via curl.
#
# Uso:
#   ./bootstrap-dev-admin.sh                    # Docker (infra/docker-test), auto-detecta o token
#   ./bootstrap-dev-admin.sh --token <token>    # backend rodando fora de container (IDE) — cole o
#                                                 # token impresso no console da IDE
#
# Variáveis de ambiente (todas opcionais, com default):
#   API_BASE             default http://localhost:8080
#   DEV_ADMIN_EMAIL       default admin@example.com — precisa bater com Bootstrap__AdminEmail do .env
#   DEV_ADMIN_PASSWORD    default DevSenhaForte!2026
#
# Idempotente — cobre os 3 estados possíveis do login:
#   "ok"                     conta já ativa e MFA já cadastrado, nada a fazer além de confirmar.
#   "mfa_enrollment_required" primeiro login: cadastra o MFA agora (enroll + confirm).
#   "mfa_required"            MFA já cadastrado em execução anterior: usa a chave salva em
#                             .dev-admin-mfa-key (por este mesmo script, não versionado) pra
#                             calcular o código e completar o login sem repetir o cadastro.

set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
KEY_FILE="$SCRIPT_DIR/.dev-admin-mfa-key"

API_BASE="${API_BASE:-http://localhost:8080}"
DEV_ADMIN_EMAIL="${DEV_ADMIN_EMAIL:-admin@example.com}"
DEV_ADMIN_PASSWORD="${DEV_ADMIN_PASSWORD:-DevSenhaForte!2026}"
COOKIE_JAR=$(mktemp)
trap 'rm -f "$COOKIE_JAR"' EXIT

log() { printf '\033[1;34m==>\033[0m %s\n' "$*"; }
err() { printf '\033[1;31mERRO:\033[0m %s\n' "$*" >&2; }
die() { err "$*"; exit 1; }

# TOTP padrão (HMAC-SHA1, 6 dígitos, janela de 30s) — mesmo algoritmo que
# ASP.NET Core Identity usa (TokenOptions.DefaultAuthenticatorProvider).
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

TOKEN=""
if [ "${1:-}" = "--token" ]; then
  TOKEN="${2:-}"
  [ -n "$TOKEN" ] || die "uso: $(basename "$0") --token <token>"
fi

# ── 1. Esperar a API ficar pronta ────────────────────────────────────────
log "aguardando $API_BASE/health/ready..."
for _ in $(seq 1 60); do
  if curl -fsS "$API_BASE/health/ready" >/dev/null 2>&1; then
    break
  fi
  sleep 2
done
curl -fsS "$API_BASE/health/ready" >/dev/null 2>&1 || die "API não ficou pronta em $API_BASE — confira se está rodando"
log "API pronta."

# ── 2. Capturar o token de ativação (se não foi passado) ────────────────
if [ -z "$TOKEN" ]; then
  if docker ps --format '{{.Names}}' 2>/dev/null | grep -q '^seniorcare-api$'; then
    TOKEN=$(docker logs seniorcare-api 2>&1 | grep "Token de ativação" | tail -1 | sed 's/^.*: //') || true
  fi
fi

# ── 3. Ativar a conta (pula se já não estiver mais PROVISIONED) ─────────
if [ -n "$TOKEN" ]; then
  log "ativando conta ($DEV_ADMIN_EMAIL)..."
  ACTIVATE_RESP=$(curl -sS -o /dev/null -w '%{http_code}' -X POST "$API_BASE/api/v1/Auth/activate" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$DEV_ADMIN_EMAIL\",\"token\":\"$TOKEN\",\"newPassword\":\"$DEV_ADMIN_PASSWORD\"}")
  if [ "$ACTIVATE_RESP" = "200" ]; then
    log "conta ativada."
  else
    log "ativação retornou HTTP $ACTIVATE_RESP — provavelmente a conta já estava ativa antes; seguindo pro login."
  fi
else
  log "sem token novo pra ativar (nenhum container seniorcare-api encontrado e --token não foi passado) — assumindo que a conta já está ativa e tentando login direto."
fi

# ── 4. Login ──────────────────────────────────────────────────────────────
log "logando..."
LOGIN_RESP=$(curl -sS -c "$COOKIE_JAR" -X POST "$API_BASE/api/v1/Auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$DEV_ADMIN_EMAIL\",\"password\":\"$DEV_ADMIN_PASSWORD\"}")

STATUS=$(json_get "$LOGIN_RESP" status)

case "$STATUS" in
  ok)
    log "login completo — MFA já cadastrado, sessão ativa. Nada mais a fazer."
    log "e-mail: $DEV_ADMIN_EMAIL | senha: $DEV_ADMIN_PASSWORD"
    exit 0
    ;;
  mfa_required)
    # MFA já cadastrado numa execução anterior deste script — usa a chave salva
    # pra calcular o código e completar o login (não precisa repetir o enroll).
    [ -f "$KEY_FILE" ] || die "login pediu MFA (já cadastrado), mas $KEY_FILE não existe — rode sem --token uma vez com a conta recém-criada, ou cadastre o MFA manualmente (ver tutorial) e crie esse arquivo com a chave."
    AUTHENTICATOR_KEY=$(cat "$KEY_FILE")
    CHALLENGE_TOKEN=$(json_get "$LOGIN_RESP" challengeToken)
    CODE=$(totp "$AUTHENTICATOR_KEY")
    MFA_RESP=$(curl -sS -c "$COOKIE_JAR" -X POST "$API_BASE/api/v1/Auth/login/mfa" \
      -H "Content-Type: application/json" \
      -d "{\"challengeToken\":\"$CHALLENGE_TOKEN\",\"code\":\"$CODE\"}")
    [ "$(json_get "$MFA_RESP" status)" = "ok" ] || die "login/mfa não retornou 'ok' — resposta: $MFA_RESP"
    log "login completo (MFA confirmado com a chave salva)."
    log "e-mail: $DEV_ADMIN_EMAIL | senha: $DEV_ADMIN_PASSWORD"
    exit 0
    ;;
  mfa_enrollment_required)
    log "MFA ainda não cadastrado — cadastrando agora."
    ;;
  *)
    die "login não retornou 'ok', 'mfa_required' nem 'mfa_enrollment_required' (retornou: '$STATUS') — resposta: $LOGIN_RESP"
    ;;
esac

CHALLENGE_TOKEN=$(json_get "$LOGIN_RESP" challengeToken)

# ── 5. Cadastrar MFA (enroll + confirm, calculando o TOTP sozinho) ──────
ENROLL_RESP=$(curl -sS -c "$COOKIE_JAR" -b "$COOKIE_JAR" -X POST "$API_BASE/api/v1/Auth/mfa/enroll" \
  -H "Content-Type: application/json" \
  -d "{\"challengeToken\":\"$CHALLENGE_TOKEN\"}")
AUTHENTICATOR_KEY=$(json_get "$ENROLL_RESP" authenticatorKey)
[ -n "$AUTHENTICATOR_KEY" ] || die "enroll de MFA não devolveu authenticatorKey — resposta: $ENROLL_RESP"

CODE=$(totp "$AUTHENTICATOR_KEY")

CONFIRM_RESP=$(curl -sS -c "$COOKIE_JAR" -b "$COOKIE_JAR" -X POST "$API_BASE/api/v1/Auth/mfa/confirm" \
  -H "Content-Type: application/json" \
  -d "{\"challengeToken\":\"$CHALLENGE_TOKEN\",\"code\":\"$CODE\"}")

if ! python3 -c "import sys,json; json.loads(sys.argv[1])['identity']" "$CONFIRM_RESP" >/dev/null 2>&1; then
  die "confirmação de MFA falhou — resposta: $CONFIRM_RESP"
fi

# Salva a chave pra próximas execuções não precisarem recadastrar o MFA — só
# faz sentido em ambiente de dev (nunca faça isso com uma conta real).
printf '%s' "$AUTHENTICATOR_KEY" > "$KEY_FILE"
chmod 600 "$KEY_FILE"

log "MFA cadastrado e login completo."
echo ""
echo "  e-mail:            $DEV_ADMIN_EMAIL"
echo "  senha:              $DEV_ADMIN_PASSWORD"
echo "  chave autenticador: $AUTHENTICATOR_KEY  (salva em $KEY_FILE pra próximas execuções)"
echo ""
log "pronto — logue no front-end com o e-mail/senha acima e o código do autenticador (ou recalcule com a chave)."
