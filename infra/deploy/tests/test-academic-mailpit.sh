#!/usr/bin/env bash
set -euo pipefail

NAME="seniorcare-mailpit-test-$$"
cleanup() { docker rm -f "$NAME" >/dev/null 2>&1 || true; }
trap cleanup EXIT

docker run -d --rm --name "$NAME" -p 127.0.0.1::1025 -p 127.0.0.1::8025 axllent/mailpit:v1.27.3 >/dev/null
SMTP_PORT=$(docker port "$NAME" 1025/tcp | sed 's/.*://')
HTTP_PORT=$(docker port "$NAME" 8025/tcp | sed 's/.*://')

for _ in $(seq 1 20); do
  curl --noproxy '*' --silent --fail "http://127.0.0.1:${HTTP_PORT}/api/v1/messages" >/dev/null 2>&1 && break
  sleep 1
done

python3 - "$SMTP_PORT" <<'PY'
import smtplib
import sys
from email.message import EmailMessage

message = EmailMessage()
message["From"] = "noreply@seniorcare.example.test"
message["To"] = "aluno@seniorcare.example.test"
message["Subject"] = "Ativação acadêmica"
message.set_content("Ative sua conta: https://seniorcare.test:8443/ativar-conta?token=ficticio")
with smtplib.SMTP("127.0.0.1", int(sys.argv[1])) as smtp:
    smtp.send_message(message)
PY

messages=$(curl --noproxy '*' --silent --fail "http://127.0.0.1:${HTTP_PORT}/api/v1/messages")
printf '%s' "$messages" | grep -q 'aluno@seniorcare.example.test'
printf '%s' "$messages" | grep -q 'https://seniorcare.test:8443/ativar-conta'
printf 'OK: Mailpit capturou ativação sintética com origem HTTPS acadêmica.\n'
