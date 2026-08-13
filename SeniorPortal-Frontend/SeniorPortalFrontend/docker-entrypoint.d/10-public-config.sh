#!/bin/sh
set -eu

# §8.1/§4.1 — gera public-config.json a partir de PUBLIC_NAME em runtime,
# sobrescrevendo o arquivo padrão de build (public/public-config.json,
# fallback "SeniorCare"). Nunca recompila o bundle — só troca o JSON
# estático servido ao lado dele (contrato de
# docs/architecture/senior-portal-contracts.md §1: "nome público... injetado
# em runtime, nunca no bundle"). Roda como parte dos scripts de entrypoint
# padrão da imagem oficial nginx (/docker-entrypoint.d/), executados antes
# do nginx subir.
PUBLIC_NAME="${PUBLIC_NAME:-SeniorCare}"

# Escapa aspas e barra invertida pra não quebrar o JSON — PUBLIC_NAME é
# config de operador (variável de ambiente do deploy), não entrada de
# usuário final; a escapagem aqui é sobre validade do JSON, não sobre XSS
# (o React já escapa o valor ao renderizar).
ESCAPED_NAME=$(printf '%s' "$PUBLIC_NAME" | sed 's/\\/\\\\/g; s/"/\\"/g')

cat > /usr/share/nginx/html/public-config.json <<EOF
{"publicName":"${ESCAPED_NAME}"}
EOF
